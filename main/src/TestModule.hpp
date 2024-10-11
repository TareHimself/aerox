﻿#pragma once
#include "rin/core/memory.hpp"
#include "rin/core/Module.hpp"
#include "rin/core/meta/MetaMacros.hpp"
#include "rin/graphics/GraphicsModule.hpp"
#include "rin/graphics/shaders/GraphicsShader.hpp"
#include "rin/widgets/Widget.hpp"
#include "rin/widgets/WidgetsModule.hpp"
#include "rin/widgets/containers/FlexWidget.hpp"
#include "rin/widgets/graphics/WidgetCustomDrawCommand.hpp"
#include "rin/window/WindowModule.hpp"
#include "rin/window/Window.hpp"

namespace bass
{
    class FileStream;
}

class TestClipCommand : public WidgetCustomDrawCommand
{
    void Run(SurfaceFrame* frame) override;
};

class TextTestWidget : public Widget
{
protected:
    int _atlas = -1;
    TextTestWidget();
    Vec2<float> ComputeContentSize() override;

public:
    void Collect(const TransformInfo& transform, WidgetDrawCommands& drawCommands) override;
};

class TestWidget : public Widget
{
    Vec2<float> _lastLocation{0.0f};

protected:
    Vec2<float> ComputeContentSize() override;

public:
    void Collect(const TransformInfo& transform,
                 WidgetDrawCommands& drawCommands) override;
};

MCLASS()

class TestModule : public RinModule
{
    WindowModule* _windowModule = nullptr;
    WidgetsModule* _widgetsModule = nullptr;
    Shared<Window> _window{};
    bass::FileStream* _stream = nullptr;
    Shared<FlexWidget> _container{};

public:
    BackgroundThread<void> tasks{};
    std::string GetName() override;
    void Startup(GRuntime* runtime) override;
    void Shutdown(GRuntime* runtime) override;
    bool IsDependentOn(RinModule* module) override;
    void RegisterRequiredModules() override;
    void LoadImages();
    static void OnCloseRequested(Window* window);
};
