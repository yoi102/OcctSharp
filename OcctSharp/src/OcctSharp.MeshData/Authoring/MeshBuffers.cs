using System.Runtime.InteropServices;

namespace OcctSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct AuthoredVertexRaw(double X, double Y, double Z, double Nx, double Ny, double Nz,
    double U, double V, int Flags, int Reserved = 0);
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct AuthoredTriangleRaw(int A, int B, int C, int Group);

internal static class MeshBuffers
{
    internal static AuthoredVertexRaw[] Vertices(AuthoredMesh mesh) => mesh.Positions.Select((p, i) =>
    {
        MeshNormal normal = mesh.Normals?[i] ?? MeshNormal.Undefined; MeshUv uv = mesh.UVs?[i] ?? default;
        int flags = (mesh.Normals is null ? 0 : 4) | (normal.IsDefined ? 1 : 0) | (mesh.UVs is null ? 0 : 2);
        return new AuthoredVertexRaw(p.X, p.Y, p.Z, normal.X, normal.Y, normal.Z, uv.U, uv.V, flags);
    }).ToArray();
    internal static AuthoredTriangleRaw[] Triangles(IEnumerable<MeshTriangle> triangles) =>
        triangles.Select(t => new AuthoredTriangleRaw(t.A, t.B, t.C, t.Group)).ToArray();
    internal static MeshTriangle[] Triangles(IEnumerable<AuthoredTriangleRaw> triangles) =>
        triangles.Select(t => new MeshTriangle(t.A, t.B, t.C, t.Group)).ToArray();
    internal static GpPoint[] Positions(IEnumerable<AuthoredVertexRaw> vertices) => vertices.Select(v => new GpPoint(v.X, v.Y, v.Z)).ToArray();
    internal static MeshNormal[] Normals(IEnumerable<AuthoredVertexRaw> vertices) =>
        vertices.Select(v => (v.Flags & 1) != 0 ? new MeshNormal(v.Nx, v.Ny, v.Nz) : MeshNormal.Undefined).ToArray();
    internal static MeshUv[] Uvs(IEnumerable<AuthoredVertexRaw> vertices) => vertices.Select(v => new MeshUv(v.U, v.V)).ToArray();
}
