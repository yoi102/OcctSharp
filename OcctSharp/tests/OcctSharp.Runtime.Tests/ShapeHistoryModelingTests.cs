using OcctSharp;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ShapeHistoryModelingTests
{
    [Theory]
    [InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void TransformAccountsForEveryNativeSlotWithoutChangingTheSource(int mode)
    {
        using var box=ShapeFactory.CreateBox(4,3,2);using var source=RepairSnapshot.Create(box);
        using var transform=mode switch
        {
            0=>GpTrsf.Identity,1=>GpTrsf.Create(7,-2,3),2=>GpTrsf.Create(4,2,-1,2,1,3,.413),
            _=>GpTrsf.CreatePlaneMirror(3,2,-1,2,1,3)
        };
        using var result=ShapeHistoryModeling.Transform(source,transform);
        Assert.Equal(LocalFeatureOperation.Transform,result.Diagnostics.Operation);
        CheckRows(result,[source],true);Assert.Equal(source.Identity,result.Source);
        using var after=RepairSnapshot.Create(box);Assert.Equal(source.Fingerprint,after.Fingerprint);
        Assert.Equal(24,Volume(result.RequireShape()),7);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public void CompoundRetainsEachOrderedInputAndItsExactNativeMembership(bool circular)
    {
        using var box=ShapeFactory.CreateBox(4,3,2);using var source=RepairSnapshot.Create(box);
        using var transform=circular?GpTrsf.Create(9,4,0,0,0,1,.67):GpTrsf.Create(7,0,0);
        using var copied=ShapeHistoryModeling.Transform(source,transform);
        using var second=RepairSnapshot.Create(copied.RequireShape());
        using var result=ShapeHistoryModeling.Compound([source,second]);
        Assert.Equal(LocalFeatureOperation.Compound,result.Diagnostics.Operation);
        CheckRows(result,[source,second],true);Assert.Equal(2,result.RequireShape().GetTopologySummary().UniqueCounts.SolidCount);
        Assert.Equal(48,Volume(result.RequireShape()),7);
    }

    [Fact]
    public void SharedCompoundTargetRetainsBothArgumentPathsInsteadOfChoosingTheFirst()
    {
        using var box=ShapeFactory.CreateBox(4,3,2);using var source=RepairSnapshot.Create(box);
        using var result=ShapeHistoryModeling.Compound([source,source]);CheckRows(result,[source,source],true);
        foreach(var slot in Tracked(source))
        {
            var rows=result.History.Where(h=>h.Source?.TopologyIndex==slot.Selection.Index).ToArray();
            Assert.Equal(2,rows.Length);Assert.Equal([0,1],rows.Select(h=>h.Source!.Value.ArgumentIndex).Order().ToArray());
            Assert.Single(rows.Select(h=>h.ResultTopologyIndex).Distinct());
        }
    }

    [Fact]
    public void ResultAndHistoryShapesSurviveSourceAndTransformDisposal()
    {
        LocalFeatureResult result;
        using(var box=ShapeFactory.CreateBox(4,3,2))using(var source=RepairSnapshot.Create(box))using(var transform=GpTrsf.CreatePlaneMirror(0,0,0,0,0,1))
            result=ShapeHistoryModeling.Transform(source,transform);
        using(result)
        {
            Assert.True(result.RequireShape().GetValidationReport(true,true).IsValid);
            foreach(var row in result.History){Assert.NotNull(row.Shape);Assert.True(row.Shape!.IsValid);}
            using var map=RepairSnapshot.Create(result.RequireShape());Assert.Equal(26,result.History.Count);
        }
        Assert.Throws<ObjectDisposedException>(()=>result.RequireShape());
    }

    [Theory][InlineData(false)][InlineData(true)]
    public void TwoReflectionsAndCircularSeamsKeepACompleteHistory(bool bored)
    {
        using var box=ShapeFactory.CreateBox(4,3,2);using var cylinder=ShapeFactory.CreateCylinder(.5,4);
        using var tool=cylinder.Transformed(ShapeTransform.CreateTranslation(2,1.5,-1));using var shape=bored?box.Cut(tool):box.Clone();
        using var source=RepairSnapshot.Create(shape);using var mirror=GpTrsf.CreatePlaneMirror(3,-2,1,2,1,3);
        using var first=ShapeHistoryModeling.Transform(source,mirror);CheckRows(first,[source],true);
        using var middle=RepairSnapshot.Create(first.RequireShape());using var second=ShapeHistoryModeling.Transform(middle,mirror);CheckRows(second,[middle],true);
        using var extra=second.RequireShape().Cut(shape);using var missing=shape.Cut(second.RequireShape());
        Assert.Equal(0,extra.GetTopologySummary().UniqueCounts.SolidCount);Assert.Equal(0,missing.GetTopologySummary().UniqueCounts.SolidCount);
    }

    [Fact]
    public void InvalidInputCollectionsAndNullsDoNotPublishResults()
    {
        Assert.Throws<ArgumentNullException>(()=>ShapeHistoryModeling.Compound(null!));
        Assert.Throws<ArgumentException>(()=>ShapeHistoryModeling.Compound([]));
        Assert.Throws<ArgumentException>(()=>ShapeHistoryModeling.Compound(new RepairSnapshot[257]));
        Assert.Throws<ArgumentNullException>(()=>ShapeHistoryModeling.Compound([null!]));
        using var box=ShapeFactory.CreateBox(4,3,2);using var source=RepairSnapshot.Create(box);using var identity=GpTrsf.Identity;
        Assert.Throws<ArgumentNullException>(()=>ShapeHistoryModeling.Transform(null!,identity));
        Assert.Throws<ArgumentNullException>(()=>ShapeHistoryModeling.Transform(source,null!));
    }

    [Fact]
    public void DisposedInputsAndTransformsAreRejected()
    {
        using var box=ShapeFactory.CreateBox(4,3,2);using var source=RepairSnapshot.Create(box);var transform=GpTrsf.Identity;transform.Dispose();
        Assert.Throws<ObjectDisposedException>(()=>ShapeHistoryModeling.Transform(source,transform));
        using var fresh=GpTrsf.Identity;source.Dispose();
        Assert.Throws<ObjectDisposedException>(()=>ShapeHistoryModeling.Transform(source,fresh));
        Assert.Throws<ObjectDisposedException>(()=>ShapeHistoryModeling.Compound([source]));
    }

    private static IEnumerable<RepairTopologyItem> Tracked(RepairSnapshot source)=>source.Topology.Where(t=>t.Kind is ShapeKind.Face or ShapeKind.Edge or ShapeKind.Vertex);
    private static double Volume(Shape shape)=>shape.InspectAdaptiveProperties(InspectionPropertyKind.Volume,1e-10).Properties.Mass;
    private static void CheckRows(LocalFeatureResult result,IReadOnlyList<RepairSnapshot> inputs,bool unique)
    {
        Assert.True(result.Diagnostics.AlgorithmDone);Assert.True(result.Diagnostics.ShapeIsValid);
        Assert.True(result.Diagnostics.GroupSupport.HasFlag(LocalFeatureGroupSupport.Evolution));Assert.False(result.Diagnostics.HasComposedHistory);
        using var map=RepairSnapshot.Create(result.RequireShape());
        for(int argument=0;argument<inputs.Count;argument++)foreach(var slot in Tracked(inputs[argument]))
        {
            var rows=result.History.Where(h=>h.Source is {} s&&s.PlanId==result.PlanId&&s.ArgumentIndex==argument&&s.TopologyIndex==slot.Selection.Index&&s.Kind==slot.Kind).ToArray();
            Assert.NotEmpty(rows);if(unique)Assert.Single(rows);
            foreach(var row in rows)
            {
                Assert.True(row.Kind is LocalFeatureHistoryKind.Modified or LocalFeatureHistoryKind.Unchanged);
                Assert.NotNull(row.ResultTopologyIndex);Assert.NotNull(row.Shape);
                Assert.Equal(slot.Kind,map.Topology[row.ResultTopologyIndex!.Value].Kind);
                Assert.Equal(slot.Kind,row.Shape!.Kind);
            }
        }
        Assert.Equal(inputs.Sum(i=>Tracked(i).Count()),result.History.Count);
    }
}
