using OcctSharp;

OcctRuntimeInfo runtime = OcctRuntime.Info;
if (runtime.OcctVersion != "8.0.1")
{
    throw new InvalidOperationException($"Expected OCCT 8.0.1, got '{runtime.OcctVersion}'.");
}

using Shape box = ShapeFactory.CreateBox(10, 20, 30);
if (box.IsNull || box.Kind != ShapeKind.Solid || box.FaceCount != 6)
{
    throw new InvalidOperationException("The direct Modeling package did not create a valid six-face box.");
}

if (File.Exists(Path.Combine(AppContext.BaseDirectory, "OcctSharp.dll")))
{
    throw new InvalidOperationException("The direct module consumer unexpectedly received the OcctSharp facade assembly.");
}

using RepairSnapshot repairSource = RepairSnapshot.Create(box);
using RepairPreview repair = ShapeRepair.Preview(repairSource,
    new RepairPlan(repairSource, [new("normalize solid", new SolidNormalizationRepair())],
        budget: new(MaximumRelativeVolumeChange: 1e-8)));
if (!repair.CanAccept || repair.Result?.Metrics.Volume is not double volume || Math.Abs(volume - 6000) > 1e-5)
{
    throw new InvalidOperationException("The direct Modeling repair preview failed its geometry budget.");
}
using Shape acceptedRepair = repair.Accept();
repair.Dispose(); repairSource.Dispose();
if (!acceptedRepair.IsValid || acceptedRepair.FaceCount != 6)
{
    throw new InvalidOperationException("The direct Modeling repair result did not survive preview disposal.");
}

Console.WriteLine(
    $"Direct Modeling consumer passed: {box.Kind}, {box.FaceCount} faces, OCCT {runtime.OcctVersion}.");
