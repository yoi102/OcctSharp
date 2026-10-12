#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
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
      Require(!OcctSharpGenerated::DocumentAdapters::TDF_Label_HasAttribute(to[i]) && !OcctSharpGenerated::DocumentAdapters::TDF_Label_HasChild(to[i]), "Relocation destinations must be empty.");
      for (int j = 0; j < count; ++j) {
        Require(from[i] != to[j] && !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsDescendant(from[i],to[j]) && !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsDescendant(to[j],from[i]), "Source and destination trees overlap.");
        if (i != j) {
          Require(from[i] != from[j] && !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsDescendant(from[i],from[j]), "Source roots overlap.");
          Require(to[i] != to[j] && !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsDescendant(to[i],to[j]), "Destination roots overlap.");
        }
      }
    }
    occ::handle<TDF_DataSet> data = new TDF_DataSet;
    occ::handle<TDF_RelocationTable> relocation = new TDF_RelocationTable(retain_external != 0);
    auto add = [&](const TDF_Label& label) {
      OcctSharpGenerated::DocumentAdapters::TDF_DataSet_AddLabel(*(data),label);
      for (OcctSharpGenerated::DocumentAdapters::PrivateTDFAttributeIterator it(label); OcctSharpGenerated::DocumentAdapters::TDF_AttributeIterator_More(it); OcctSharpGenerated::DocumentAdapters::TDF_AttributeIterator_Next(it)) data->AddAttribute(OcctSharpGenerated::DocumentAdapters::TDF_AttributeIterator_Value(it));
    };
    for (int i = 0; i < count; ++i) {
      // OCCT AddRoot only appends a label already present in the data set.
      add(from[i]); OcctSharpGenerated::DocumentAdapters::TDF_DataSet_AddRoot(*(data),from[i]); OcctSharpGenerated::DocumentAdapters::TDF_RelocationTable_SetRelocation(*(relocation),from[i], to[i]);
      for (OcctSharpGenerated::DocumentAdapters::PrivateTDFChildIterator it(from[i], true); OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_More(it); OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_Next(it)) add(OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_Value(it));
    }
    // Explicit roots prevent transitive closure from silently copying unrelated user data.
    if (!retain_external) {
      for (NCollection_Map<occ::handle<TDF_Attribute>>::Iterator it(OcctSharpGenerated::DocumentAdapters::TDF_DataSet_Attributes(*(data))); it.More(); it.Next()) {
        occ::handle<TDF_DataSet> references = new TDF_DataSet;
        it.Key()->References(references);
        for (NCollection_Map<TDF_Label>::Iterator r(OcctSharpGenerated::DocumentAdapters::TDF_DataSet_Labels(*(references))); r.More(); r.Next())
          Require(OcctSharpGenerated::DocumentAdapters::TDF_DataSet_ContainsLabel(*(data),r.Key()), "The copied subgraph contains an external reference.");
      }
    }
    OcctSharpGenerated::DocumentAdapters::TDF_CopyTool_Copy(data, relocation);
  });
}
