﻿using rin.Framework.Core.Math;
using rin.Framework.Views.Enums;
using rin.Framework.Views.Graphics;
using rin.Framework.Views.Layouts;

namespace rin.Framework.Views.Composite;

public enum FitMode
{
    Fill,
    Contain,
    Cover,
    None
}

/// <summary>
/// </summary>
public class Fitter : SingleSlotCompositeView
{

    private FitMode _fitFittingMode = FitMode.Fill;
    /// <summary>
    /// Adds the View to this container
    /// </summary>
    
    public Fitter() : base()
    {
        Clip = Clip.Bounds;
    }

    public FitMode FittingMode
    {
        get => _fitFittingMode;
        set
        {
            var old = _fitFittingMode;
            _fitFittingMode = value;
            if (_fitFittingMode != old)
            {
                FitContent(GetContentSize());
            }
        }
    }
    

    protected override Vector2<float> ComputeDesiredContentSize()
    {
        if (GetSlot() is { } slot)
        {
            return slot.Child.GetDesiredSize();
        }

        return 0.0f;
    }

    public static Vector2<float> ComputeContainSize(Vector2<float> drawSize, Vector2<float> widgetSize)
    {
        var widgetAspect = widgetSize.Y / widgetSize.X;
        var scaledWidgetSize = new Vector2<float>(drawSize.X, drawSize.X * widgetAspect);

        if (drawSize.Equals(scaledWidgetSize)) return scaledWidgetSize;

        return scaledWidgetSize.Y <= drawSize.Y
            ? scaledWidgetSize
            : new Vector2<float>(drawSize.Y / widgetAspect, drawSize.Y);
    }

    public static Vector2<float> ComputeCoverSize(Vector2<float> drawSize, Vector2<float> widgetSize)
    {
        var widgetAspect = widgetSize.Y / widgetSize.X;
        var scaledWidgetSize = new Vector2<float>(drawSize.X, drawSize.X * widgetAspect);

        if (drawSize.Equals(scaledWidgetSize)) return scaledWidgetSize;


        return scaledWidgetSize.Y <= drawSize.Y
            ? new Vector2<float>(drawSize.Y / widgetAspect, drawSize.Y)
            : scaledWidgetSize;
    }

    public Vector2<float> FitContent(Vector2<float> drawSize)
    {
        if (GetSlot() is { } slot)
        {
            var widget = slot.Child;
            var widgetSize = widget.GetDesiredSize();
            var newDrawSize = _fitFittingMode switch
                {
                    FitMode.Fill => drawSize,
                    FitMode.Contain => ComputeContainSize(drawSize, widgetSize),
                    FitMode.Cover => ComputeCoverSize(drawSize, widgetSize),
                    FitMode.None => widgetSize,
                    _ => throw new ArgumentOutOfRangeException()
                };

            widget.ComputeSize(newDrawSize);


            var halfSelfDrawSize = drawSize;
            halfSelfDrawSize /= 2.0f;
            var halfSlotDrawSize = newDrawSize;
            halfSlotDrawSize /= 2.0f;

            var diff = halfSelfDrawSize - halfSlotDrawSize;

            widget.Offset = diff;

            return drawSize;
        }

        return drawSize;
    }

    protected override Vector2<float> ArrangeContent(Vector2<float> availableSpace)
    {
        var desired = GetDesiredContentSize();
        return FitContent(new Vector2<float>(float.IsFinite(availableSpace.X) ? availableSpace.X : desired.X,
            float.IsFinite(availableSpace.Y) ? availableSpace.Y : desired.Y));
    }

    public override void OnChildInvalidated(View child, InvalidationType invalidation)
    {
        FitContent(GetContentSize());
    }

    public override IEnumerable<ISlot> GetSlots()
    {
        if (GetSlot() is { } slot)
        {
            return [slot];
        }

        return [];
    }
}