// Native Foundation/Text implementation. Public contracts and ownership are unchanged.
#include "Foundation/Text.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <cstddef>
#include <cstring>
#include <string>

namespace OcctSharp::Native
{
void ValidateAsciiString(const OcctSharp_AsciiStringHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The ASCII string handle is null.");
  if (!IsLiveValue(handle, LiveAsciiStrings)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The ASCII string handle is invalid or already released.");
}

void ValidateExtendedString(const OcctSharp_ExtendedStringHandle* handle)
{
  if (handle == nullptr) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The extended string handle is null.");
  if (!IsLiveValue(handle, LiveExtendedStrings)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The extended string handle is invalid or already released.");
}

TCollection_AsciiString MakeAsciiString(const char* utf8, const int32_t length)
{
  return length == 0 ? TCollection_AsciiString() : TCollection_AsciiString(utf8, length);
}

TCollection_ExtendedString MakeExtendedUtf8(const char* utf8, const int32_t length)
{
  ValidateUtf8Input(utf8, length);
  return TCollection_ExtendedString(MakeAsciiString(utf8, length), true);
}

std::string ExtendedToUtf8(const TCollection_ExtendedString& value)
{
  const int32_t capacity = value.LengthOfCString() + 1;
  std::string result(static_cast<size_t>(capacity), '\0');
  Standard_PCharacter output = result.data();
  const int32_t written = value.ToUTF8CString(output);
  result.resize(static_cast<size_t>(written));
  return result;
}

void CopyUtf8Result(const std::string& value, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UTF-8 output length pointer is null.");
  }
  *written = 0;
  ValidateOutputBuffer(buffer, capacity, static_cast<int32_t>(value.size()) + 1);
  if (!value.empty())
  {
    std::memcpy(buffer, value.data(), value.size());
  }
  buffer[value.size()] = '\0';
  *written = static_cast<int32_t>(value.size());
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_create(
  const char* utf8, const int32_t length, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateUtf8Input(utf8, length);
    *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(MakeAsciiString(utf8, length)), LiveAsciiStrings);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_clone(
  const OcctSharp_AsciiStringHandle* source, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateAsciiString(source); *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(source->Value), LiveAsciiStrings); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_length(const OcctSharp_AsciiStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The ASCII string length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateAsciiString(string); *length = string->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_append(
  OcctSharp_AsciiStringHandle* string, const char* utf8, const int32_t length)
{
  return Guard([&] { ValidateAsciiString(string); ValidateUtf8Input(utf8, length); if (length > 0) string->Value.AssignCat(utf8, length); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_utf8(
  const OcctSharp_AsciiStringHandle* string, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The ASCII string output length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateAsciiString(string);
    const int32_t length = string->Value.Length();
    ValidateOutputBuffer(buffer, capacity, length + 1);
    if (length > 0) std::memcpy(buffer, string->Value.ToCString(), static_cast<size_t>(length));
    buffer[length] = '\0';
    *written = length;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ascii_to_extended(
  const OcctSharp_AsciiStringHandle* string, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateAsciiString(string);
    *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(TCollection_ExtendedString(string->Value, true)), LiveExtendedStrings);
  });
}

void OCCTSHARP_CALL occtsharp_ascii_release(OcctSharp_AsciiStringHandle* string)
{ if (string != nullptr && UnregisterValue(string, LiveAsciiStrings)) delete string; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_create_utf8(
  const char* utf8, const int32_t length, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&]
  {
    ValidateUtf8Input(utf8, length);
    TCollection_AsciiString ascii = MakeAsciiString(utf8, length);
    *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(TCollection_ExtendedString(ascii, true)), LiveExtendedStrings);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_clone(
  const OcctSharp_ExtendedStringHandle* source, OcctSharp_ExtendedStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output extended string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateExtendedString(source); *out_string = AllocateValue(new OcctSharp_ExtendedStringHandle(source->Value), LiveExtendedStrings); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_length(const OcctSharp_ExtendedStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The extended string length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateExtendedString(string); *length = string->Value.Length(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_utf8_length(const OcctSharp_ExtendedStringHandle* string, int32_t* length)
{
  if (length == nullptr) { SetLastError("The extended string UTF-8 length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { ValidateExtendedString(string); *length = string->Value.LengthOfCString(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_append_utf8(
  OcctSharp_ExtendedStringHandle* string, const char* utf8, const int32_t length)
{
  return Guard([&]
  {
    ValidateExtendedString(string);
    ValidateUtf8Input(utf8, length);
    TCollection_AsciiString ascii = MakeAsciiString(utf8, length);
    string->Value.AssignCat(TCollection_ExtendedString(ascii, true));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_utf8(
  const OcctSharp_ExtendedStringHandle* string, char* buffer, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The extended string output length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    ValidateExtendedString(string);
    const int32_t length = string->Value.LengthOfCString();
    ValidateOutputBuffer(buffer, capacity, length + 1);
    std::string converted(static_cast<size_t>(length) + 1, '\0');
    Standard_PCharacter output = converted.data();
    const int32_t convertedLength = string->Value.ToUTF8CString(output);
    if (convertedLength > 0) std::memcpy(buffer, converted.data(), static_cast<size_t>(convertedLength));
    buffer[convertedLength] = '\0';
    *written = convertedLength;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_value(
  const OcctSharp_ExtendedStringHandle* string, const int32_t index, uint16_t* value)
{
  if (value == nullptr) { SetLastError("The extended string value output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *value = 0;
  return Guard([&] { ValidateExtendedString(string); if (index < 1 || index > string->Value.Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Extended string indices are 1-based and must be within the string length."); *value = static_cast<uint16_t>(string->Value.Value(index)); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_extended_to_ascii(
  const OcctSharp_ExtendedStringHandle* string, OcctSharp_AsciiStringHandle** out_string)
{
  if (out_string == nullptr) { SetLastError("The output ASCII string pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_string = nullptr;
  return Guard([&] { ValidateExtendedString(string); *out_string = AllocateValue(new OcctSharp_AsciiStringHandle(TCollection_AsciiString(string->Value)), LiveAsciiStrings); });
}

void OCCTSHARP_CALL occtsharp_extended_release(OcctSharp_ExtendedStringHandle* string)
{ if (string != nullptr && UnregisterValue(string, LiveExtendedStrings)) delete string; }
