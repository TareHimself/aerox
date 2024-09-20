﻿#include "aerox/widgets/containers/Fitter.hpp"

#include "aerox/widgets/ContainerSlot.hpp"

namespace aerox::widgets
{
    FitMode Fitter::GetMode() const
    {
        return _mode;
    }

    void Fitter::SetMode(FitMode mode)
    {
        _mode = mode;
    }

    Vec2<float> Fitter::ComputeDesiredSize()
    {
        if(auto slot = GetSlot(0))
        {
            return slot->GetWidget()->GetDesiredSize();
        }

        return Vec2{0.0f};
    }

    size_t Fitter::GetMaxSlots() const
    {
        return 1;
    }

    void Fitter::ArrangeSlots(const Vec2<float>& drawSize)
    {
        SizeContent(drawSize);
    }

    Vec2<float> Fitter::ComputeContainSize(const Vec2<float>& drawSize, const Vec2<float>& widgetSize)
    {
        auto aspect = widgetSize.y / widgetSize.x;
        Vec2 scaledWidgetSize{drawSize.x,drawSize.x * aspect};

        if(drawSize.NearlyEquals(scaledWidgetSize,0.001)) return scaledWidgetSize;

        return scaledWidgetSize.y < drawSize.y  ? scaledWidgetSize : Vec2{drawSize.y / aspect,drawSize.y};
    }

    Vec2<float> Fitter::ComputeCoverSize(const Vec2<float>& drawSize, const Vec2<float>& widgetSize)
    {
        auto aspect = widgetSize.y / widgetSize.x;
        Vec2 scaledWidgetSize{drawSize.x,drawSize.x * aspect};

        if(drawSize.NearlyEquals(scaledWidgetSize,0.001)) return scaledWidgetSize;

        return scaledWidgetSize.y < drawSize.y  ? Vec2{drawSize.y / aspect,drawSize.y} : scaledWidgetSize;
    }

    void Fitter::SizeContent(const Vec2<float>& size) const
    {
        if(auto slot = GetSlot(0))
        {
            auto widget = slot->GetWidget();
            auto widgetSize = widget->GetDesiredSize();
            auto newDrawSize = widgetSize;
            if(!widgetSize.NearlyEquals(size,0.001))
            {
                switch (GetMode())
                {
                case FitMode::None:
                    break;
                case FitMode::Fill:
                    {
                        newDrawSize = size;
                    }
                    break;
                case FitMode::Contain:
                    {
                        newDrawSize = ComputeContainSize(widgetSize,size);
                    }
                    break;
                case FitMode::Cover:
                    {
                        newDrawSize = ComputeCoverSize(widgetSize,size);
                    }
                    break;
                }
            }

            if(!widgetSize.NearlyEquals(newDrawSize,0.001)) widget->SetDrawSize(newDrawSize);
            
            auto halfSelfDrawSize = size / 2.0f;
            auto halfSlotDrawSize = newDrawSize / 2.0f;

            auto diff = halfSelfDrawSize - halfSlotDrawSize;

            if(!widget->GetRelativeOffset().NearlyEquals(diff,0.001)) widget->SetRelativeOffset(diff);
        }
    }
}
