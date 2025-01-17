﻿using rin.Framework.Core;
using rin.Framework.Core.Math;

namespace rin.Framework.Views.Graphics;

public struct ClippingRect
{
    public Mat3 Matrix;
    public Vec2<float> Size;
}

public class TransformInfo : ICloneable<TransformInfo>
{
    public Mat3 Transform;
    /// <summary>
    /// This is only used for intersection estimates
    /// </summary>
    public Vec2<float> Size;
    public int Depth;

    protected Rect _clipRect;
    
    /// <summary>
    /// True if the target of this draw info will be occluded (tests are done using an AABB approach
    /// </summary>
    public bool Occluded = false;

    public TransformInfo(Mat3 transform,Vec2<float> size,int depth,bool checkOcclusion = false,TransformInfo? parent = null,Clip clip = Clip.None)
    {
        Transform = transform;
        Size = size;
        Depth = depth;
        if (checkOcclusion)
        {
            var AABB = ComputeAxisAlignedBoundingRect(this);
            if (parent != null)
            {
                _clipRect = clip switch
                {
                    Clip.None => parent._clipRect,
                    Clip.Bounds => AABB.Clamp(parent._clipRect),
                    _ => throw new ArgumentOutOfRangeException(nameof(clip), clip, null)
                };
            }
            else
            {
                _clipRect = AABB;
            }

            Occluded = !AABB.IntersectsWith(_clipRect);
        }
        else
        {
            _clipRect = new Rect(float.NegativeInfinity, float.PositiveInfinity);
        }
    }
    
    public TransformInfo(Surface surface)
    {
        Transform = Mat3.Identity;
        Size = surface.GetDrawSize().Cast<float>();
        Depth = 0;
        _clipRect = ComputeAxisAlignedBoundingRect(this);
    }

    public Rect ToRect() => ComputeAxisAlignedBoundingRect(this);

    public TransformInfo Clone()
    {
        return new TransformInfo(Transform,Size,Depth)
        {
            _clipRect = _clipRect,
            Occluded = Occluded
        };
    }

    //public static Vector2<float>[] ClippingRectToPoints()


    public static Rect ComputeAxisAlignedBoundingRect(TransformInfo target)
    {
        var tl = new Vec2<float>(0.0f);
        var br = tl + target.Size;
        var tr = new Vec2<float>(br.X, tl.Y);
        var bl = new Vec2<float>(tl.X, br.Y);

        tl = tl.ApplyTransformation(target.Transform);
        br = br.ApplyTransformation(target.Transform);
        tr = tr.ApplyTransformation(target.Transform);
        bl = bl.ApplyTransformation(target.Transform);

        var p1AABB = new Vec2<float>(
            System.Math.Min(
                System.Math.Min(tl.X, tr.X),
                System.Math.Min(bl.X, br.X)
            ),
            System.Math.Min(
                System.Math.Min(tl.Y, tr.Y),
                System.Math.Min(bl.Y, br.Y)
            )
        );
        var p2AABB = new Vec2<float>(
            System.Math.Max(
                System.Math.Max(tl.X, tr.X),
                System.Math.Max(bl.X, br.X)
            ),
            System.Math.Max(
                System.Math.Max(tl.Y, tr.Y),
                System.Math.Max(bl.Y, br.Y)
            )
        );

        return new Rect
        {
            Offset = p1AABB,
            Size = p2AABB - p1AABB
        };
    }

    public bool IntersectsWith(TransformInfo target)
    {
        var targetAARect = ComputeAxisAlignedBoundingRect(target);
        var myAARect = ComputeAxisAlignedBoundingRect(this);
        return targetAARect.IntersectsWith(myAARect);
    }

    public bool PointWithin(Vec2<float> point)
    {
        var tl = new Vec2<float>(0.0f);
        var br = tl + Size;
        var tr = new Vec2<float>(br.X, tl.Y);
        var bl = new Vec2<float>(tl.X, br.Y);

        tl = tl.ApplyTransformation(Transform);
        br = br.ApplyTransformation(Transform);
        tr = tr.ApplyTransformation(Transform);
        bl = bl.ApplyTransformation(Transform);

        var p1AABB = new Vec2<float>(
            System.Math.Min(
                System.Math.Min(tl.X, tr.X),
                System.Math.Min(bl.X, br.X)
            ),
            System.Math.Min(
                System.Math.Min(tl.Y, tr.Y),
                System.Math.Min(bl.Y, br.Y)
            )
        );
        var p2AABB = new Vec2<float>(
            System.Math.Max(
                System.Math.Max(tl.X, tr.X),
                System.Math.Max(bl.X, br.X)
            ),
            System.Math.Max(
                System.Math.Max(tl.Y, tr.Y),
                System.Math.Max(bl.Y, br.Y)
            )
        );

        // Perform AABB test first
        if (!point.Within(new Rect
            {
                Offset = p1AABB,
                Size = p2AABB - p1AABB
            })) return false;

        var top = tr - tl;
        var right = br - tr;
        var bottom = bl - br;
        var left = tl - bl;
        var pTop = point - tl;
        var pRight = point - tr;
        var pBottom = point - br;
        var pLeft = point - bl;
        
        var a = top.Acos(pTop);
        var b = right.Cross(pRight);
        var c = bottom.Cross(pBottom);
        var d = left.Cross(pLeft);

        if (a >= 0)
        {
            return b >= 0 && c >= 0 && d >= 0;
        }
        else
        {
            return b < 0 && c < 0 && d < 0;
        }
    }
}