#pragma once
#include "GpuNative.hpp"
#include "vengine/Object.hpp"
#include "vengine/assets/Asset.hpp"

namespace vengine::drawing {
class Drawer;
class Texture : public Object<Drawer>, public assets::Asset, public GpuNative {

  std::optional<AllocatedImage> _gpuData;
  vk::Filter _filter = vk::Filter::eLinear;
  vk::SamplerAddressMode _tiling = vk::SamplerAddressMode::eRepeat;
  vk::Sampler _sampler = nullptr;
  vk::Format _format = vk::Format::eUndefined;
  vk::Extent3D _size;
  Array<unsigned char> _data;

  void MakeSampler();
public:

  std::optional<AllocatedImage> GetGpuData() const;
  vk::Sampler GetSampler() const;

  void SetGpuData(const std::optional<AllocatedImage> &allocation);
  virtual void SetTiling(vk::SamplerAddressMode tiling);
  virtual void SetFilter(vk::Filter filter);
  String GetSerializeId() override;

  void ReadFrom(Buffer &store) override;

  void WriteTo(Buffer &store) override;

  void Upload() override;
  bool IsUploaded() const override;

  void HandleDestroy() override;

  void Init(Drawer *outer) override;
  
  static Texture * FromData(Drawer * drawer, const Array<unsigned char> &data,vk::Extent3D size,vk::Format format,vk::Filter filter,vk::SamplerAddressMode tiling = vk::SamplerAddressMode::eRepeat);
};
}
