using OcctSharp.Generator.Model;
using OcctSharp.Generator.Transformation;
namespace OcctSharp.Generator.Emission;

internal static class BulkCollectionGuards
{
    internal static IEnumerable<string> Emit(BindingDeclaration d)
    {
        string owner=OwnedValueOperationContract.GetOwnerName(d), member=d.NativeName[(owner.Length+2)..];
        foreach(var p in d.Parameters)
            if(BulkCollectionOperationContract.Find(p.Type) is { } c)
                yield return c.Grid ? $"Require(local{p.Position}.LowerRow() == 1 && local{p.Position}.LowerCol() == 1, \"Bulk grid operation requires one-based bounds.\");"
                    : $"Require(local{p.Position}.Lower() == 1, \"Bulk array operation requires one-based bounds.\");";
        if(owner=="BSplCLib")
        {
            switch(member)
            {
                case "Hunt": yield return "Ascending(local0, false); Require(local0.Upper() <= std::numeric_limits<int>::max()/2, \"Hunt midpoint overflows.\");"; break;
                case "FirstUKnotIndex": case "LastUKnotIndex": case "NbPoles":
                    yield return member=="NbPoles" ? "BulkMults(local2,local0);" : "BulkMults(local1,local0);";
                    if(member!="NbPoles")yield return "Require(BulkSum(local1) > local0, \"Multiplicity sum must exceed degree.\");";
                    break;
                case "FlatIndex": yield return "BulkMults(local2, local0); BulkIndex(local2, local1);"; break;
                case "PoleIndex": yield return "BulkMults(local3, local0); BulkIndex(local3, local1);"; break;
                case "MinKnotMult": case "MultForm": yield return "BulkRange(local0, local1, local2);"; break;
                case "KnotForm": yield return "Ascending(local0, true); BulkRange(local0, local1, local2);"; break;
                case "KnotAnalysis": yield return "BulkSpline(local2, local3, local0, local1);"; break;
                case "KnotSequenceLength": yield return "BulkMults(local0, local1); Require(!local2 || BSplCLib::NbPoles(local1,true,local0) >= local1+1, \"Periodic knot extension requires a full degree of poles.\");"; break;
                case "KnotSequence":
                    bool degree=d.Parameters.Count==5;
                    yield return "SameBounds(local0,local1); Minimum(local0,2); Ascending(local0,true);";
                    if(degree)yield return "BulkSpline(local0,local1,local2,local3);";
                    else yield return "Require(!local3, \"Periodic expansion requires the explicit-degree overload.\"); BulkPositiveMults(local1);";
                    yield return $"Require(local{(degree?4:2)}.Length() == BSplCLib::KnotSequenceLength(local1,{(degree?"local2":"0")},{(degree?"local3":"false")}), \"Expanded knot output size does not match.\");";
                    if(degree)yield return "Require(!local3 || BSplCLib::NbPoles(local2,true,local1) >= local2+1, \"Periodic knot extension requires enough poles.\");";
                    break;
                case "KnotsLength": yield return "Ascending(local0,false);"; break;
                case "Knots": yield return "Ascending(local0,false); SameBounds(local1,local2); Require(local1.Length()==BSplCLib::KnotsLength(local0,local3), \"Compressed knot output size does not match.\");"; break;
                case "IsRational": yield return "BulkRange(local0,local1,local2); Require(local3>=0, \"Tolerance must be nonnegative.\");"; break;
                case "BuildSchoenbergPoints": yield return "BulkDegree(local0); Ascending(local1,false); Require(local1.Length() >= 2*(local0+1) && local2.Length()==local1.Length()-local0-1, \"Schoenberg output size does not match spline poles.\");"; break;
                case "PrepareTrimming": yield return "BulkSpline(local2,local3,local0,local1); Require(local4 < local5 && local4 >= local2(BSplCLib::FirstUKnotIndex(local0,local3)) && local5 <= local2(BSplCLib::LastUKnotIndex(local0,local3)), \"Trim interval must lie within the principal parameter range.\");"; break;
                case "PrepareUnperiodize": yield return "BulkMults(local1,local0); Require(BSplCLib::NbPoles(local0,true,local1) >= local0+1, \"Periodic representation requires enough poles.\");"; break;
                case "IncreaseDegreeCountKnots": yield return "BulkMults(local3,local0); BulkDegree(local1); Require(local1>=local0 && BSplCLib::NbPoles(local0,local2,local3)>=local0+1, \"Invalid degree increase or pole count.\");"; break;
                case "PolesCoefficients": yield return "Require(local0.Length()>=2 && local0.Length()<=26, \"Bezier coefficient degree must be 1..25.\"); SameBounds(local0,local1);"; break;
                case "UnitWeights": yield return "Bounds(1,local0,false);"; break;
                default:throw new InvalidDataException("Unproved bulk guard "+d.NativeSignature);
            }
        }
        else if(owner=="BSplSLib")
        {
            switch(member)
            {
                case "UnitWeights":yield return "GridBounds(1,local0,1,local1);";break;
                case "Reverse":
                    yield return "Require(local1 >= 1 && local1 <= (local2 ? local0.ColLength() : local0.RowLength()) && static_cast<int64_t>(local1)+(local2 ? local0.ColLength() : local0.RowLength()) <= std::numeric_limits<int>::max(), \"Reverse pivot or temporary arithmetic is outside safe bounds.\");";
                    yield return "GridBounds(local2 ? 0 : 1, local0.ColLength(), local2 ? 1 : 0, local0.RowLength());";
                    break;
                case "IsRational":yield return "Require(local1>=1 && local1<=local2 && local2<=local0.UpperRow() && local3>=1 && local3<=local4 && local4<=local0.UpperCol() && local5>0, \"Invalid rationality range or tolerance.\");";break;
                case "PolesCoefficients":yield return "SameGrid(local0,local1); Require(local0.ColLength()>=2 && local0.ColLength()<=26 && local0.RowLength()>=2 && local0.RowLength()<=26, \"Bezier grid degree must be 1..25.\");";break;
                case "SetPoles":
                    bool weighted=d.Parameters.Count==4;
                    if(weighted)yield return "SameGrid(local0,local1); BulkWeights(local1);";
                    yield return $"Require(static_cast<int64_t>(local0.Size()) * {(weighted?4:3)} == local{(weighted?2:1)}.Length(), \"Packed pole output size does not match.\");";break;
                case "GetPoles":
                    bool rational=d.Parameters.Count==4;
                    if(rational)yield return "SameGrid(local1,local2);";
                    yield return $"Require(static_cast<int64_t>(local1.Size()) * {(rational?4:3)} == local0.Length(), \"Packed pole input size does not match.\");";
                    if(rational)yield return "for(int i=4;i<=local0.Length();i+=4) Require(local0(i)>gp::Resolution(), \"Packed weights must be positive.\");";
                    break;
                default:throw new InvalidDataException("Unproved bulk guard "+d.NativeSignature);
            }
        }
    }
    internal const string Support = """
inline void BulkDegree(int d) { Require(d>=1 && d<=25,"Degree must be 1..25."); }
inline int64_t BulkSum(const NCollection_Array1<int>& m) { int64_t s=0; for(int i=m.Lower();i<=m.Upper();++i)s+=m(i); return s; }
inline void BulkPositiveMults(const NCollection_Array1<int>& m) {
  Minimum(m,2); for(int i=m.Lower();i<=m.Upper();++i) Require(m(i)>0,"Multiplicities must be positive.");
  Require(BulkSum(m)<std::numeric_limits<int>::max()/128,"Multiplicity arithmetic exceeds safe limits.");
}
inline void BulkMults(const NCollection_Array1<int>& m,int d) { BulkDegree(d); BulkPositiveMults(m); for(int i=m.Lower();i<=m.Upper();++i)Require(m(i)<=d+1,"Multiplicity exceeds degree+1."); }
template<class T> void BulkIndex(const NCollection_Array1<T>& a,int i) { Require(i>=a.Lower() && i<=a.Upper(),"Index outside copied bounds."); }
template<class T> void BulkRange(const NCollection_Array1<T>& a,int f,int l) { BulkIndex(a,f); BulkIndex(a,l); Require(f<=l,"Range must increase."); }
inline void BulkSpline(const NCollection_Array1<double>& k,const NCollection_Array1<int>& m,int d,bool p) {
  SameBounds(k,m); Ascending(k,true); BulkMults(m,d); Require(BSplCLib::NbPoles(d,p,m)>=d+1,"Invalid spline representation.");
}
inline void BulkWeights(const NCollection_Array2<double>& w) { for(int i=w.LowerRow();i<=w.UpperRow();++i)for(int j=w.LowerCol();j<=w.UpperCol();++j)Require(w(i,j)>gp::Resolution(),"Weights must be positive."); }
""";
}
