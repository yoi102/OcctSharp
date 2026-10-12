using System.Text.Json;
using OcctSharp.DocumentProjections;
using OcctSharp.StepCafWorkspaces;
using OcctSharp.XdeMetadataWorkspaces;

namespace OcctSharp.Tests.Shared;

internal static class BatchCDStepWorkflow
{
    static readonly JsonSerializerOptions ReportOptions=new(){WriteIndented=true};
    static readonly string[] PartLayers=["尺寸层🙂"];
    static readonly Dictionary<string,string[]> SemanticFields=new(){
        ["Dimension"]=["Type","Values","LeftDecimalPlaces","RightDecimalPlaces","FirstPoint","SecondPoint","TextPosition","SemanticName","Description","DescriptionName"],
        ["Tolerance"]=["Type","TypeOfValue","Value","SemanticName"],
        ["Datum"]=["Name","Description","Identification","SemanticName","Position"]};
    internal sealed record Field(string Group, string Name, JsonElement Expected, JsonElement Observed, string Fidelity);
    internal sealed record Report(string Schema, string Domain, string Sha256, IReadOnlyList<Field> Fields,double Volume,int FreeRoots,bool SourceDeleted,bool SourceOwnerDisposed);
    static JsonElement Json(object? value) => JsonSerializer.SerializeToElement(value);
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static void Near(double expected, double observed, string message) => Require(Math.Abs(expected-observed) < 1e-6, message);
    static void ValidateReport(Report report)
    {
        using var ledger=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","ConfiguredStep","fidelity.json")));
        var profile=ledger.RootElement.GetProperty("profiles").GetProperty(report.Schema+"-"+report.Domain);
        Require(profile.EnumerateObject().Count()==report.Fields.Count,"STEP fixture field catalogue changed.");
        foreach(var field in report.Fields)Require(profile.GetProperty(field.Group+"."+field.Name).GetString()==field.Fidelity,"STEP field fidelity changed: "+field.Group+"."+field.Name);
    }
    static bool Equal(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind) return false;
        if (a.ValueKind == JsonValueKind.Number) return Math.Abs(a.GetDouble()-b.GetDouble()) < 1e-6;
        if (a.ValueKind == JsonValueKind.Array) return a.GetArrayLength()==b.GetArrayLength() && a.EnumerateArray().Zip(b.EnumerateArray()).All(pair=>Equal(pair.First,pair.Second));
        if (a.ValueKind == JsonValueKind.Object) return a.EnumerateObject().All(p=>b.TryGetProperty(p.Name,out var v)&&Equal(p.Value,v)) && a.EnumerateObject().Count()==b.EnumerateObject().Count();
        return a.GetRawText()==b.GetRawText();
    }
    static void Compare(List<Field> fields,string group,object expected,object? observed)
    {
        var a=Json(expected);var b=Json(observed);
        foreach(var p in a.EnumerateObject())
        {
            if(SemanticFields.TryGetValue(group,out var selected)&&!selected.Contains(p.Name,StringComparer.Ordinal))continue;
            // Geometry owners are separately inspected; null/empty defaults are not fidelity evidence.
            if(p.Name is "Path" or "Presentation" or "Target" || p.Value.ValueKind==JsonValueKind.Null || p.Value.ValueKind==JsonValueKind.String&&p.Value.GetString()=="" || p.Value.ValueKind==JsonValueKind.Array&&p.Value.GetArrayLength()==0)continue;
            var actual=b.ValueKind==JsonValueKind.Object&&b.TryGetProperty(p.Name,out var value)?value:Json(null);
            bool exact=p.Value.GetRawText()==actual.GetRawText();
            fields.Add(new(group,p.Name,p.Value.Clone(),actual.Clone(),exact?"supported-exact":Equal(p.Value,actual)?"supported-numeric-tolerance":"lost-or-changed"));
        }
    }
    internal static void RunNative()
    {
        string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.CD.Native."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string seed=Path.Combine(directory,"seed.step"),output=Path.Combine(directory,"配置🙂.step");
            using(var box=ShapeFactory.CreateBox(2,3,4))ShapeExchange.WriteStep(box,seed);
            using var imported=StepCafFiles.Read(seed);File.Delete(seed);
            using var context=DocumentProjectionContext.Bind(imported.Document);using var metadata=XdeMetadataContext.Bind(context);
            var label=context.GetLabel(imported.Diagnostics.FreeRoots.Single().Entry);
            imported.Document.NewCommand();metadata.SetColors([new(label,MetadataColorChannel.Surface,new(.2,.3,.4))]);Require(imported.Document.CommitCommand(),"Native color command failed.");
            var written=StepCafFiles.WriteConfigured(context,output,new(StepCafSchema.Ap242,0.0002));
            string? evidence=Environment.GetEnvironmentVariable("OCCTSHARP_CD_EVIDENCE_DIR");if(!string.IsNullOrWhiteSpace(evidence)){Directory.CreateDirectory(evidence);File.Copy(output,Path.Combine(evidence,"Ap242-Native.step"),true);}
            using var independentShape=ShapeExchange.ReadStep(output);using var reread=StepCafFiles.Read(output);File.Delete(output);imported.Dispose();Near(24,independentShape.InspectProperties(InspectionPropertyKind.Volume).Mass,"Native STEP geometry failed.");
            Require(written.File.Sha256==reread.Diagnostics.Sha256&&reread.Diagnostics.FreeRoots.Count==1,"Native configured writer/read identity failed.");
            using var readContext=DocumentProjectionContext.Bind(reread.Document);using var readMetadata=XdeMetadataContext.Bind(readContext);
            var color=readMetadata.Snapshot([readContext.GetLabel(reread.Diagnostics.FreeRoots.Single().Entry)]).Targets.Single().Surface;
            Require(color is not null,"Native STEP lost surface color.");Near(.2,color!.Red,"Native STEP red mismatch.");Near(.3,color.Green,"Native STEP green mismatch.");Near(.4,color.Blue,"Native STEP blue mismatch.");
            Require(written.AppliedSchema==StepCafSchema.Ap242&&written.AppliedPrecision==.0002,"Native applied configuration failed.");
            var nativeFields=new List<Field>();Compare(nativeFields,"Color",new MetadataColor(.2,.3,.4),color);var nativeReport=new Report("Ap242","Native",written.File.Sha256,nativeFields.AsReadOnly(),24,1,true,true);ValidateReport(nativeReport);
            if(!string.IsNullOrWhiteSpace(evidence))File.WriteAllText(Path.Combine(evidence,"Ap242-Native.json"),JsonSerializer.Serialize(nativeReport,ReportOptions));
        }
        finally{Directory.Delete(directory,true);}
    }
