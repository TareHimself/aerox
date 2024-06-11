﻿using aerox.Runtime.Graphics.Material;

namespace aerox.Runtime.Widgets.Draw.Commands;

public class MaterialRect : DrawCommand
{
    private readonly MaterialInstance _materialInstance;
    private readonly WidgetPushConstants _pushConstants;

    public MaterialRect(MaterialInstance materialInstance, WidgetPushConstants pushConstant)
    {
        materialInstance.Reserve();
        _materialInstance = materialInstance;
        _pushConstants = pushConstant;
    }

    protected override void OnDispose(bool isManual)
    {
        base.OnDispose(isManual);
        _materialInstance.Dispose();
    }
    

    protected override void Draw(WidgetFrame frame)
    {
        _materialInstance.BindTo(frame);
        _materialInstance.Push(frame.Raw.GetCommandBuffer(),  _pushConstants);
        Quad(frame);
    }
}