using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Xunit;

namespace OcctSharp.Runtime.Tests;

public sealed class ExactOffsetStepUnitRejectionTests
{
    [Theory]
    [InlineData("missing-length", "lack a proved length")]
    [InlineData("duplicate-length", "repeated unit")]
    [InlineData("conflicting-length", "repeated unit")]
    [InlineData("duplicate-angle", "repeated unit")]
    [InlineData("duplicate-solid-angle", "repeated unit")]
    public void InvalidUnitsAreRejectedAcrossPublicReadersWithoutChangingSource(string kind,string expected)
    {
        var (folder,source)=Prepare();
        using(source)
        {
            string valid=ShapeExchange.WriteStep(source,Path.Combine(folder,"valid.step"));
            string text=File.ReadAllText(valid).Replace("\r\n","\n",StringComparison.Ordinal);
            string length=Id("LENGTH_UNIT"),angle=Id("PLANE_ANGLE_UNIT"),solid=Id("SOLID_ANGLE_UNIT");
            string contextPattern=@"GLOBAL_UNIT_ASSIGNED_CONTEXT\s*\(\s*\((?<units>[^)]*)\)\s*\)";
            string bad=Regex.Replace(text,contextPattern,m=>
            {
                var refs=m.Groups["units"].Value.Split(',').Select(v=>v.Trim()).ToList();
                switch(kind)
                {
                    case "missing-length":refs.Remove(length);break;
                    case "duplicate-length":refs.Add(length);break;
                    case "conflicting-length":refs.Add("#900000");break;
                    case "duplicate-angle":refs.Add(angle);break;
                    case "duplicate-solid-angle":refs.Add(solid);break;
                    default:throw new InvalidOperationException();
                }
                return "GLOBAL_UNIT_ASSIGNED_CONTEXT(("+string.Join(',',refs)+"))";
            });
            if(kind=="conflicting-length")bad=bad.Replace("ENDSEC;\nEND-ISO-10303-21;","#900000 = ( LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT($,.METRE.) );\nENDSEC;\nEND-ISO-10303-21;",StringComparison.Ordinal);
            Assert.NotEqual(text,bad);Assert.Contains("OFFSET_CURVE_3D",bad);
            if(kind=="conflicting-length")Assert.Contains("#900000 =",bad);
            string invalid=Path.Combine(folder,"invalid.step");File.WriteAllText(invalid,bad);
            byte[] fileBefore=SHA256.HashData(File.ReadAllBytes(invalid));
            byte[] sourceBefore=SourceBytes(source,folder,"before");
            Assert.Contains(expected,Assert.Throws<OcctException>(()=>{using var s=ShapeExchange.ReadStep(invalid);}).Message);
            Assert.Contains(expected,Assert.Throws<OcctException>(()=>{using var r=ShapeExchange.ReadStepWithReport(invalid);}).Message);
            using(var session=StepReadSession.Open(invalid))
            {
                for(int i=0;i<2;i++)Assert.Contains(expected,Assert.Throws<OcctException>(()=>{using var s=session.TransferRoot(0);}).Message);
            }
            Assert.Equal(fileBefore,SHA256.HashData(File.ReadAllBytes(invalid)));
            Assert.Equal(sourceBefore,SourceBytes(source,folder,"after"));
            using var retained=ShapeExchange.ReadStep(valid);Assert.True(retained.GetValidationReport(true,true).IsValid);

            string Id(string category)
            {
                var match=Regex.Match(text,@"(?<id>#\d+)\s*=\s*\([^;]*\b"+category+@"\(\)[^;]*;",RegexOptions.Singleline);
                Assert.True(match.Success,"Generated STEP lacks expected unit "+category);return match.Groups["id"].Value;
            }
        }
    }

    [Fact]
    public void OffsetUnderflowIsRejectedRepeatedlyByUnitScaledSession()
    {
        var (folder,source)=Prepare();
        using(source)
        {
            string valid=ShapeExchange.WriteStep(source,Path.Combine(folder,"valid.step"));
            string text=File.ReadAllText(valid);
            string bad=Regex.Replace(text,@"(OFFSET_CURVE_3D\('[^']*',#\d+,)[^,]+",m=>m.Groups[1].Value+"-1E-300");
            Assert.NotEqual(text,bad);string path=Path.Combine(folder,"underflow.step");File.WriteAllText(path,bad);
            byte[] before=SourceBytes(source,folder,"before");
            using var session=StepReadSession.Open(path,1e50);
            for(int i=0;i<2;i++)Assert.Contains("underflowed a read offset",Assert.Throws<OcctException>(()=>{using var s=session.TransferRoot(0);}).Message);
            Assert.Equal(before,SourceBytes(source,folder,"after"));
            Assert.Equal(bad,File.ReadAllText(path));
        }
    }
    private static (string Folder,Shape Source) Prepare()
    {
        string folder=Path.Combine(Path.GetTempPath(),"OcctSharpExactStepUnits",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        var source=ShapeExchange.ReadBrep(Path.Combine(AppContext.BaseDirectory,"Fixtures","b18-exact-step","spline-g1-normal-outward-Round.brep"));
        return (folder,source);
    }
    private static byte[] SourceBytes(Shape source,string folder,string label)=>SHA256.HashData(File.ReadAllBytes(ShapeExchange.WriteBrep(source,Path.Combine(folder,label+".brep"))));
}
