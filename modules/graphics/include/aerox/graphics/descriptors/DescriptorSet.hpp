﻿#pragma once
#include "aerox/core/Disposable.hpp"
#include "aerox/graphics/shaders/Shader.hpp"
#include "vulkan/vulkan.hpp"

class DescriptorSet : public  Disposable
{
    vk::DescriptorSet _set{};
    vk::Device _device{};
    std::unordered_map<uint32_t,std::vector<Shared<Disposable>>> _resources{};
public:
    explicit DescriptorSet(const vk::DescriptorSet& descriptorSet);
        
    void WriteSampledImages(uint32_t binding,const std::vector<Shared<DeviceImage>>& images,vk::ImageLayout layout);

    void WriteSamplers(uint32_t binding,const std::vector<vk::Sampler>& samplers);

    void WriteBuffer(uint32_t binding,const vk::DescriptorType bufferType,const Shared<DeviceBuffer>& buffer,const uint32_t offset = 0);

    void OnDispose(bool manual) override;

    vk::DescriptorSet GetInternalSet() const;
};