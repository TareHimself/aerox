﻿#pragma once
#include "Event.hpp"
#include "aerox/core/Disposable.hpp"
#include "aerox/core/math/Vec2.hpp"

class ResizeEvent : public Event
{
public:
    Vec2<float> size;
    ResizeEvent(const Shared<WidgetSurface>& inSurface,const Vec2<float>& inSize);
};