﻿#include "aerox/graphics/descriptors/DescriptorAllocator.hpp"
#include "aerox/core/GRuntime.hpp"
#include "aerox/graphics/GraphicsModule.hpp"

namespace aerox::graphics
{
    void DescriptorAllocator::DestroyPools()
    {
        for (auto &descriptorPool : _fullPools)
        {
            descriptorPool->Dispose();
        }

        for (auto &readyPool : _readyPools)
        {
            readyPool->Dispose();
        }

        _fullPools.clear();
        _readyPools.clear();

        _pools.clear();
    }

    void DescriptorAllocator::ClearPools()
    {
        for (auto &readyPool : _readyPools)
        {
            readyPool->Reset();
        }

        for (auto &descriptorPool : _fullPools)
        {
            descriptorPool->Reset();
            _readyPools.emplace(descriptorPool);
        }

        _fullPools.clear();
    }

    Shared<DescriptorPool> DescriptorAllocator::GetPool()
    {
        Shared<DescriptorPool> pool{};
        if(!_readyPools.empty())
        {
            pool = _pools[*_readyPools.begin()];
            _readyPools.erase(pool.get());
        }
        else
        {
            pool = CreatePool();
            _readyPools.emplace(pool.get());
            _setsPerPool = static_cast<uint32_t>(_setsPerPool * 1.5);
            if(_setsPerPool > 4092) _setsPerPool = 4092;
        }

        return pool;
    }

    Shared<DescriptorSet> DescriptorAllocator::Allocate(const vk::DescriptorSetLayout& layout,const std::vector<uint32_t>& variableCount)
    {
        auto targetPool = GetPool();
        Shared<DescriptorSet> set{};
        try
        {
            set = targetPool->Allocate(layout,variableCount);
        }
        catch (...)
        {
            _fullPools.emplace(targetPool.get());
            _readyPools.erase(targetPool.get());
            targetPool = GetPool();
            set = targetPool->Allocate(layout);
        }

        return set;
    }

    Shared<DescriptorPool> DescriptorAllocator::CreatePool()
    {
       std::vector<vk::DescriptorPoolSize> poolSizes{};
        poolSizes.reserve(_ratios.size());
        for (auto &ratio : _ratios)
        {
            poolSizes.emplace_back(ratio.type,static_cast<uint32_t>(ratio.ratio * static_cast<float>(_setsPerPool)));
        }

        auto poolInfo = vk::DescriptorPoolCreateInfo{_poolCreateFlags,_setsPerPool,poolSizes};

        auto device = GRuntime::Get()->GetModule<GraphicsModule>()->GetDevice();
        auto pool = device.createDescriptorPool(poolInfo);
        return newShared<DescriptorPool>(pool);
    }

    DescriptorAllocator::DescriptorAllocator(uint32_t maxSets, const std::vector<PoolSizeRatio>& poolRatios,
        const vk::DescriptorPoolCreateFlags& poolCreateFlags)
    {
        _setsPerPool = maxSets;
        _ratios = poolRatios;
        _poolCreateFlags = poolCreateFlags;
    }

    Shared<DescriptorAllocator> DescriptorAllocator::New(uint32_t maxSets,const std::vector<PoolSizeRatio>& poolRatios,const vk::DescriptorPoolCreateFlags& poolCreateFlags)
    {
        return newShared<DescriptorAllocator>(maxSets,poolRatios,poolCreateFlags);
    }

    void DescriptorAllocator::OnDispose(bool manual)
    {
        Disposable::OnDispose(manual);
        DestroyPools();
    }
}
