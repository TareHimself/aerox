﻿#include "aerox/core/GRuntime.hpp"
#include <ranges>
#include <algorithm>
#include <filesystem>

#include "aerox/core/utils.hpp"

namespace aerox
{
    Unique<GRuntime> GRuntime::_runtime{};
    std::mutex GRuntime::_mutex{};

    GRuntime* GRuntime::Get()
    {
        if(!_runtime)
        {
            {
                std::lock_guard guard(_mutex);
                if(!_runtime)
                {
                    _runtime = std::make_unique<GRuntime>();
                }
            }
        }
        
        return _runtime.get();
    }

    void GRuntime::StartupModules()
    {
        {
            std::unordered_map<size_t,Shared<Module>> oldModules = _modules;
            
            for (const auto &mod : _modules | std::views::values)
            {
                mod->RegisterRequiredModules();
            }
        }
        
        _moduleList.reserve(_modules.size());
        for (const auto& module : _modules | std::views::values)
        {
            _moduleList.push_back(module);
        }

        std::ranges::sort(_moduleList,[](Shared<Module>& a,Shared<Module>& b)
        {

            if(a->IsSystemModule() != b->IsSystemModule())
            {
                if(a->IsSystemModule())
                {
                    return -1;
                }

                return 1;
            }
            
            if(a->IsDependentOn(b.get()))
            {
                return -1;
            }

            if(b->IsDependentOn(a.get()))
            {
                return 1;
            }

            
            return a->GetName().compare(b->GetName());
        });

        for (const auto module : _moduleList)
        {
            module->Startup(this);
        } 
    }

    void GRuntime::ShutdownModules()
    {
        
    
        for (auto& module : std::ranges::views::reverse(_moduleList))
        {
            module->Shutdown(this);
        }

        _modules.clear();
    
        _moduleList.clear();
    }

    void GRuntime::Loop()
    {
        while(!WillExit())
        {
            onTick->Invoke(0.0);
        }
    }

    bool GRuntime::WillExit() const
    {
        return _exitRequested;
    }

    void GRuntime::RequestExit()
    {
        _exitRequested = true;
    }

    void GRuntime::Run()
    {
        if(!std::filesystem::exists(getResourcesPath()))
        {
            std::filesystem::create_directories(getResourcesPath());
        }
        StartupModules();
        Loop();
        ShutdownModules();
    }
}
