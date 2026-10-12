using OcctSharp.Generator.Emission;
using OcctSharp.Generator.Model;
namespace OcctSharp.Generator.Tests;
public sealed class UnitDiagnosticSupportTests {
 private static BindingDeclaration[] Rows()=>UnitDiagnosticSupportEmitter.Dependencies.Values.Select(op=>
  new BindingDeclaration(op.StableId,op.NativeName,op.NativeName.EndsWith("::Message_Msg",StringComparison.Ordinal)?BindingDeclarationKind.Constructor:BindingDeclarationKind.Method,"Test.hxx",1,1){NativeSignature=op.Signature,Access=BindingAccess.Public,ProductModule=OcctProductModule.Foundation}).ToArray();
 [Fact] public void ReusedDependencyCompositionIsDeterministicAndAddsOnlyManagedSupport(){
  var rows=Rows();var files=UnitDiagnosticSupportEmitter.Emit(new(rows));
  Assert.Equal(25,rows.Length);Assert.Single(files);Assert.EndsWith(".cs",files[0].RelativePath);
  Assert.Equal(files[0].Content,UnitDiagnosticSupportEmitter.Emit(new(rows.Reverse()))[0].Content);
  Assert.Equal(files[0].Content,UnitDiagnosticSupportEmitter.Emit(new(rows.Append(rows[0])))[0].Content);
  Assert.Contains("no additional SDK declaration coverage",files[0].Content);
  Assert.Empty(UnitDiagnosticSupportEmitter.Emit(new([])));
 }
 [Fact] public void MissingDuplicateUnsafeOrChangedDependenciesFailClosed(){
  var rows=Rows();Assert.Throws<InvalidDataException>(()=>UnitDiagnosticSupportEmitter.Emit(new(rows.Skip(1))));
  // BindingModel canonicalizes identical parser discoveries. Exercise an actually
  // noncanonical model instead of requiring valid duplicate discovery to fail.
  var malformed=new BindingModel(rows);var declarations=(BindingDeclaration[])malformed.Declarations;
  declarations[0]=declarations[1];
  Assert.Throws<InvalidDataException>(()=>UnitDiagnosticSupportEmitter.Emit(malformed));
  foreach(var changed in new[]{rows[0] with{NativeSignature=rows[0].NativeSignature+" "},rows[0] with{IsDeleted=true},rows[0] with{Access=BindingAccess.Protected},rows[0] with{SupportState=BindingSupportState.Manual}})
   Assert.Throws<InvalidDataException>(()=>UnitDiagnosticSupportEmitter.Emit(new(rows.Skip(1).Prepend(changed))));
 }
}
