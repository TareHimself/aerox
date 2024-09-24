﻿using aerox.Runtime.Math;

namespace aerox.Runtime.Widgets.Defaults.Containers;

public class Sizer : Container
{
    private float? _heightOverride;
    private float? _widthOverride;

    public Sizer(Widget child) : base(child)
    {
    }

    public Sizer()
    {
    }

    public float? WidthOverride
    {
        get => _widthOverride;
        set
        {
            _widthOverride = value;
            CheckSize();
        }
    }

    public float? HeightOverride
    {
        get => _heightOverride;
        set
        {
            _heightOverride = value;
            CheckSize();
        }
    }

    protected override Size2d ComputeDesiredSize()
    {
        if (Slots.Count == 0) return new Size2d();

        var slot = Slots[0];
        var desiredSize = slot.GetWidget().GetDesiredSize();
        return new Size2d(WidthOverride ?? desiredSize.Width, HeightOverride ?? desiredSize.Height);
    }


    public override void Collect(WidgetFrame frame, DrawInfo info)
    {
        foreach (var slot in Slots.ToArray())
        {
            var slotDrawInfo = info.AccountFor(slot.GetWidget());
            if (slotDrawInfo.Occluded) continue;
            slot.GetWidget().Collect(frame, info.AccountFor(slot.GetWidget()));
        }
    }

    public override uint GetMaxSlots()
    {
        return 1;
    }

    protected override void ArrangeSlots(Size2d drawSize)
    {
        if (Slots.Count == 0) return;

        var slot = Slots[0];
        slot.GetWidget().SetRelativeOffset(new Vector2<float>(0, 0));
        slot.GetWidget().SetDrawSize(drawSize.Clone());
    }
}