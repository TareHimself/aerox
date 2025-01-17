﻿using rin.Framework.Core.Math;
using rin.Framework.Graphics.Windows;
using rin.Framework.Views.Graphics;


namespace rin.Framework.Views.Events;

public class CursorUpEvent(Surface surface, CursorButton button, Vec2<float> position)
    : Event(surface)
{
    public Vec2<float> Position = position;
    public CursorButton Button = button;
}