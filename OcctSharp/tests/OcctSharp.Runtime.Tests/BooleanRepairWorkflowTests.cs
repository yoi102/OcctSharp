using OcctSharp.Tests.Shared;
namespace OcctSharp.Runtime.Tests;
public sealed class BooleanRepairWorkflowTests {
 [Theory][InlineData(TopologyBooleanOperation.Fuse)][InlineData(TopologyBooleanOperation.Cut)][InlineData(TopologyBooleanOperation.Common)]
 public void BooleanRepairExplicitEditStepAndActualViewerRemainIndependent(TopologyBooleanOperation operation)=>BatchBYBooleanRepairWorkflow.Run(operation);
 [Fact] public void ActualSdkFailureTextSurvivesSourceAndResultClosure()=>BatchBYBooleanRepairWorkflow.FailureDiagnostics();
 [Fact] public void ActualSdkWarningTextSurvivesSourceAndResultClosure()=>BatchBYBooleanRepairWorkflow.WarningDiagnostics();
 [Fact] public void CompositionKeepsEverySuccessorAndRejectsNoPathByPreference(){var first=new RepairIdentity(Guid.NewGuid(),0);var second=new RepairIdentity(Guid.NewGuid(),1);var third=new RepairIdentity(Guid.NewGuid(),2);RepairSelection start=new(first,0),a=new(second,1),b=new(second,2),x=new(third,3),y=new(third,4),z=new(third,5);var previous=new RepairHistoryRelation[]{new(start,a,RepairRelationKind.Modified),new(start,b,RepairRelationKind.Modified)};var next=new RepairHistoryRelation[]{new(a,x,RepairRelationKind.Modified),new(a,y,RepairRelationKind.Generated),new(b,z,RepairRelationKind.Modified),new(b,null,RepairRelationKind.Deleted)};var result=ShapeRepair.Compose(previous,next);Assert.Equal(4,result.Count);Assert.Equal(new[]{x,y,z},result.Where(r=>r.Result.HasValue).Select(r=>r.Result!.Value));Assert.Contains(result,r=>r.Source==start&&r.Kind==RepairRelationKind.Deleted&&r.Result is null);Assert.All(result,r=>Assert.Equal(start,r.Source));}
}
