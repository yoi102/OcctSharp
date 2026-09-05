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
  Require(label.Root().FindAttribute(TFunction_Scope::GetID(), scope), "No function scope exists.");
  return scope;
}
occ::handle<TFunction_GraphNode> Node(const TDF_Label& label)
{
  occ::handle<TFunction_GraphNode> node;
  Require(label.FindAttribute(TFunction_GraphNode::GetID(), node), "The label is not a registered feature.");
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
    Require(id && driver && Standard_GUID::CheckGUIDFormat(driver), "A valid driver GUID and ID output are required.");
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    auto scope = TFunction_Scope::Set(label);
    Require(!scope->HasFunction(label), "The feature is already registered.");
    TFunction_Function::Set(label, Standard_GUID(driver));
    auto node = TFunction_GraphNode::Set(label);
    // Relocated attributes contain old scope IDs. A newly registered node starts isolated.
    node->RemoveAllPrevious(); node->RemoveAllNext();
    Require(scope->AddFunction(label), "Could not allocate the function scope ID.");
    *id = scope->GetFunction(label);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_remove(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&] {
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    auto scope = Scope(label); auto node = Node(label); const int id = scope->GetFunction(label);
    Require(node->GetNext().IsEmpty(), "Remove dependant functions first.");
    for (NCollection_Map<int>::Iterator it(node->GetPrevious()); it.More(); it.Next())
      Node(scope->GetFunction(it.Key()))->RemoveNext(id);
    scope->RemoveFunction(label);
    label.ForgetAllAttributes(true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_function_rewire(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t* previous, int32_t count)
{
  return Guard([&] {
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    Require(count >= 0 && count <= 4096, "The dependency count exceeds the bounded limit.");
    ValidateArray(previous, count, "The dependency buffer is null.");
    auto scope = Scope(label); auto node = Node(label); const int id = scope->GetFunction(label);
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
        const auto ancestor = Node(scope->GetFunction(current));
        for (NCollection_Map<int>::Iterator it(ancestor->GetPrevious()); it.More(); it.Next())
          pending.push_back(it.Key());
      }
    }
    // All references and cycles are checked before either direction is changed.
    for (NCollection_Map<int>::Iterator it(node->GetPrevious()); it.More(); it.Next())
      Node(scope->GetFunction(it.Key()))->RemoveNext(id);
    node->RemoveAllPrevious();
    for (int dependency : unique) {
      node->AddPrevious(dependency); Node(scope->GetFunction(dependency))->AddNext(id);
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
    const auto& links = next ? node->GetNext() : node->GetPrevious();
    std::vector<int> result;
    for (NCollection_Map<int>::Iterator it(links); it.More(); it.Next()) result.push_back(it.Key());
    std::sort(result.begin(), result.end());
    *count = static_cast<int32_t>(result.size()); *id = Scope(label)->GetFunction(label);
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
    Require(label.FindAttribute(TFunction_Function::GetID(), function), "The function attribute is missing.");
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
      RequireOpenOcafCommand(document); log = TFunction_Logbook::Set(label);
      // Inline OCCT mutators do not all call Backup; preserve undo for every set.
      log->Backup();
      if (operation == 0) log->Clear();
      if (operation == 1) log->SetTouched(label);
      if (operation == 2) log->SetImpacted(label);
      if (operation == 3) log->SetValid(label);
      if (operation == 4) log->Done(true);
    } else if (!label.Root().FindAttribute(TFunction_Logbook::GetID(), log)) return;
    *flags = (log->GetTouched().Contains(label) ? 1 : 0) | (log->GetImpacted().Contains(label) ? 2 : 0)
      | (log->GetValid().Contains(label) ? 4 : 0) | (log->IsDone() ? 8 : 0);
  });
}
