﻿#include "aerox/widgets/Surface.hpp"
#include <ranges>
#include <set>
#include "aerox/graphics/commandBufferUtils.hpp"
#include "aerox/core/GRuntime.hpp"
#include "aerox/graphics/DeviceBuffer.hpp"
#include "aerox/graphics/GraphicsModule.hpp"
#include "aerox/graphics/ResourceManager.hpp"
#include "aerox/widgets/Widget.hpp"
#include "aerox/widgets/Container.hpp"
#include "aerox/widgets/WidgetsModule.hpp"
#include "aerox/widgets/event/CursorDownEvent.hpp"
#include "aerox/widgets/event/CursorMoveEvent.hpp"
#include "aerox/widgets/event/ResizeEvent.hpp"
#include "aerox/widgets/event/ScrollEvent.hpp"
#include "aerox/widgets/graphics/BatchedDrawCommand.hpp"
#include "aerox/widgets/graphics/BatchInfo.hpp"
#include "aerox/widgets/graphics/CustomDrawCommand.hpp"
#include "aerox/widgets/graphics/QuadInfo.hpp"

namespace aerox::widgets
{
    void Surface::DoHover()
    {
        auto cursorPosition = GetCursorPosition();

        auto delta = _lastCursorPosition.has_value() ? cursorPosition - _lastCursorPosition.value() : Vec2<float>(0, 0);

        _lastCursorPosition = cursorPosition;

        auto event = std::make_shared<CursorMoveEvent>(this->GetSharedDynamic<Surface>(), cursorPosition);

        TransformInfo info{this};

        auto size = GetDrawSize();

        // Store old hover list
        auto oldHoverList = _lastHovered;
        _lastHovered.clear();

        // Build new hover list
        for (auto& rootWidget : std::ranges::reverse_view(_rootWidgets))
        {
            if (!rootWidget->IsHitTestable()) continue;

            auto widgetTransformInfo = info.AccountFor(rootWidget);

            if (!widgetTransformInfo.IsPointWithin(event->position)) continue;

            rootWidget->NotifyCursorEnter(event, widgetTransformInfo, _lastHovered);

            break;
        }

        std::set<Widget*> hoveredSet{};
        for (auto& hovered : _lastHovered)
        {
            hoveredSet.emplace(hovered.get());
        }
        
        Shared<Container> lastParent{};
        TransformInfo curTransform = info;
        for (auto &widget : std::ranges::reverse_view(oldHoverList))
        {
            curTransform = lastParent ? lastParent->ComputeChildTransform(widget,curTransform) : curTransform;
            lastParent = std::dynamic_pointer_cast<Container>(widget);
            if(!hoveredSet.contains(widget.get()))
            {
                widget->NotifyCursorLeave(event,curTransform);
            }
        }
    }

    std::string Surface::MAIN_PASS_ID;
    
    std::vector<Shared<Widget>> Surface::GetRootWidgets() const
    {
        return _rootWidgets;
    }

    void Surface::Init()
    {
        CreateImages();
    }

    void Surface::CreateImages()
    {
        auto imageExtent = GetDrawSize().Cast<uint32_t>();
        auto usageFlags = vk::ImageUsageFlagBits::eTransferSrc | vk::ImageUsageFlagBits::eTransferDst |
            vk::ImageUsageFlagBits::eSampled | vk::ImageUsageFlagBits::eColorAttachment;

        auto graphicsModule = GRuntime::Get()->GetModule<graphics::GraphicsModule>();

        _drawImage = graphicsModule->CreateImage({imageExtent.x, imageExtent.y, 1}, vk::Format::eR32G32B32A32Sfloat,
                                                 usageFlags, false, "Widget Surface Main Image");

        _copyImage = graphicsModule->CreateImage({imageExtent.x, imageExtent.y, 1}, vk::Format::eR32G32B32A32Sfloat,
                                                 usageFlags, false, "Widget Surface Main Image");
        
        _stencilImage = graphicsModule->CreateImage({imageExtent.x,imageExtent.y,1},vk::Format::eD32SfloatS8Uint,vk::ImageUsageFlagBits::eTransferSrc | vk::ImageUsageFlagBits::eTransferDst |
            vk::ImageUsageFlagBits::eDepthStencilAttachment,false,"Widget Surface Stencil Image");
    }

    void Surface::ClearFocus()
    {
        if (_focusedWidget)
        {
            _focusedWidget->OnFocusLost();
        }

        _focusedWidget.reset();
    }

    bool Surface::RequestFocus(const Shared<Widget>& widget)
    {
        if (_focusedWidget == widget) return true;
        if (!widget->IsHitTestable()) return false;

        ClearFocus();

        _focusedWidget = widget;

        widget->OnFocus();

        return true;
    }

    void Surface::NotifyResize(const Shared<ResizeEvent>& event)
    {
        GRuntime::Get()->GetModule<graphics::GraphicsModule>()->WaitForDeviceIdle();
        _drawImage.reset();
        _copyImage.reset();
        CreateImages();

        for (const auto& rootWidget : GetRootWidgets())
        {
            rootWidget->SetDrawSize(event->size);
        }

        onResize->Invoke(event);
    }

