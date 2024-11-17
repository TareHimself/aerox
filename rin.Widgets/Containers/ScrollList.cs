﻿using rin.Core.Math;
using rin.Widgets.Events;
using rin.Widgets.Graphics;
using rin.Widgets.Graphics.Quads;

namespace rin.Widgets.Containers;

/// <summary>
///     Slot = <see cref="ContainerSlot" />
/// </summary>
public class ScrollList : List
{
    private float _mouseDownOffset;
    private Vector2<float> _mouseDownPos = new(0.0f);
    private float _offset;
    private float _maxOffset;

    public float MinBarSize = 40.0f;

    public ScrollList()
    {
        Clip = Clip.Bounds;
    }

    public float ScrollScale { get; set; } = 10.0f;

    public virtual bool ScrollBy(float delta)
    {
        var finalOffset = _offset + delta;
        return ScrollTo(finalOffset);
    }

    public virtual bool ScrollTo(float offset)
    {
        var scrollSize = GetMaxScroll();

        if (scrollSize < 0) return false;

        _offset = Math.Clamp(offset, 0, scrollSize);

        return Math.Abs(offset - _offset) > 0.001;
    }

    protected override Vector2<float> ArrangeContent(Vector2<float> spaceGiven)
    {
        var spaceTaken = base.ArrangeContent(spaceGiven);
        
        
        _maxOffset = Axis switch
        {
            Axis.Column => Math.Max(spaceTaken.Y - spaceGiven.Y.FiniteOr(spaceTaken.Y),0),
            Axis.Row => Math.Max(spaceTaken.X - spaceGiven.X.FiniteOr(spaceTaken.X),0),
            _ => throw new ArgumentOutOfRangeException()
        };
        ScrollTo(_offset);
        return new Vector2<float>(Math.Min(spaceTaken.X,spaceGiven.X),Math.Min(spaceTaken.Y,spaceGiven.Y));
    }

    public virtual float GetScroll()
    {
        return _offset;
    }

    public virtual float GetAxisSize()
    {
        var size = GetContentSize();
        return Axis switch
        {
            Axis.Row => size.X,
            Axis.Column => size.Y,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public virtual float GetDesiredAxisSize()
    {
        var desiredSize = GetDesiredContentSize();

        return Axis switch
        {
            Axis.Row => desiredSize.X,
            Axis.Column => desiredSize.Y,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public virtual float GetMaxScroll() => _maxOffset;

    public virtual bool IsScrollable()
    {
        return GetMaxScroll() > 0;
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        if (!IsScrollable()) return base.OnScroll(e);

        switch (Axis)
        {
            case Axis.Column:
            {
                var delta = e.Delta.Y * -1.0f;
                return ScrollBy(delta * ScrollScale);
            }
            case Axis.Row:
            {
                var delta = e.Delta.X;
                return ScrollBy(delta * ScrollScale);
            }
            default:
                return base.OnScroll(e);
        }
    }

    public override void Collect(TransformInfo info, DrawCommands drawCommands)
    {
        base.Collect(info, drawCommands);
        if (IsVisible && IsScrollable())
        {
            var scroll = GetScroll();
            var maxScroll = GetMaxScroll();
            var axisSize = GetAxisSize();
            var desiredAxisSize = axisSize + maxScroll;

            var barSize = Math.Max(MinBarSize, axisSize - (desiredAxisSize - axisSize));
            var availableDist = axisSize - barSize;
            var drawOffset = (float)(availableDist * (Math.Max(scroll, 0.0001) / maxScroll));

            var size = GetContentSize();

            var transform = info.Transform.Translate(new Vector2<float>(size.X - 10.0f, drawOffset));
            drawCommands.AddRect(transform, new Vector2<float>(10.0f, barSize), color: Color.White, borderRadius: 7.0f);
        }
    }

    protected override Matrix3 ComputeSlotOffset(ContainerSlot slot)
    {
        return Matrix3.Identity.Translate(Axis switch
        {
            Axis.Row => new Vector2<float>(-GetScroll(), 0.0f),
            Axis.Column => new Vector2<float>(0.0f, -GetScroll()),
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    protected override bool OnCursorDown(CursorDownEvent e)
    {
        _mouseDownOffset = _offset;
        _mouseDownPos = e.Position.Cast<float>();
        return true;
    }

    protected override bool OnCursorMove(CursorMoveEvent e)
    {
        if (IsPendingMouseUp)
        {
            var pos = _mouseDownPos - e.Position.Cast<float>();

            return ScrollTo(Axis switch
            {
                Axis.Row => _mouseDownOffset + pos.X,
                Axis.Column => _mouseDownOffset + pos.Y,
                _ => throw new ArgumentOutOfRangeException()
            });
            // var delta = Direction switch
            // {
            //     Axis.Horizontal => e.Delta.X,
            //     Axis.Vertical => e.Delta.Y * -1.0f,
            //     _ => throw new ArgumentOutOfRangeException()
            // };

            // return ScrollBy((float)delta);
        }

        return base.OnCursorMove(e);
    }
}