﻿using aerox.Runtime.Extensions;
using aerox.Runtime.Math;
using aerox.Runtime.Widgets.Events;

namespace aerox.Runtime.Widgets.Containers;

/// <summary>
///     A container that draws children on top of each other
/// </summary>
public class Overlay : Container
{
    public Overlay(IEnumerable<Widget> children) : base(children)
    {
    }


    protected override Size2d ComputeContentDesiredSize()
    {
        return GetSlots().Aggregate(new Size2d(), (size, slot) =>
        {
            var slotSize = slot.GetWidget().GetDesiredSize();
            size.Height = System.Math.Max(size.Height, slotSize.Height);
            size.Width = System.Math.Max(size.Width, slotSize.Width);
            return size;
        });
    }
    
    public override uint GetMaxSlots()
    {
        return 0;
    }

    protected override void ArrangeSlots(Size2d drawSize)
    {
        foreach (var slot in GetSlots())
        {
            slot.GetWidget().SetOffset(new Vector2<float>(0, 0));
            slot.GetWidget().SetSize(drawSize);
        }
    }

    // protected override bool ChildrenReceiveScroll(ScrollEvent e, TransformInfo info)
    // {
    //     var position = e.Position.Cast<float>();
    //     var myInfo = ComputeChildTransform()
    //     foreach (var slot in GetSlots().AsReversed())
    //         if (myInfo.AccountFor(slot.GetWidget()).PointWithin(position))
    //             return slot.GetWidget().ReceiveScroll(e, myInfo);
    //
    //     return false;
    // }
    //
    // protected override void ChildrenReceiveCursorEnter(CursorMoveEvent e, TransformInfo info, List<Widget> items)
    // {
    //     var myInfo = info.AccountFor(this);
    //     var position = e.Position.Cast<float>();
    //     foreach (var slot in _slot.AsReversed())
    //         if (myInfo.AccountFor(slot.GetWidget()).PointWithin(position))
    //         {
    //             slot.GetWidget().ReceiveCursorEnter(e, myInfo, items);
    //             break;
    //         }
    // }
    //
    // protected override Widget? ChildrenReceiveCursorDown(CursorDownEvent e, TransformInfo info)
    // {
    //     var position = e.Position.Cast<float>();
    //     var myInfo = info.AccountFor(this);
    //     foreach (var slot in _slot.AsReversed())
    //         if (myInfo.AccountFor(slot.GetWidget()).PointWithin(position))
    //             return slot.GetWidget().ReceiveCursorDown(e, myInfo);
    //
    //     return null;
    // }
    //
    // protected override bool ChildrenReceiveCursorMove(CursorMoveEvent e, TransformInfo info)
    // {
    //     var position = e.Position.Cast<float>();
    //     foreach (var slot in _slot.AsReversed())
    //     {
    //         var slotInfo = info.AccountFor(slot.GetWidget());
    //         if (slotInfo.PointWithin(position))
    //             return slot.GetWidget().ReceiveCursorMove(e, slotInfo);
    //     }
    //
    //     return false;
    // }
}