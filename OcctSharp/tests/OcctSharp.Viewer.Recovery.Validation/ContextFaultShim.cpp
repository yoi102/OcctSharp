// Test-only instrumentation. Never linked into or packaged with the product bridge.
#include "Visualization/Context.hxx"
#include <OpenGl_View.hxx>
#include <OpenGl_Window.hxx>
#include <OpenGl_Context.hxx>
#include <memory>

struct FaultLease
{
  occ::handle<OpenGl_Window> Window;
  occ::handle<OpenGl_Context> Original;
};

extern "C" __declspec(dllexport) void* __cdecl cg_context_fault_begin(OcctSharp_ViewerHandle* viewer, int mode)
{
  try
  {
    if (!viewer || viewer->OwnerThread != std::this_thread::get_id() || (mode != 2 && mode != 4)) return nullptr;
    auto view = occ::down_cast<OpenGl_View>(viewer->View->View());
    if (view.IsNull() || view->GlWindow().IsNull()) return nullptr;
    auto lease = std::make_unique<FaultLease>(); lease->Window = view->GlWindow();
    auto& slot = const_cast<occ::handle<OpenGl_Context>&>(lease->Window->GetGlContext());
    if (slot.IsNull() || !slot->IsValid()) return nullptr;
    lease->Original = slot;
    // Replace the SDK's actual window-owned context slot, not a health-mask field.
    // The old context is parked only for safe cleanup; product queries see the fault.
    if (mode == 2) slot.Nullify();
    else slot = new OpenGl_Context(); // Real SDK object: never initialized, IsValid()==false.
    return lease.release();
  }
  catch (...) { return nullptr; }
}
extern "C" __declspec(dllexport) int __cdecl cg_context_fault_observe(OcctSharp_ViewerHandle* viewer)
{
  try
  {
    if (!viewer || viewer->OwnerThread != std::this_thread::get_id()) return -1;
    const auto& actual = viewer->Driver->GetSharedContext();
    return actual.IsNull() ? 2 : (actual->IsValid() ? 0 : 4);
  }
  catch (...) { return -1; }
}
extern "C" __declspec(dllexport) void __cdecl cg_context_fault_end(void* value)
{
  std::unique_ptr<FaultLease> lease(static_cast<FaultLease*>(value));
  if (lease) const_cast<occ::handle<OpenGl_Context>&>(lease->Window->GetGlContext()) = lease->Original;
}