    void Surface::NotifyCursorDown(const Shared<CursorDownEvent>& event)
    {
        TransformInfo rootInfo{this};
        bool shouldKeepFocus = false;
        for(auto &rootWidget  : std::ranges::reverse_view(GetRootWidgets()))
        {
            if(!rootWidget->IsHitTestable()) continue;
            
            auto widgetTransformInfo = rootInfo.AccountFor(rootWidget);
            
            if(!widgetTransformInfo.IsPointWithin(event->position)) continue;

            // The widget that handled the cursor down event
            auto result = rootWidget->NotifyCursorDown(event,widgetTransformInfo);

            if(!result) continue;

            if(!_focusedWidget) continue;
            
            while(result)
            {
                if(result == _focusedWidget)
                {
                    shouldKeepFocus = true;
                    break;
                }

                result = result->GetParent();
            }

            break;
        }

        if(!shouldKeepFocus)
        {
            ClearFocus();
        }
    }

    void Surface::NotifyCursorUp(const Shared<CursorUpEvent>& event)
    {
        onCursorUp->Invoke(event);
    }

    void Surface::NotifyCursorMove(const Shared<CursorMoveEvent>& event)
    {
        _lastCursorPosition = event->position;

        TransformInfo info{this};

        for (auto& rootWidget : GetRootWidgets())
        {
            if (!rootWidget->IsHitTestable()) continue;

            auto widgetTransformInfo = info.AccountFor(rootWidget);

            if (!widgetTransformInfo.IsPointWithin(event->position)) continue;

            if (!rootWidget->NotifyCursorMove(event, widgetTransformInfo)) continue;

            break;
        }
    }

    void Surface::NotifyScroll(const Shared<ScrollEvent>& event)
    {
        TransformInfo info{this};

        for (auto& rootWidget : GetRootWidgets())
        {
            if (!rootWidget->IsHitTestable()) continue;

            auto widgetTransformInfo = info.AccountFor(rootWidget);

            if (!widgetTransformInfo.IsPointWithin(event->position)) continue;

            if (!rootWidget->NotifyScroll(event, widgetTransformInfo)) continue;

            break;
        }
    }


    Shared<Widget> Surface::AddChild(const Shared<Widget>& widget)
    {
        widget->SetDrawSize(GetDrawSize().Cast<float>());
        widget->SetRelativeOffset({0, 0});
        widget->NotifyAddedToSurface(this->GetSharedDynamic<Surface>());
        _rootWidgets.push_back(widget);
        _rootWidgetsMap.emplace(widget.get(), widget);
        return widget;
    }

    bool Surface::RemoveChild(const Shared<Widget>& widget)
    {
        if (_rootWidgetsMap.contains(widget.get())) return false;

        _rootWidgetsMap.erase(widget.get());

        for (auto i = 0; i < _rootWidgets.size(); i++)
        {
            if (_rootWidgets[i].get() == widget.get())
            {
                _rootWidgets.erase(_rootWidgets.begin() + i);
                break;
            }
        }

        return true;
    }

    Shared<graphics::DeviceImage> Surface::GetDrawImage() const
    {
        return _drawImage;
    }

    Shared<graphics::DeviceImage> Surface::GetCopyImage() const
    {
        return _copyImage;
    }

    void Surface::BeginMainPass(SurfaceFrame* frame,bool clear)
    {
        using namespace graphics;
        auto size = GetDrawSize().Cast<uint32_t>();
        vk::Extent2D renderExtent{size.x, size.y};
        auto cmd = frame->raw->GetCommandBuffer();
        std::optional<vk::ClearValue> drawImageClearColor = clear ? vk::ClearValue{vk::ClearColorValue{0.0f,0.0f,0.0f,0.0f}} : std::optional<vk::ClearValue>{};
        std::optional<vk::ClearValue> stencilImageClearColor = clear ? vk::ClearValue{vk::ClearDepthStencilValue{0.0f,1}} : std::optional<vk::ClearValue>{};
        auto drawImageAttachment = GraphicsModule::MakeRenderingAttachment(GetDrawImage(),vk::ImageLayout::eAttachmentOptimal,drawImageClearColor);
        auto stencilImageAttachment = GraphicsModule::MakeRenderingAttachment(_stencilImage,vk::ImageLayout::eDepthStencilAttachmentOptimal,stencilImageClearColor);
        beginRendering(cmd,renderExtent,drawImageAttachment,{},stencilImageAttachment);
        disableVertexInput(cmd);
        disableRasterizerDiscard(cmd);
        disableMultiSampling(cmd);
        disableCulling(cmd);
        disableDepthTest(cmd);
        setInputTopology(cmd, vk::PrimitiveTopology::eTriangleList);
        setPolygonMode(cmd, vk::PolygonMode::eFill);
        enableBlendingAlphaBlend(cmd, 0, 1);
        setRenderExtent(cmd,renderExtent);
        cmd.setStencilTestEnable(true);
        auto faceMask = vk::StencilFaceFlagBits::eFrontAndBack;
        cmd.setStencilReference(faceMask,0x1);
        cmd.setStencilCompareMask(faceMask,0x1);
        cmd.setStencilWriteMask(faceMask,0x1);
        cmd.setStencilOp(faceMask,vk::StencilOp::eKeep,vk::StencilOp::eKeep,vk::StencilOp::eKeep,vk::CompareOp::eNever);
        
        
        frame->activePass = MAIN_PASS_ID;
    }

