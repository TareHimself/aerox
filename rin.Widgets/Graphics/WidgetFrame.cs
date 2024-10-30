﻿using System.Runtime.InteropServices;
using rin.Graphics;
using rin.Core.Math;

namespace rin.Widgets.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct SimpleRectPush
{
    public Matrix3 Transform;

    public Vector2<float> Size;

    public Vector4<float> BorderRadius;

    public Vector4<float> Color;
}

public class WidgetFrame
{
    //public readonly AeroxLinkedList<Command> DrawCommandList = [];
    public readonly Frame Raw;
    public readonly Surface Surface;
    public string ActivePass = "";
    public bool IsMainPassActive => ActivePass == Surface.MainPassId;
    public Matrix4 Projection;

    public WidgetFrame(Surface surface, Frame raw)
    {
        Surface = surface;
        Raw = raw;
        var size = surface.GetDrawSize();
        Projection = Glm.Orthographic(0, size.X, 0, size.Y);
        //raw.OnReset += CleanupCommands;
    }

    // public WidgetFrame AddRect(Matrix3 transform, Vector2<float> size, Vector4<float>? borderRadius = null,
    //     Color? color = null)
    // {
    //     return AddCommands(new SimpleRect(transform, size)
    //     {
    //         BorderRadius = borderRadius,
    //         Color = color
    //     });
    // }
    //
    // public WidgetFrame AddMaterialRect(MaterialInstance materialInstance, WidgetPushConstants pushConstants)
    // {
    //     return AddCommands(new MaterialRect(materialInstance, pushConstants));
    // }
    //
    // public WidgetFrame AddCommands(params Command[] commands)
    // {
    //     LinkedList<Command> lCommands = [];
    //     foreach (var command in commands)
    //     {
    //         DrawCommandList.InsertBack(command);
    //     }
    //     return this;
    // }
    //
    // public void CleanupCommands(Frame frame)
    // {
    //     foreach (var widgetFrameDrawCommand in DrawCommandList) widgetFrameDrawCommand.Value.Dispose();
    //     DrawCommandList.Clear();
    // }


    public static implicit operator Frame(WidgetFrame frame)
    {
        return frame.Raw;
    }
}