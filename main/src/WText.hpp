﻿#pragma once
#include "rin/widgets/USDFContainer.hpp"
#include "rin/widgets/Widget.hpp"

class WText : public Widget
{
public:
    USDFContainer sdf{};
    void CollectContent(const TransformInfo& transform, WidgetDrawCommands& drawCommands) override;
    Vec2<float> ComputeContentSize() override;
};
