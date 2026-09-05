#pragma once

// Private native Xde/Structure contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Standard_GUID.hxx>

namespace OcctSharp::Native
{
const Standard_GUID& AssemblyExternalReferencesId();
}