    void Surface::EndActivePass(SurfaceFrame* frame)
    {
        if(frame->activePass.empty()) return;
        auto cmd = frame->raw->GetCommandBuffer();
        cmd.endRendering();
        frame->activePass.clear();
    }

    void Surface::DrawBatches(SurfaceFrame * frame, std::vector<QuadInfo>& quads)
    {
        if(quads.empty()) return;
        
        if(frame->activePass != MAIN_PASS_ID)
        {
            EndActivePass(frame);
            BeginMainPass(frame);
        }

        auto graphicsModule = GRuntime::Get()->GetModule<graphics::GraphicsModule>();
        auto widgetsModule = GRuntime::Get()->GetModule<WidgetsModule>();
        auto shader = widgetsModule->GetBatchShader();
        auto cmd = frame->raw->GetCommandBuffer();
        
        if(shader->Bind(cmd,true))
        {
            auto resource = shader->resources.at("batch_info");
            auto setLayout = shader->GetDescriptorSetLayouts().at(resource.set);
            auto pipelineLayout = shader->GetPipelineLayout();
            auto size = GetDrawSize().Cast<float>();
            Vec4<float> viewport{0.0f,0.0f,size.x,size.y};
            Matrix4<float> projection = static_cast<Matrix4<float>>(glm::ortho(0.0f, size.x, 0.0f, size.y));
            for(auto i = 0; i < quads.size(); i += BatchInfo::MAX_BATCH)
            {
                auto set = frame->raw->GetAllocator()->Allocate(setLayout);
                auto dataBuffer = graphicsModule->GetAllocator()->NewResourceBuffer(resource);
                auto totalQuads =  std::min(BatchInfo::MAX_BATCH,static_cast<int>(quads.size() - i));
                BatchInfo data{viewport,projection};
                memcpy(&data.quads,quads.data(),totalQuads * sizeof(QuadInfo));
                dataBuffer->Write(data);
                set->WriteBuffer(resource.binding,resource.type,dataBuffer);
                std::vector<vk::DescriptorSet> sets = {graphicsModule->GetResourceManager()->GetDescriptorSet(),set->GetInternalSet()};
                cmd.bindDescriptorSets(vk::PipelineBindPoint::eGraphics,pipelineLayout,0,sets,{});
                if(!dataBuffer || dataBuffer->IsDisposed())
                {
                    throw std::runtime_error("YO");
                }
                cmd.draw(totalQuads * 6,1,0,0);
            }
        }
        
        
    }

    void Surface::Draw(graphics::Frame* frame)
    {
        if (!_drawImage || !_copyImage) return;

        const auto cmd = frame->GetCommandBuffer();

        if (_rootWidgets.empty())
        {
            HandleDrawSkipped(frame);
            return;
        }

        DoHover();

        {
            std::vector<Shared<Widget>> widgets = _rootWidgets;
            TransformInfo transform{this};
            std::vector<Shared<DrawCommand>> drawCommands{};
            for (auto &widget : widgets)
            {
                widget->Collect(transform,drawCommands);
            }
            
            if(drawCommands.empty())
            {
                HandleDrawSkipped(frame);
                return;
            }

            HandleBeforeDraw(frame);
            
            SurfaceFrame surfFrame{this,frame,""};
            std::vector<QuadInfo> pendingQuads{};
            for (auto &drawCommand : drawCommands)
            {
                switch (drawCommand->GetType())
                {
                case DrawCommand::Type::Batched:
                    {
                        if(auto batched = std::dynamic_pointer_cast<BatchedDrawCommand>(drawCommand))
                        {
                            auto quads = batched->ComputeQuads();
                            pendingQuads.insert(pendingQuads.end(),quads.begin(),quads.end());
                        }
                    }
                    break;
                case DrawCommand::Type::Custom:
                    {
                        if(auto custom = std::dynamic_pointer_cast<CustomDrawCommand>(drawCommand))
                        {
                            custom->Draw(&surfFrame);
                        }
                    }
                    break;
                }
            }

            if(!pendingQuads.empty())
            {
                DrawBatches(&surfFrame,pendingQuads);
            }

            if(!surfFrame.activePass.empty())
            {
                EndActivePass(&surfFrame);
            }
        }
        
        HandleAfterDraw(frame);
    }

    void Surface::OnDispose(bool manual)
    {
        Disposable::OnDispose(manual);
        _drawImage.reset();
        _copyImage.reset();
    }
}
