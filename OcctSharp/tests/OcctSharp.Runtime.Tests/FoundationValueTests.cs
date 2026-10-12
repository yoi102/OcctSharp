using System.Text;
using System.Runtime.InteropServices;
using OcctSharp.FoundationValues;
namespace OcctSharp.Runtime.Tests;
public sealed class FoundationValueTests {
 [Fact]public void FoundationInputsFeedActualModelingWorkflow()=>OcctSharp.Tests.Shared.BatchAVFoundationWorkflow.Run();
 [Theory][InlineData(false)][InlineData(true)]public void EmptyAndIndependentCopiedStrings(bool unicode){var a=SdkValues.Text("",unicode:unicode);Assert.True(a.IsEmpty);Assert.Equal(0,a.Length);var b=SdkValues.Text("old",TextOperation.Assign,"new",unicode:unicode);Assert.Equal("new",b.Text);Assert.Equal("",a.Text);}
 [Theory][InlineData(false)][InlineData(true)]public void StringMutationAndIndexOperationsCopyExpectedData(bool unicode){
  Assert.Equal("abcdef",SdkValues.Text("abc",TextOperation.Concatenate,"def",unicode:unicode).Text);
  Assert.Equal("abcdef",SdkValues.Text("abc",TextOperation.Append,"def",unicode:unicode).Text);
  Assert.Equal("defabc",SdkValues.Text("abc",TextOperation.Prepend,"def",unicode:unicode).Text);
  Assert.Equal("abXYc",SdkValues.Text("abc",TextOperation.Insert,"XY",3,unicode:unicode).Text);
  Assert.Equal("bc",SdkValues.Text("abcd",TextOperation.Substring,position:2,countOrEnd:3,unicode:unicode).Text);
  var split=SdkValues.Text("abcd",TextOperation.Split,position:2,unicode:unicode);Assert.Equal("ab",split.Text);Assert.Equal("cd",split.Remainder);
  Assert.Equal("ad",SdkValues.Text("abcd",TextOperation.Remove,position:2,countOrEnd:2,unicode:unicode).Text);
  Assert.Equal("ab",SdkValues.Text("abcd",TextOperation.Truncate,position:2,unicode:unicode).Text);
  Assert.Equal(98,SdkValues.Text("abc",TextOperation.CodeUnit,position:2,unicode:unicode).Number);
 }
 [Theory][InlineData(false)][InlineData(true)]public void StringSearchAndPredicatesUseSdkSemantics(bool unicode){
  Assert.Equal(2,SdkValues.Text("banana",TextOperation.Search,"an",unicode:unicode).Number);
  Assert.Equal(4,SdkValues.Text("banana",TextOperation.SearchFromEnd,"an",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abc",TextOperation.StartsWith,"ab",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abc",TextOperation.EndsWith,"bc",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abc",TextOperation.Equal,"abc",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abc",TextOperation.Different,"abd",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abd",TextOperation.Greater,"abc",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("abc",TextOperation.Less,"abd",unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("ABC",TextOperation.SameString,"abc",caseSensitive:false,unicode:unicode).Number);
 }
 [Theory][InlineData(false)][InlineData(true)]public void NumericParsingRejectsTruncatedIntegersAndOverflow(bool unicode){
  Assert.Equal(42,SdkValues.Text("42",TextOperation.ParseInteger,unicode:unicode).Number);
  Assert.Equal(-2147483648,SdkValues.Text("-2147483648",TextOperation.ParseInteger,unicode:unicode).Number);
  Assert.Equal(1.25,SdkValues.Text("1.25",TextOperation.ParseReal,unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("42",TextOperation.IsInteger,unicode:unicode).Number);
  Assert.Equal(1,SdkValues.Text("1.25",TextOperation.IsReal,unicode:unicode).Number);
  foreach(string text in new[]{"42junk","2147483648","-2147483649","abc",""})Assert.ThrowsAny<Exception>(()=>SdkValues.Text(text,TextOperation.ParseInteger,unicode:unicode));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text("1e9999",TextOperation.ParseReal,unicode:unicode));
 }
 [Fact]public void UnicodeCodeUnitsAreExplicitAndSurrogateSplittingIsRejected(){
  var copied=SdkValues.Text("汉😀字");Assert.Equal(4,copied.Length);Assert.Equal(10,copied.StorageLength);Assert.False(copied.IsAscii);
  Assert.Equal("😀",SdkValues.Text(copied.Text,TextOperation.Substring,position:2,countOrEnd:3).Text);
  var split=SdkValues.Text(copied.Text,TextOperation.Split,position:3);Assert.Equal("汉😀",split.Text);Assert.Equal("字",split.Remainder);
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text(copied.Text,TextOperation.Split,position:2));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text(copied.Text,TextOperation.Truncate,position:2));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text(copied.Text,TextOperation.Remove,position:2,countOrEnd:1));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text(copied.Text,TextOperation.Insert,"a",position:3));
 }
 [Theory][InlineData(TextOperation.Capitalize,"hELLO","Hello")][InlineData(TextOperation.LowerCase,"AbC","abc")][InlineData(TextOperation.UpperCase,"AbC","ABC")][InlineData(TextOperation.TrimStart,"  a  ","a  ")][InlineData(TextOperation.TrimEnd,"  a  ","  a")][InlineData(TextOperation.Clear,"abc","")]
 public void AsciiCaseAndTrim(TextOperation operation,string text,string expected)=>Assert.Equal(expected,SdkValues.Text(text,operation,unicode:false).Text);
 [Fact]public void AsciiPaddingReplacementTokensAndOverloadProfiles(){
  Assert.Equal(" abc ",SdkValues.Text("abc",TextOperation.Center,position:5,unicode:false).Text);
  Assert.Equal("abc  ",SdkValues.Text("abc",TextOperation.LeftJustify,position:5,unicode:false).Text);
  Assert.Equal("  abc",SdkValues.Text("abc",TextOperation.RightJustify,position:5,unicode:false).Text);
  Assert.Equal("bbb",SdkValues.Text("aAa",TextOperation.ReplaceAll,position:'a',countOrEnd:'b',caseSensitive:false,unicode:false).Text);
  Assert.Equal("bc",SdkValues.Text("aAbc",TextOperation.RemoveAll,position:'a',caseSensitive:false,unicode:false).Text);
  Assert.Equal("Abc",SdkValues.RemoveAsciiCharacter("aAbc",'a').Text);
  Assert.Equal("b",SdkValues.Text("a,b,c",TextOperation.Token,",",position:2,unicode:false).Text);
  Assert.Equal("aXbc",SdkValues.Text("abc",TextOperation.InsertAfter,"X",position:1,unicode:false).Text);
  Assert.Equal("aXbc",SdkValues.Text("abc",TextOperation.InsertBefore,"X",position:2,unicode:false).Text);
  foreach(var operation in new[]{TextOperation.EqualViewToString,TextOperation.EqualStringToView})Assert.Equal(1,SdkValues.Text("abc",operation,"abc",unicode:false).Number);
 }
 [Fact]public void InvalidInputsNeverPublishCopiedData(){
  Assert.Throws<ArgumentException>(()=>SdkValues.Text("a\0b"));Assert.Throws<ArgumentException>(()=>SdkValues.Text("汉",unicode:false));
  Assert.Throws<EncoderFallbackException>(()=>SdkValues.Text("\uD800"));Assert.Throws<ArgumentException>(()=>SdkValues.Text(new string('a',1048577)));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text("abc",TextOperation.Substring,position:0,countOrEnd:2));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Text("abc",TextOperation.UpperCase));
  Assert.ThrowsAny<Exception>(()=>SdkValues.Path("a",system:999));Assert.ThrowsAny<Exception>(()=>SdkValues.ColorFromHex("wrong"));
  Assert.ThrowsAny<Exception>(()=>SdkValues.ColorFromName("no-such-color"));Assert.ThrowsAny<Exception>(()=>SdkValues.ConvertColor(ColorConversion.LinearToSrgb,double.NaN,0,0));
 }
 [Fact]public void PurePathsReturnIndependentComponentsAndSyntax(){
  var original=SdkValues.Path("C:\\models\\part.step",system:9);var edited=SdkValues.Path(original.FullName,PathOperation.SetName,"other",9);
  Assert.Equal("part",original.Name);Assert.Equal("other",edited.Name);Assert.Equal(".step",original.Extension);
  Assert.Equal(".iges",SdkValues.Path(original.FullName,PathOperation.SetExtension,".iges",9).Extension);
  Assert.NotEmpty(SdkValues.Path(original.FullName,PathOperation.TrekValue,system:9).Projection);
  Assert.Equal(9,original.Traits.Count);
  Assert.False(original.SdkIsValid); // SDK DOS IsValid rejects colon even in drive-qualified paths.
 }
 [Fact]public void ColorConversionsAreCopiedAndRoundTrip(){
  var red=SdkValues.ColorFromName("RED");Assert.Equal(1,red.Red,6);Assert.Equal(0,red.Green,6);
  var packed=SdkValues.ColorFromArgb(unchecked((int)0xFFFF0000));Assert.Equal(red.Red,packed.Red,6);
  var rgb=SdkValues.ColorFromHex("#336699");var lab=SdkValues.ConvertColor(ColorConversion.LinearToLab,rgb.Red,rgb.Green,rgb.Blue);var lch=SdkValues.ConvertColor(ColorConversion.LabToLch,lab[0],lab[1],lab[2]);var back=SdkValues.ConvertColor(ColorConversion.LchToLab,lch[0],lch[1],lch[2]);for(int i=0;i<3;++i)Assert.Equal(lab[i],back[i],3);
  var linear=SdkValues.ConvertColor(ColorConversion.LabToLinear,back[0],back[1],back[2]);Assert.Equal(rgb.Red,linear[0],4);
  var hls=SdkValues.ConvertColor(ColorConversion.LinearToHls,rgb.Red,rgb.Green,rgb.Blue);var srgb=SdkValues.ConvertColor(ColorConversion.HlsToSrgb,hls[0],hls[1],hls[2]);Assert.Equal(.2,srgb[0],4);
  var hls2=SdkValues.ConvertColor(ColorConversion.SrgbToHls,srgb[0],srgb[1],srgb[2]);var linear2=SdkValues.ConvertColor(ColorConversion.HlsToLinear,hls2[0],hls2[1],hls2[2]);Assert.Equal(rgb.Red,linear2[0],4);
 }
 [Fact]public void UnitValidationDistinguishesZeroInvalidAndIncompatible(){
  Assert.Equal(0,SdkValues.ConvertUnits(0,"mm","m").Value);Assert.Equal(.001,SdkValues.ToSi(1,"mm").Value,10);Assert.Equal(1000,SdkValues.FromSi(1,"mm").Value,8);
  Assert.ThrowsAny<Exception>(()=>SdkValues.ConvertUnits(0,"noSuchUnit","m"));Assert.ThrowsAny<Exception>(()=>SdkValues.ConvertUnits(0,"m","s"));Assert.ThrowsAny<Exception>(()=>SdkValues.ToSi(double.PositiveInfinity,"m"));
  foreach(string text in new[]{"m/","()","(m","m**","m**9999","m//s"})Assert.ThrowsAny<Exception>(()=>SdkValues.ToSi(0,text));
  Assert.Equal(1,SdkValues.ConvertUnits(1000,"mm/s","m/s").Value,8);
  for(int kind=0;kind<10;++kind){var d=SdkValues.Dimension(kind);Assert.Equal(9,d.Exponents.Count);if(kind==0)Assert.All(d.Exponents,x=>Assert.Equal(0,x));else Assert.Equal(1,d.Exponents[kind-1]);}
  Assert.Equal(1,SdkValues.QuantityDimension("LENGTH").Exponents[1]);
 }
 [Fact]public void IndependentPrivateCopiesSupportConcurrentCalls(){Parallel.For(0,32,i=>{string suffix=i.ToString(System.Globalization.CultureInfo.InvariantCulture);Assert.Equal("汉😀"+suffix,SdkValues.Text("汉😀",TextOperation.Concatenate,suffix).Text);Assert.Equal(.001,SdkValues.ConvertUnits(1,"mm","m").Value,10);});}
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)]unsafe delegate int ExecuteRaw(int family,int operation,byte* text,int size,byte* other,int otherSize,int p,int q,int flags,double* values,int count,out nint token);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)]unsafe delegate int TextRaw(nint token,int index,byte* buffer,int capacity,out int written);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)]delegate void ReleaseRaw(nint token);
 [Fact]public unsafe void RawInvalidUtf8CapacityAndStaleTokensAreContained(){
  _=OcctRuntime.Info;nint library=NativeLibrary.Load(System.IO.Path.Combine(AppContext.BaseDirectory,"occt","OcctSharp.Native.dll"));
  try{
   var execute=Marshal.GetDelegateForFunctionPointer<ExecuteRaw>(NativeLibrary.GetExport(library,"occtsharp_foundation_values_execute"));
   var text=Marshal.GetDelegateForFunctionPointer<TextRaw>(NativeLibrary.GetExport(library,"occtsharp_foundation_values_text"));
   var release=Marshal.GetDelegateForFunctionPointer<ReleaseRaw>(NativeLibrary.GetExport(library,"occtsharp_foundation_values_release"));
   byte* bad=stackalloc byte[4]{0xF0,0x80,0x80,0x80};Assert.NotEqual(0,execute(1,0,bad,4,null,0,0,32,1,null,0,out nint failed));Assert.Equal(0,failed);
   byte* bytes=stackalloc byte[3]{97,98,99};Assert.Equal(0,execute(0,0,bytes,3,null,0,0,32,1,null,0,out nint token));
   try{byte* output=stackalloc byte[3]{99,99,99};Assert.NotEqual(0,text(token,0,output,2,out _));Assert.Equal((byte)99,output[0]);Assert.Equal(0,text(token,0,output,3,out int written));Assert.Equal(3,written);Assert.Equal((byte)97,output[0]);Assert.NotEqual(0,text(token,999,null,0,out _));}
   finally{release(token);}Assert.NotEqual(0,text(token,0,null,0,out _));release(token);
  }finally{NativeLibrary.Free(library);}
 }
}
