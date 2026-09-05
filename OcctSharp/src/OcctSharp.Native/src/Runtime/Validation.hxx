#pragma once

// Private native Runtime/Validation contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"

namespace OcctSharp::Native
{
void ValidateUtf8Input(const char* utf8, const int32_t length);

void ValidateOutputBuffer(char* buffer, const int32_t capacity, const int32_t required);

void ValidatePath(const char* filePath);

void ValidateOutputCapacity(const int32_t capacity, const int32_t required, const void* buffer, const char* message);

void ValidateFinite(double value, const char* name);

void ValidateArray(const void* values, const int32_t count, const char* message);
}
