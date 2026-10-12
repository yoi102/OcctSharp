using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace OcctSharp.Packaging
{
    public sealed class InputIdentity
    {
        public string Path { get; set; }
        public string Sha256 { get; set; }
    }

    // Existing inputs stay open without write/delete sharing. The watcher also
    // detects transient additions/renames, including changes later reverted.
    // Any watcher overflow is a failure, never permission to accept a mixed pack.
    public sealed class PackageInputSnapshot : IDisposable
    {
        private readonly string root;
        private readonly FileSystemWatcher watcher;
        private readonly ConcurrentQueue<string> changes = new ConcurrentQueue<string>();
        private readonly Dictionary<string, FileStream> streams = new Dictionary<string, FileStream>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> sourcePaths;
        private bool disposed;

        public PackageInputSnapshot(string repositoryRoot)
        {
            root = System.IO.Path.GetFullPath(repositoryRoot).TrimEnd(System.IO.Path.DirectorySeparatorChar);
            watcher = new FileSystemWatcher(root) { IncludeSubdirectories = true, InternalBufferSize = 65536,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size };
            watcher.Changed += OnChange;
            watcher.Created += OnChange;
            watcher.Deleted += OnChange;
            watcher.Renamed += (s, e) => { OnChange(s, e); Track(e.OldFullPath, e.ChangeType); };
            watcher.Error += (s, e) => changes.Enqueue("Watcher error: " + e.GetException().Message);
            watcher.EnableRaisingEvents = true;
            try
            {
                sourcePaths = new HashSet<string>(EnumerateSourcePaths(), StringComparer.OrdinalIgnoreCase);
                AddFiles(sourcePaths.ToArray());
                Validate();
            }
            catch { Dispose(); throw; }
        }

        private string Relative(string path)
        {
            string full = System.IO.Path.GetFullPath(path);
            if (!full.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Package input is outside the repository: " + full);
            return full.Substring(root.Length + 1).Replace('\\', '/');
        }

        public static bool IsSourceInput(string relative)
        {
            string p = relative.Replace('\\', '/');
            // A Windows rename notification may omit the side outside this tree.
            if (string.IsNullOrWhiteSpace(p)) return false;
            string[] parts = p.Split('/');
            if (parts.Any(x => x.Equals("bin", StringComparison.OrdinalIgnoreCase) || x.Equals("obj", StringComparison.OrdinalIgnoreCase) || x == ".git" || x == ".vs")) return false;
            if (p.Equals("docs/STATUS.md", StringComparison.OrdinalIgnoreCase)) return false;
            if (parts.Length == 1) return !p.StartsWith(".", StringComparison.Ordinal) || p == ".editorconfig" || p == ".gitattributes";
            if (parts[0].Equals("docs", StringComparison.OrdinalIgnoreCase)) return true;
            if (!parts[0].Equals("OcctSharp", StringComparison.OrdinalIgnoreCase)) return false;
            if (parts.Length >= 2 && new[] { "artifacts", "build", ".vs" }.Contains(parts[1], StringComparer.OrdinalIgnoreCase)) return false;
            if (parts.Length == 2) return true;
            return new[] { "src", "eng", "config", "runtime", "assets", "packaging", "generated" }.Contains(parts[1], StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> EnumerateSourcePaths()
        {
            var files = Directory.EnumerateFiles(root).Where(p => IsSourceInput(Relative(p))).ToList();
            string workspace = System.IO.Path.Combine(root, "OcctSharp");
            if (Directory.Exists(workspace)) files.AddRange(Directory.EnumerateFiles(workspace).Where(p => IsSourceInput(Relative(p))));
            foreach (string dir in new[] { "docs", "OcctSharp/src", "OcctSharp/eng", "OcctSharp/config", "OcctSharp/runtime", "OcctSharp/assets", "OcctSharp/packaging", "OcctSharp/generated" })
            {
                string full = System.IO.Path.Combine(root, dir.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (Directory.Exists(full)) files.AddRange(Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories).Where(p => IsSourceInput(Relative(p))));
            }
            return files.OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
        }

        private void OnChange(object sender, FileSystemEventArgs e) { Track(e.FullPath, e.ChangeType); }
        private void Track(string fullPath, WatcherChangeTypes kind)
        {
            try
            {
                string relative = Relative(fullPath);
                // Directory LastWrite notifications also accompany ignored bin/obj writes.
                if (kind == WatcherChangeTypes.Changed && Directory.Exists(fullPath)) return;
                if (IsSourceInput(relative)) changes.Enqueue(kind + ": " + relative);
            }
            catch (Exception e) { changes.Enqueue("Watcher classification error: " + e.Message); }
        }

        private static string Hash(Stream stream)
        {
            stream.Position = 0;
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        public void AddFiles(string[] files)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PackageInputSnapshot));
            foreach (string path in files)
            {
                string full = System.IO.Path.GetFullPath(path);
                string relative = Relative(full);
                if (streams.ContainsKey(full)) continue;
                var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
                try { hashes.Add(full, Hash(stream)); streams.Add(full, stream); }
                catch { stream.Dispose(); throw; }
            }
        }

        public void CheckEvents()
        {
            if (disposed) throw new ObjectDisposedException(nameof(PackageInputSnapshot));
            string change;
            if (changes.TryPeek(out change)) throw new InvalidOperationException("Package inputs changed during the frozen snapshot: " + change);
        }

        public void Validate()
        {
            CheckEvents();
            var current = new HashSet<string>(EnumerateSourcePaths(), StringComparer.OrdinalIgnoreCase);
            if (!sourcePaths.SetEquals(current)) throw new InvalidOperationException("Package input path set changed during the frozen snapshot.");
            foreach (var pair in streams)
                if (!File.Exists(pair.Key) || hashes[pair.Key] != Hash(pair.Value)) throw new InvalidOperationException("Package input content changed: " + Relative(pair.Key));
            CheckEvents();
        }

        public InputIdentity[] Identities
        {
            get { return hashes.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase).Select(p => new InputIdentity { Path = Relative(p.Key), Sha256 = p.Value }).ToArray(); }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            watcher.Dispose();
            foreach (var stream in streams.Values) stream.Dispose();
        }
    }
}
