﻿#pragma once
#include "vulkan/vulkan.hpp"

namespace aerox::graphics
{
    struct SamplerSpec
    {
 
    public:
        vk::Filter filter{};
        vk::SamplerAddressMode tiling{};
        SamplerSpec(const vk::Filter& inFilter,const vk::SamplerAddressMode& inTiling);

        std::string GetId() const;
    };
}
