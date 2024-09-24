﻿#pragma once
#include "aerox/core/Disposable.hpp"
#include "aerox/core/delegates/DelegateList.hpp"
#include "aerox/core/math/Vec2.hpp"
#include "aerox/graphics/DeviceImage.hpp"
#include <optional>

#include "aerox/graphics/Frame.hpp"
#include "graphics/QuadInfo.hpp"

struct SurfaceFrame;
class Widget;
class CursorDownEvent;
    class ResizeEvent;
    class CursorMoveEvent;
    class CursorUpEvent;
    class ScrollEvent;

namespace SurfaceGlobals
{
   inline std::string MAIN_PASS_ID = "MAIN";
}
    class WidgetSurface : public Disposable
    {

        Shared<DeviceImage> _copyImage{};
        Shared<DeviceImage> _drawImage{};
        Shared<DeviceImage> _stencilImage{};
        std::optional<Vec2<float>> _lastCursorPosition{};
        std::unordered_map<Widget *,Shared<Widget>> _rootWidgetsMap{};
        std::vector<Shared<Widget>> _rootWidgets{};
        Shared<Widget> _focusedWidget{};
        std::vector<Shared<Widget>> _lastHovered{};

        void DoHover();
        
    public:
    
        std::vector<Shared<Widget>> GetRootWidgets() const;
        DEFINE_DELEGATE_LIST(onCursorDown,const Shared<CursorDownEvent>&)
        DEFINE_DELEGATE_LIST(onCursorUp,const Shared<CursorUpEvent>&)
        DEFINE_DELEGATE_LIST(onCursorMove,const Shared<CursorMoveEvent>&)
        DEFINE_DELEGATE_LIST(onResize,const Shared<ResizeEvent>&)
        DEFINE_DELEGATE_LIST(onScroll,const Shared<ScrollEvent>&)


        virtual void Init();

        virtual Vec2<int> GetDrawSize() const = 0;
        virtual Vec2<float> GetCursorPosition() const = 0;

        virtual void CreateImages();

        virtual void ClearFocus();

        virtual bool RequestFocus(const Shared<Widget>& widget);

        virtual void NotifyResize(const Shared<ResizeEvent>& event);
        virtual void NotifyCursorDown(const Shared<CursorDownEvent>& event);
        virtual void NotifyCursorUp(const Shared<CursorUpEvent>& event);
        virtual void NotifyCursorMove(const Shared<CursorMoveEvent>& event);
        virtual void NotifyScroll(const Shared<ScrollEvent>& event);

        virtual Shared<Widget> AddChild(const Shared<Widget>& widget);

        virtual bool RemoveChild(const Shared<Widget>& widget);

        Shared<DeviceImage> GetDrawImage() const;
        Shared<DeviceImage> GetCopyImage() const;

        virtual void BeginMainPass(SurfaceFrame * frame,bool clear = false);
        virtual void EndActivePass(SurfaceFrame * frame);

        virtual void DrawBatches(SurfaceFrame * frame,std::vector<QuadInfo>& quads);
        virtual void Draw(Frame * frame);


        virtual void HandleDrawSkipped(Frame* frame) = 0;
        virtual void HandleBeforeDraw(Frame* frame) = 0;
        virtual void HandleAfterDraw(Frame* frame) = 0;
        

        void OnDispose(bool manual) override;

        template<typename T,typename ...TArgs>
        std::enable_if_t<std::is_constructible_v<T,TArgs...> && std::is_base_of_v<Widget,T>,Shared<T>> AddChild(TArgs&&... args);
    };

    template <typename T, typename ... TArgs>
    std::enable_if_t<std::is_constructible_v<T, TArgs...> && std::is_base_of_v<Widget, T>, Shared<T>> WidgetSurface::
    AddChild(TArgs&&... args)
    {
        auto w = newShared<T>(std::forward<TArgs>(args)...);
        AddChild(w);
        return w;
    }