#if !DIRECT_MODULE_CONSUMER
    internal static Report RunFacade(StepCafSchema schema,bool nativeRewrite=false,bool cleanDuplicates=false)
    {
        string directory=Path.Combine(Path.GetTempPath(),"OcctSharp.CD.Facade."+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string output=Path.Combine(directory,"属性🙂.step");
            var dimensionDefinition=new XdeDimensionDefinition(XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_LinearDistance,[10.0])
            {SemanticName="Overall length",Description="Nominal overall length",DescriptionName="inspection",FirstPoint=GpPoint.Origin,SecondPoint=new(10,0,0),TextPosition=new(5,-3,0)};
            var toleranceDefinition=new XdeGeomToleranceDefinition{Type=XCAFDimTolObjectsGeomToleranceType.XCAFDimTolObjects_GeomToleranceType_Flatness,TypeOfValue=XCAFDimTolObjectsGeomToleranceTypeValue.XCAFDimTolObjects_GeomToleranceTypeValue_None,Value=.1,SemanticName="Flatness"};
            var datumDefinition=new XdeDatumDefinition{Name="A",Identification="A",SemanticName="Primary datum",Description="Primary support",Position=1};
            var physical=new XdeMaterial("Steel","Density record",7.85,"density","mass density");
            XdeVisualMaterial? visual=null;StepCafConfiguredDiagnostics written;
            using(var box=ShapeFactory.CreateBox(10,20,30))
            using(var document=XdeDocument.Create())
            {
                using(var transaction=document.BeginTransaction())
                {
                    var part=document.AddShape(box,"精密部件🙂");part.Color=new(.2,.3,.4);part.AddLayer("尺寸层🙂");part.Material=physical;
                    using var metadata=XdeMetadataContext.Bind(document.Projections);metadata.CreateMaterial("PBR metal",new(.2,.3,.4,.75),.4,.6,[document.Projections.GetLabel(part.Entry)]);visual=part.VisualMaterial;
                    var datum=document.CreateDatum(datumDefinition,[part]);document.CreateDimension(dimensionDefinition,[part],[part]);document.CreateGeometricTolerance(toleranceDefinition,[part],[datum]);Require(transaction.Commit(),"PMI command failed.");
                }
                File.WriteAllText(output,"existing delivery");
                try{written=StepCafFiles.WriteConfigured(document.Projections,output,new(schema,.0002,cleanDuplicates?StepCafWriteModes.All:StepCafWriteModes.All&~StepCafWriteModes.CleanDuplicates));}
                catch{Require(File.ReadAllText(output)=="existing delivery"&&Directory.GetFiles(directory).Length==1,"Failed PMI serialization changed destination or leaked staging.");throw;}
            }
            Require(written.AppliedSchema==schema&&written.AppliedPrecision==.0002,"Facade writer configuration failed.");
            string original=File.ReadAllText(output);Require(original.Contains("FILE_SCHEMA"),"Missing physical STEP schema.");
            string schemaName=schema switch{StepCafSchema.Ap203=>"CONFIG_CONTROL_DESIGN",StepCafSchema.Ap214=>"AUTOMOTIVE_DESIGN",_=>"AP242"};Require(original.Contains(schemaName,StringComparison.OrdinalIgnoreCase),"Wrong actual STEP FILE_SCHEMA: "+schema);
            if(nativeRewrite)
            {
                using var native=StepCafFiles.Read(output);using var context=DocumentProjectionContext.Bind(native.Document);File.Delete(output);
                written=StepCafFiles.WriteConfigured(context,output,new(schema,.0002,StepCafWriteModes.All&~StepCafWriteModes.CleanDuplicates));
            }
            string? evidence=Environment.GetEnvironmentVariable("OCCTSHARP_CD_EVIDENCE_DIR");string reportName=schema+"-"+(nativeRewrite?"NativeRewrite":"Facade");
            if(!string.IsNullOrWhiteSpace(evidence)){Directory.CreateDirectory(evidence);File.Copy(output,Path.Combine(evidence,reportName+".step"),true);}
            using var reopened=XdeDocument.ReadStep(output,new XdeStepReadOptions(ReadGdt:true,ReadSavedViews:true));File.Delete(output);
            var root=reopened.GetFreeShapes().Single();using(var shape=root.Shape)Near(6000,shape.InspectProperties(InspectionPropertyKind.Volume).Mass,"STEP geometry volume mismatch.");
            var fields=new List<Field>();Compare(fields,"Part",new{Name="精密部件🙂",Layers=PartLayers},new{root.Name,Layers=root.Layers.ToArray()});Compare(fields,"Color",new XdeColor(.2,.3,.4),root.Color);Compare(fields,"PhysicalMaterial",physical,root.Material);if(visual is not null)Compare(fields,"VisualMaterial",visual,root.VisualMaterial);
            using var dim=reopened.GetDimensions().FirstOrDefault()?.GetSnapshot();using var tol=reopened.GetGeometricTolerances().FirstOrDefault()?.GetSnapshot();using var dat=reopened.GetDatums().FirstOrDefault()?.GetSnapshot();
            Compare(fields,"Dimension",dimensionDefinition,dim?.Definition);Compare(fields,"Tolerance",toleranceDefinition,tol?.Definition);Compare(fields,"Datum",datumDefinition,dat?.Definition);
            if(schema==StepCafSchema.Ap242)
            {
                Require(reopened.GetDimensions().Length==1&&reopened.GetGeometricTolerances().Length==1&&reopened.GetDatums().Length==1,"AP242 semantic PMI cardinality failed.");
                Require(dim!.Definition.Type==dimensionDefinition.Type,"AP242 dimension type failed.");Near(10,dim.Definition.Values.Single(),"AP242 nominal dimension failed.");Require(tol!.Definition.Type==toleranceDefinition.Type,"AP242 tolerance type failed.");Near(.1,tol.Definition.Value,"AP242 tolerance value failed.");
                Require(dim.FirstShapeEntries.Count>0&&dim.SecondShapeEntries.Count>0&&tol.ShapeEntries.Count>0&&tol.DatumEntries.Count==1&&dat!.ShapeEntries.Count>0,"AP242 PMI association graph failed.");
                foreach(string entry in dim.FirstShapeEntries.Concat(dim.SecondShapeEntries).Concat(tol.ShapeEntries).Concat(dat!.ShapeEntries)){using var referred=reopened.GetLabel(entry).Shape;Require(!referred.IsNull,"AP242 reference points to no geometry.");}
            }
            var report=new Report(schema.ToString(),nativeRewrite?"NativeRewrite":"Facade",written.File.Sha256,fields.AsReadOnly(),6000,1,true,true);
            ValidateReport(report);
            if(!string.IsNullOrWhiteSpace(evidence))File.WriteAllText(Path.Combine(evidence,reportName+".json"),JsonSerializer.Serialize(report,ReportOptions));
            return report;
        }
        finally{Directory.Delete(directory,true);}
    }
#endif
}
