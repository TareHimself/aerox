﻿#pragma once
#include "WidgetList.hpp"
#include "aerox/widgets/WidgetContainer.hpp"

class WidgetScrollList : public WidgetList
{
protected:
    Shared<WidgetContainerSlot> MakeSlot(const Shared<Widget>& widget) override;
    void ApplyScroll() const;
    float _scroll = 0.0f;
public:
    WidgetScrollList(const Axis& axis);

    bool IsScrollable() const;
    float GetMaxScroll() const;
    float GetScroll() const;

    bool OnScroll(const Shared<ScrollEvent>& event) override;
    bool OnCursorDown(const Shared<CursorDownEvent>& event) override;

    void Collect(const TransformInfo& transform, std::vector<Shared<DrawCommand>>& drawCommands) override;

    TransformInfo ComputeChildTransform(const Shared<Widget>& widget, const TransformInfo& myTransform) override;
};