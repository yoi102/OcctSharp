#include "Visualization/Context.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Validation.hxx"
#include <Image_AlienPixMap.hxx>
#include <filesystem>
#include <fstream>
#include <cstring>
#include <algorithm>

using namespace OcctSharp::Native;
namespace {
void CheckImageSize(OcctSharp_ViewerHandle* viewer, int width, int height) {
  const auto caps = ReadRenderCaps(viewer);
  RequireRender(width > 0 && height > 0 && width <= caps.max_texture && height <= caps.max_texture &&
    static_cast<int64_t>(width) * height <= 16777216, "Image exceeds the driver or 16-megapixel budget.");
}
int64_t StoreImage(OcctSharp_ViewerHandle* viewer, const opencascade::handle<Image_PixMap>& image) {
  RequireRender(viewer->Rendering.Textures.size() < 128, "Texture count budget exceeded.");
  const int64_t id = viewer->Rendering.NextId; viewer->Rendering.Textures.emplace(id, image); ++viewer->Rendering.NextId; return id;
}
}
namespace OcctSharp::Native {
opencascade::handle<Image_PixMap> FindTexture(OcctSharp_ViewerHandle* viewer, int64_t id) {
  const auto found = viewer->Rendering.Textures.find(id);
  RequireRender(found != viewer->Rendering.Textures.end(), "Unknown or removed texture ID."); return found->second;
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_pixels(OcctSharp_ViewerHandle* viewer, int64_t id,
  const OcctSharp_PixelInput* input, const uint8_t* bytes, int32_t length, int64_t* output) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(input && output, "Null texture description/output.");
    const auto& p = *input; CheckImageSize(viewer, p.width, p.height); RenderFlag(p.bottom_up); RenderFlag(p.format);
    RequireRender(p.reserved == 0 && p.stride >= static_cast<int64_t>(p.width) * 4 &&
      static_cast<int64_t>(p.stride) * p.height == length && length <= 67108864 && bytes, "Invalid copied RGBA/BGRA buffer or stride.");
    if (id != 0) (void)FindTexture(viewer, id);
    opencascade::handle<Image_PixMap> image = new Image_PixMap();
    RequireRender(image->InitTrash(Image_Format_RGBA, p.width, p.height), "Cannot allocate texture image."); image->SetTopDown(true);
    for (int y = 0; y < p.height; ++y) {
      const auto source = bytes + static_cast<size_t>(p.bottom_up ? p.height - 1 - y : y) * p.stride;
      auto target = image->ChangeRow(y); std::memcpy(target, source, static_cast<size_t>(p.width) * 4);
      if (p.format == 1) for (int x = 0; x < p.width; ++x) std::swap(target[x * 4], target[x * 4 + 2]);
    }
    if (id == 0) id = StoreImage(viewer, image);
    else { ReplaceReviewTexture(viewer, id, image); viewer->Rendering.Textures.at(id) = image; }
    *output = id;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_file(OcctSharp_ViewerHandle* viewer, const char* path,
  int64_t* output, OcctSharp_PixelInput* description) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(output && description, "Null image output."); ValidatePath(path);
    const std::string text(path);
    RequireRender(text.size() > 3 && text.size() < 32768 && text[1] == ':' && (text[2] == '\\' || text[2] == '/'), "An explicit local drive path is required; URLs and UNC paths are not loaded.");
    const auto file = std::filesystem::u8path(text); const auto size = std::filesystem::file_size(file);
    RequireRender(size > 0 && size <= 33554432, "Local image exceeds the 32-MiB encoded-file budget.");
    std::ifstream stream(file, std::ios::binary); std::vector<uint8_t> encoded(static_cast<size_t>(size));
    RequireRender(static_cast<bool>(stream.read(reinterpret_cast<char*>(encoded.data()), static_cast<std::streamsize>(size))), "Cannot read local image.");
    opencascade::handle<Image_AlienPixMap> decoded = new Image_AlienPixMap();
    RequireRender(decoded->Load(encoded.data(), encoded.size(), TCollection_AsciiString(file.extension().string().c_str())), "Unsupported or corrupt local image.");
    CheckImageSize(viewer, static_cast<int>(decoded->SizeX()), static_cast<int>(decoded->SizeY()));
    opencascade::handle<Image_PixMap> image = new Image_PixMap();
    RequireRender(image->InitTrash(Image_Format_RGBA, decoded->SizeX(), decoded->SizeY()), "Cannot allocate owned image."); image->SetTopDown(true);
    for (int y = 0; y < static_cast<int>(image->SizeY()); ++y) for (int x = 0; x < static_cast<int>(image->SizeX()); ++x)
      image->SetPixelColor(x, y, decoded->PixelColor(x, y));
    const int64_t id = StoreImage(viewer, image);
    *description = { static_cast<int32_t>(image->SizeX()), static_cast<int32_t>(image->SizeY()), static_cast<int32_t>(image->SizeX() * 4), 0, 0, 0 }; *output = id;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_texture_remove(OcctSharp_ViewerHandle* viewer, int64_t id) {
  return Guard([&] { ValidateViewerThread(viewer); (void)FindTexture(viewer, id);
    ReplaceReviewTexture(viewer, id, {}); viewer->Rendering.Textures.erase(id); });
}
