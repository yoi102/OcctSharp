#pragma once

// Private native Foundation/Text contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <string>
#include <utility>

struct OcctSharp_AsciiStringHandle { explicit OcctSharp_AsciiStringHandle(TCollection_AsciiString value) : Value(std::move(value)) {} TCollection_AsciiString Value; };

struct OcctSharp_ExtendedStringHandle { explicit OcctSharp_ExtendedStringHandle(TCollection_ExtendedString value) : Value(std::move(value)) {} TCollection_ExtendedString Value; };

namespace OcctSharp::Native
{
void ValidateAsciiString(const OcctSharp_AsciiStringHandle* handle);

void ValidateExtendedString(const OcctSharp_ExtendedStringHandle* handle);

TCollection_AsciiString MakeAsciiString(const char* utf8, const int32_t length);

TCollection_ExtendedString MakeExtendedUtf8(const char* utf8, const int32_t length);

std::string ExtendedToUtf8(const TCollection_ExtendedString& value);

void CopyUtf8Result(const std::string& value, char* buffer, const int32_t capacity, int32_t* written);
}
