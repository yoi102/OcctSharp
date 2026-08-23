using System.Runtime.InteropServices;
using OcctSharp.Interop;

namespace OcctSharp;

/// <summary>Provides metadata-preserving STEPCAF/XDE assembly workflows.</summary>
public static class StepAssembly
{
    /// <summary>
    /// Reads STEP files through STEPCAF, preserves their XDE label trees and metadata,
    /// places them below one assembly root, and writes a new STEP file.
    /// </summary>
    public static string WriteXde(IEnumerable<StepAssemblyInput> inputs, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        StepAssemblyInput[] items = inputs.ToArray();
        if (items.Length == 0)
        {
            throw new ArgumentException("At least one STEP input is required.", nameof(inputs));
        }

        string fullOutputPath = Path.GetFullPath(outputPath);
        string? outputDirectory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        int itemSize = Marshal.SizeOf<NativeStepAssemblyInput>();
        nint nativeItems = Marshal.AllocHGlobal(checked(itemSize * items.Length));
        List<nint> nativePaths = new(items.Length);
        try
        {
            for (int index = 0; index < items.Length; index++)
            {
                StepAssemblyInput item = items[index]
                    ?? throw new ArgumentException("STEP inputs cannot contain null entries.", nameof(inputs));
                ArgumentException.ThrowIfNullOrWhiteSpace(item.FilePath);
                string fullInputPath = Path.GetFullPath(item.FilePath);
                if (!File.Exists(fullInputPath))
                {
                    throw new FileNotFoundException("The STEP input file does not exist.", fullInputPath);
                }

                nint nativePath = Marshal.StringToCoTaskMemUTF8(fullInputPath);
                nativePaths.Add(nativePath);
                Marshal.StructureToPtr(
                    new NativeStepAssemblyInput(nativePath, item.Transform),
                    nativeItems + index * itemSize,
                    fDeleteOld: false);
            }

            OcctRuntime.EnsureCompatible();
            NativeError.ThrowIfFailed(
                NativeMethods.MergeStepXde(nativeItems, items.Length, fullOutputPath),
                "step_merge_xde");
            return fullOutputPath;
        }
        finally
        {
            foreach (nint nativePath in nativePaths)
            {
                Marshal.FreeCoTaskMem(nativePath);
            }

            Marshal.FreeHGlobal(nativeItems);
        }
    }
}
