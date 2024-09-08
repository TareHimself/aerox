﻿#pragma once
#include <unordered_map>
#include <vulkan/vulkan.hpp>

namespace aerox::graphics
{
    class DescriptorLayoutBuilder
    {
        std::unordered_map<uint32_t,vk::DescriptorSetLayoutBinding> _bindings{};
        std::unordered_map<uint32_t,vk::DescriptorBindingFlags> _flags{};
    public:
        DescriptorLayoutBuilder& AddBinding(uint32_t binding,vk::DescriptorType type,const vk::ShaderStageFlags& stageFlags,uint32_t count = 1,const vk::DescriptorBindingFlags& flags = vk::DescriptorBindingFlagBits::eUpdateAfterBind);
        DescriptorLayoutBuilder& Clear();
        vk::DescriptorSetLayout Build(const vk::DescriptorSetLayoutCreateFlags& layoutFlags = vk::DescriptorSetLayoutCreateFlagBits::eUpdateAfterBindPool);
    };
}
