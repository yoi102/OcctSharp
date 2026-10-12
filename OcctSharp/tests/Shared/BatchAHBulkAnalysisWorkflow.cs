using OcctSharp.CopiedCollections;
using OcctSharp.Generated;
using OcctSharp.Values;
namespace OcctSharp.Tests.Shared;

internal static class BatchAHBulkAnalysisWorkflow
{
    internal static GeomBSplineCurve Curve(bool rational=false) => rational
        ? GeomBSplineCurveCollections.Create(new([new Coordinates3d(0,0,0),new(1,2,0),new(2,1,1),new(3,0,0)]),new([1.0,2,1,1]),new([0.0,1]),new([4,4]),3,false,true)
        : GeomBSplineCurveCollections.Create(new([new Coordinates3d(0,0,0),new(1,2,0),new(2,1,1),new(3,0,0)]),new([0.0,1]),new([4,4]),3,false);
    internal static double CurveError(GeomBSplineCurve source,GeomBSplineCurve target,bool derivative=false)
    {
        double max=0;
        for(int i=0;i<=40;++i){double u=0.025*i;
            if(derivative){var a=source.EvalDN(u,1);var b=target.EvalDN(u,1);max=Math.Max(max,Math.Sqrt(Math.Pow(a.X-b.X,2)+Math.Pow(a.Y-b.Y,2)+Math.Pow(a.Z-b.Z,2)));}
            else{var a=source.EvalD0(u);var b=target.EvalD0(u);max=Math.Max(max,Math.Sqrt(Math.Pow(a.X-b.X,2)+Math.Pow(a.Y-b.Y,2)+Math.Pow(a.Z-b.Z,2)));}}
        return max;
    }
    internal static void Run()
    {
        using var curve=Curve(true);using var reference=Curve(true);
        var knots=GeomBSplineCurveCollections.Knots(curve);var mults=GeomBSplineCurveCollections.Multiplicities(curve);
        if(BSplCLibCollections.NbPoles(curve.Degree(),false,mults)!=4)throw new InvalidOperationException("Pole-count analysis failed.");
        int count=BSplCLibCollections.KnotSequenceLength(mults,curve.Degree(),false);
        var flat=new CopiedArray<double>(new double[count]);BSplCLibCollections.KnotSequence(knots,mults,curve.Degree(),false,ref flat);
        var parameters=new CopiedArray<double>(new double[4]);BSplCLibCollections.BuildSchoenbergPoints(3,flat,ref parameters);
        if(Math.Abs(parameters[2]-1.0/3)>1e-12)throw new InvalidOperationException("Schoenberg analysis failed.");
        curve.InsertKnot(0.37,1,1e-12,true);curve.IncreaseDegree(5);
        if(CurveError(reference,curve)>1e-10 || CurveError(reference,curve,true)>1e-9)throw new InvalidOperationException("Bulk representation edits changed sampled geometry.");
        using var pieces=GeomConvertCollections.C0BSplineToArrayOfC1BSplineCurve(curve,1e-8,1e-6);
        using var independent=pieces.GetCurve(0);pieces.Dispose();curve.Dispose();
        if(CurveError(reference,independent)>1e-8)throw new InvalidOperationException("Split result changed geometry or depends on its owner.");
        using Shape edge=BatchAGSurfaceInterpolationWorkflow.Edge(independent);
        string path=Path.Combine(Path.GetTempPath(),"OcctSharp.AH."+Guid.NewGuid().ToString("N")+".step");
        try{ShapeExchange.WriteStep(edge,path);using var restored=ShapeExchange.ReadStep(path);if(!restored.IsValid)throw new InvalidOperationException("AH edge STEP roundtrip failed.");}finally{File.Delete(path);}
        using var first=Geom2dBSplineCurveCollections.Create(new([new Point2d(0,0),new(0.5,0),new(1,0)]),new([0.0,1]),new([3,3]),2,false);
        using var second=Geom2dBSplineCurveCollections.Create(new([new Point2d(1,0),new(1.5,0),new(2,0)]),new([0.0,1]),new([3,3]),2,false);
        using var sourceSet=OwnedCurveSet2d.Create(first,second);
        using var joined=Geom2dConvertCollections.ConcatC1(sourceSet,new([1e-8]),false,1e-8,1e-6);
        using var joinedSet=joined.Curves;using var joinedCurve=joinedSet.GetCurve(0);joined.Dispose();sourceSet.Dispose();first.Dispose();second.Dispose();
        var end=joinedCurve.EvalD0(joinedCurve.LastParameter());if(Math.Abs(end.X-2)>1e-8 || Math.Abs(end.Y)>1e-8)throw new InvalidOperationException("2D concat endpoint failed.");
        using var surface=GeomBSplineSurfaceCollections.Create(BatchAGSurfaceInterpolationWorkflow.Grid(3,4),new([0.0,1]),new([0.0,1]),new([3,3]),new([4,4]),2,3,false,false);
        var grid=GeomBSplineSurfaceCollections.Poles(surface);var packed=new CopiedArray<double>(new double[grid.Rows*grid.Columns*3]);
        BSplSLibCollections.SetPoles(grid,ref packed,false);var unpacked=new CopiedGrid<Coordinates3d>(new Coordinates3d[12],3,4);BSplSLibCollections.GetPoles(packed,ref unpacked,false);
        if(!grid.ToArray().SequenceEqual(unpacked.ToArray()))throw new InvalidOperationException("Surface packed data transposed.");
        var before=surface.EvalD0(0.4,0.6);var tangent=surface.EvalDN(0.4,0.6,1,0);surface.InsertUKnot(0.3,1,1e-12,true);surface.IncreaseDegree(4,5);
        var after=surface.EvalD0(0.4,0.6);var updatedTangent=surface.EvalDN(0.4,0.6,1,0);
        if(Math.Abs(before.Z-after.Z)>1e-10 || Math.Abs(tangent.Z-updatedTangent.Z)>1e-9)throw new InvalidOperationException("Surface representation edits changed sampled geometry.");
        using var face=BatchAGSurfaceInterpolationWorkflow.Face(surface);if(!face.IsValid)throw new InvalidOperationException("Edited surface face failed.");
        // Measured approximation: explicit samples and degree bounds, compared at the fitted physical parameter domain.
        Coordinates3d[] samples=Enumerable.Range(0,7).Select(i=>new Coordinates3d(i,2*i,0)).ToArray();
        using var fitter=GeomAPIPointsToBSplineCollections.Create(new(samples),new CopiedArray<double>(Enumerable.Range(0,7).Select(i=>(double)i).ToArray()),1,3,GeomAbsShape.GeomAbs_C1,1e-7);
        if(!fitter.IsDone())throw new InvalidOperationException("Approximation failed.");using var fitted=fitter.Curve();fitter.Dispose();
        double error=0,derivativeError=0;
        for(int i=0;i<=30;++i){double u=i/5.0;var p=fitted.EvalD0(u);var d=fitted.EvalDN(u,1);error=Math.Max(error,Math.Sqrt(Math.Pow(p.X-u,2)+Math.Pow(p.Y-2*u,2)+p.Z*p.Z));derivativeError=Math.Max(derivativeError,Math.Sqrt(Math.Pow(d.X-1,2)+Math.Pow(d.Y-2,2)+d.Z*d.Z));}
        if(error>1e-6 || derivativeError>1e-6)throw new InvalidOperationException("Measured approximation exceeds sampled position/derivative bounds.");
    }
}
