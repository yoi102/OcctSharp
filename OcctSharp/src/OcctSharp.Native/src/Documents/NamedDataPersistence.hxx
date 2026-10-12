#pragma once
#include <Standard_Handle.hxx>
class TDocStd_Application;
namespace OcctSharp::Native {
// Application-local extension of existing attribute drivers; no global factory mutation.
void ConfigureSafeNamedDataDrivers(const occ::handle<TDocStd_Application>& application);
}
