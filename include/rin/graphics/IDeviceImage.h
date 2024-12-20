﻿#pragma once
#include "ImageBarrierOptions.h"
#include "ImageFormat.h"
#include "vulkan/vulkan.hpp"
#include "graph/IGraphResource.h"
#include "rin/core/IDisposable.h"
#include "rin/core/memory.h"

namespace rin::graphics
{
    class IDeviceImage :  public IDisposable, public IGraphResource
    {
    public:
        virtual ImageFormat GetFormat() = 0;
        virtual vk::Extent3D GetExtent() = 0;
        virtual vk::Image GetImage() = 0;
        virtual vk::ImageView GetImageView() = 0;
        void Barrier(const vk::CommandBuffer& cmd,vk::ImageLayout from,
                             vk::ImageLayout to, const ImageBarrierOptions& options = {});
    };

}