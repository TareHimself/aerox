﻿using System.Runtime.InteropServices;
using rin.Framework.Core;
using rin.Framework.Core.Math;
using rin.Framework.Graphics;
using rin.Framework.Graphics.Shaders;
using rin.Framework.Views.Graphics;
using rin.Framework.Views.Graphics.Commands;

namespace ViewsTest;

public class PrettyShaderDrawCommand(Mat3 transform,Vec2<float> size,bool hovered) : CustomCommand
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Data
    {
        public required Mat4 Projection;
        public required Vec2<float> ScreenSize;
        public required Mat3 Transform;
        public required Vec2<float> Size;
        public required float Time;
        public required Vec2<float> Center;
    }
    public override bool WillDraw => true;

    public override ulong MemoryNeeded => (ulong)Marshal.SizeOf<Data>();


    private readonly IGraphicsShader _prettyShader = SGraphicsModule.Get().GraphicsShaderFromPath(Path.Join(SRuntime.ResourcesDirectory,"test","pretty.slang"));

    public override void Run(ViewsFrame frame, uint stencilMask, IDeviceBuffer? buffer = null)
    {
        frame.BeginMainPass();
        var cmd = frame.Raw.GetCommandBuffer();
        if (_prettyShader.Bind(cmd, true) && buffer != null)
        {
            var pushResource = _prettyShader.PushConstants.First().Value;
            var screenSize = frame.Surface.GetDrawSize().Cast<float>();
            var data = new Data()
            {
                Projection = frame.Projection,
                ScreenSize = screenSize,
                Transform = transform,
                Size = size,
                Time = (float)SRuntime.Get().GetTimeSeconds(),
                Center = hovered ?  frame.Surface.GetCursorPosition() : screenSize / 2.0f
            };
            buffer.Write(data);
            cmd.PushConstant(_prettyShader.GetPipelineLayout(), pushResource.Stages,buffer.GetAddress());
            cmd.Draw(6);
        }
    }
}