#include "../../generated/Foundation/OcctSharp.Foundation.DocumentIdentityAdapters.Generated.hxx"
#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include "Documents/Parametric.hxx"
#include "Runtime/Validation.hxx"
#include <Standard_GUID.hxx>
#include <TDF_Reference.hxx>
#include <TFunction_Function.hxx>
#include <TFunction_Logbook.hxx>
#include <algorithm>
#include <set>
#include <vector>

namespace OcctSharp::Native::Parametric
{
occ::handle<TFunction_Scope> Scope(const TDF_Label& label)
{
  occ::handle<TFunction_Scope> scope;
  Require(OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(OcctSharpGenerated::DocumentAdapters::TDF_Label_Root(label),OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetID(), scope), "No function scope exists.");
  return scope;
}
occ::handle<TFunction_GraphNode> Node(const TDF_Label& label)
{
  occ::handle<TFunction_GraphNode> node;
  Require(OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(label,OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetID(), node), "The label is not a registered feature.");
  return node;
}
}

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Parametric;

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_register(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* driver, int32_t* id)
{
  if (id) *id = 0;
  return Guard([&] {
    Require(id && driver && OcctSharpGenerated::DocumentIdentityAdapters::Standard_GUID_CheckGUIDFormat(driver), "A valid driver GUID and ID output are required.");
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    auto scope = OcctSharpGenerated::DocumentAdapters::TFunction_Scope_Set(label);
    Require(!OcctSharpGenerated::DocumentAdapters::TFunction_Scope_HasFunction(*(scope),label), "The feature is already registered.");
    OcctSharpGenerated::DocumentAdapters::TFunction_Function_Set(label, OcctSharpGenerated::DocumentIdentityAdapters::ConstructStandardGUID(driver));
    auto node = OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_Set(label);
    // Relocated attributes contain old scope IDs. A newly registered node starts isolated.
    node->RemoveAllPrevious(); node->RemoveAllNext();
    Require(OcctSharpGenerated::DocumentAdapters::TFunction_Scope_AddFunction(*(scope),label), "Could not allocate the function scope ID.");
    *id = OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),label);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_remove(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&] {
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    auto scope = Scope(label); auto node = Node(label); const int id = OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),label);
    Require(OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetNext(*(node)).IsEmpty(), "Remove dependant functions first.");
    for (NCollection_Map<int>::Iterator it(OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetPrevious(*(node))); it.More(); it.Next())
      Node(OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),it.Key()))->RemoveNext(id);
    OcctSharpGenerated::DocumentAdapters::TFunction_Scope_RemoveFunction(*(scope),label);
    OcctSharpGenerated::DocumentAdapters::TDF_Label_ForgetAllAttributes(label,true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_rewire(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t* previous, int32_t count)
{
  return Guard([&] {
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    Require(count >= 0 && count <= 4096, "The dependency count exceeds the bounded limit.");
    ValidateArray(previous, count, "The dependency buffer is null.");
    auto scope = Scope(label); auto node = Node(label); const int id = OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),label);
    std::set<int> unique;
    for (int i = 0; i < count; ++i) {
      Require(previous[i] != id && unique.insert(previous[i]).second && scope->HasFunction(previous[i]),
        "A dependency is self-referential, repeated or absent from this document.");
      std::vector<int> pending{previous[i]}; std::set<int> visited;
      while (!pending.empty()) {
        const int current = pending.back(); pending.pop_back();
        Require(current != id, "The dependency rewire would create a cycle.");
        if (!visited.insert(current).second) continue;
        Require(visited.size() <= 4096, "The function graph exceeds the bounded limit.");
        const auto ancestor = Node(OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),current));
        for (NCollection_Map<int>::Iterator it(OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetPrevious(*(ancestor))); it.More(); it.Next())
          pending.push_back(it.Key());
      }
    }
    // All references and cycles are checked before either direction is changed.
    for (NCollection_Map<int>::Iterator it(OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetPrevious(*(node))); it.More(); it.Next())
      Node(OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),it.Key()))->RemoveNext(id);
    node->RemoveAllPrevious();
    for (int dependency : unique) {
      node->AddPrevious(dependency); Node(OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(scope),dependency))->AddNext(id);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_links(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t next,
  int32_t* values, int32_t capacity, int32_t* count, int32_t* id, int32_t* state)
{
  if (count) *count = 0; if (id) *id = 0; if (state) *state = 0;
  return Guard([&] {
    Require(count && id && state && (next == 0 || next == 1) && capacity >= 0, "Invalid graph outputs.");
    const auto label = ResolveOcafLabel(document, entry); auto node = Node(label);
    const auto& links = next ? OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetNext(*(node)) : OcctSharpGenerated::DocumentAdapters::TFunction_GraphNode_GetPrevious(*(node));
    std::vector<int> result;
    for (NCollection_Map<int>::Iterator it(links); it.More(); it.Next()) result.push_back(it.Key());
    std::sort(result.begin(), result.end());
    *count = static_cast<int32_t>(result.size()); *id = OcctSharpGenerated::DocumentAdapters::TFunction_Scope_GetFunction(*(Scope(label)),label);
    *state = static_cast<int32_t>(node->GetStatus());
    if (!values && capacity == 0) return;
    ValidateOutputCapacity(capacity, *count, values, "The graph output buffer is too small.");
    std::copy(result.begin(), result.end(), values);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_state(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t state, int32_t failure)
{
  return Guard([&] {
    Require(state >= 0 && state <= 4 && failure >= 0, "Invalid function execution state.");
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    Node(label)->SetStatus(static_cast<TFunction_ExecutionStatus>(state));
    occ::handle<TFunction_Function> function;
    Require(OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(label,OcctSharpGenerated::DocumentAdapters::TFunction_Function_GetID(), function), "The function attribute is missing.");
    function->SetFailure(failure);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_logbook(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t operation, int32_t* flags)
{
  if (flags) *flags = 0;
  return Guard([&] {
    Require(flags && operation >= 0 && operation <= 5, "Invalid logbook operation.");
    const auto label = ResolveOcafLabel(document, entry);
    occ::handle<TFunction_Logbook> log;
    if (operation != 5) {
      RequireOpenOcafCommand(document); log = OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_Set(label);
      // Inline OCCT mutators do not all call Backup; preserve undo for every set.
      log->Backup();
      if (operation == 0) log->Clear();
      if (operation == 1) OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_SetTouched(*(log),label);
      if (operation == 2) OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_SetImpacted(*(log),label);
      if (operation == 3) OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_SetValid(*(log),label);
      if (operation == 4) log->Done(true);
    } else if (!OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(OcctSharpGenerated::DocumentAdapters::TDF_Label_Root(label),OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_GetID(), log)) return;
    *flags = (OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_GetTouched(*(log)).Contains(label) ? 1 : 0) | (OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_GetImpacted(*(log)).Contains(label) ? 2 : 0)
      | (OcctSharpGenerated::DocumentAdapters::TFunction_Logbook_GetValid(*(log)).Contains(label) ? 4 : 0) | (log->IsDone() ? 8 : 0);
  });
}
