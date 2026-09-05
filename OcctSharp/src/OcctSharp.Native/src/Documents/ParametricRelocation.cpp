#include "Documents/Parametric.hxx"
#include "Runtime/Validation.hxx"
#include <TDF_AttributeIterator.hxx>
#include <TDF_ChildIterator.hxx>
#include <TDF_CopyTool.hxx>
#include <TDF_DataSet.hxx>
#include <TDF_RelocationTable.hxx>
#include <vector>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Parametric;

OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_relocate(
  const OcctSharp_OcafDocumentHandle* document, const char* const* sources,
  const char* const* destinations, int32_t count, int32_t retain_external)
{
  return Guard([&] {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    Require(count > 0 && count <= 4096 && (retain_external == 0 || retain_external == 1), "Invalid relocation request.");
    ValidateArray(sources, count, "Missing source entries."); ValidateArray(destinations, count, "Missing destination entries.");
    std::vector<TDF_Label> from, to;
    for (int i = 0; i < count; ++i) { from.push_back(ResolveOcafLabel(document, sources[i])); to.push_back(ResolveOcafLabel(document, destinations[i])); }
    for (int i = 0; i < count; ++i) {
      Require(!to[i].HasAttribute() && !to[i].HasChild(), "Relocation destinations must be empty.");
      for (int j = 0; j < count; ++j) {
        Require(from[i] != to[j] && !from[i].IsDescendant(to[j]) && !to[j].IsDescendant(from[i]), "Source and destination trees overlap.");
        if (i != j) {
          Require(from[i] != from[j] && !from[i].IsDescendant(from[j]), "Source roots overlap.");
          Require(to[i] != to[j] && !to[i].IsDescendant(to[j]), "Destination roots overlap.");
        }
      }
    }
    occ::handle<TDF_DataSet> data = new TDF_DataSet;
    occ::handle<TDF_RelocationTable> relocation = new TDF_RelocationTable(retain_external != 0);
    auto add = [&](const TDF_Label& label) {
      data->AddLabel(label);
      for (TDF_AttributeIterator it(label); it.More(); it.Next()) data->AddAttribute(it.Value());
    };
    for (int i = 0; i < count; ++i) {
      // OCCT AddRoot only appends a label already present in the data set.
      add(from[i]); data->AddRoot(from[i]); relocation->SetRelocation(from[i], to[i]);
      for (TDF_ChildIterator it(from[i], true); it.More(); it.Next()) add(it.Value());
    }
    // Explicit roots prevent transitive closure from silently copying unrelated user data.
    if (!retain_external) {
      for (NCollection_Map<occ::handle<TDF_Attribute>>::Iterator it(data->Attributes()); it.More(); it.Next()) {
        occ::handle<TDF_DataSet> references = new TDF_DataSet;
        it.Key()->References(references);
        for (NCollection_Map<TDF_Label>::Iterator r(references->Labels()); r.More(); r.Next())
          Require(data->ContainsLabel(r.Key()), "The copied subgraph contains an external reference.");
      }
    }
    TDF_CopyTool::Copy(data, relocation);
  });
}
