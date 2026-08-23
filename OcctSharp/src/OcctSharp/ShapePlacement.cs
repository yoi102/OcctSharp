namespace OcctSharp;

/// <summary>Pairs an input shape with the rigid transform used during assembly.</summary>
public sealed record ShapePlacement(Shape Shape, ShapeTransform Transform);
