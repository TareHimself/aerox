﻿
using rin.Core.Math;

namespace rin.Widgets;

public struct Transform2d
{
    public float Angle = 0.0f;
    public Vector2<float> Translate = 0.0f;
    public Vector2<float> Scale = 1.0f;

    public Transform2d()
    {
    }
}