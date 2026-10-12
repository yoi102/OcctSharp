using OcctSharp;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ExactTopologyIndexTests
{
    [Fact]
    public void PickedSubshapeMapsToOriginalFullTopologyWithoutCopyIdentity()
    {
        using var source=ShapeFactory.CreateBox(10,20,30);
        using var map=RepairSnapshot.Create(source);
        var edges=source.GetSubShapes(ShapeKind.Edge);
        try
        {
            foreach(var edge in edges)
            {
                int index=RepairSnapshot.FindTopologyIndex(source,edge);
                Assert.InRange(index,0,map.Topology.Count-1);
                Assert.Equal(ShapeKind.Edge,map.Topology[index].Kind);
            }
            using var foreign=ShapeFactory.CreateBox(10,20,30);
            var foreignEdges=foreign.GetSubShapes(ShapeKind.Edge);
            try{Assert.Equal(-1,RepairSnapshot.FindTopologyIndex(source,foreignEdges[0]));}
            finally{foreach(var edge in foreignEdges)edge.Dispose();}
        }
        finally{foreach(var edge in edges)edge.Dispose();}
    }

    [Fact]
    public void LocatedViewerHitMapsOnlyUniqueUnderlyingTopologyPartner()
    {
        using var source=ShapeFactory.CreateBox(10,20,30);
        var faces=source.GetSubShapes(ShapeKind.Face);
        try
        {
            using var move=GpTrsf.Create(30,0,0,0,0,1,0);
            using var location=TopLocLocation.FromTransform(move);
            using var picked=faces[0].Moved(location);
            int exact=RepairSnapshot.FindTopologyIndex(source,faces[0]);
            Assert.Equal(-1,RepairSnapshot.FindTopologyIndex(source,picked));
            Assert.Equal(exact,RepairSnapshot.FindTopologyPartnerIndex(source,picked));
            using var foreign=ShapeFactory.CreateBox(10,20,30);
            var others=foreign.GetSubShapes(ShapeKind.Face);
            try{Assert.Equal(-1,RepairSnapshot.FindTopologyPartnerIndex(source,others[0]));}
            finally{foreach(var other in others)other.Dispose();}
            using var located=source.Moved(location);
            using var repeated=ShapeFactory.CreateCompound([source,located]);
            Assert.Equal(-1,RepairSnapshot.FindTopologyPartnerIndex(repeated,picked));
        }
        finally{foreach(var face in faces)face.Dispose();}
    }
}
