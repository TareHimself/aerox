﻿using rin.Framework.Views.Graphics.Commands;

namespace rin.Framework.Views.Graphics;

public enum CommandType
{
    None,
    ClipDraw,
    ClipClear,
    BatchedDraw,
    Custom
}

public class FinalDrawCommand
{
    public IBatch? Batch;
    public Graphics.StencilClip[] Clips = [];
    public uint Mask = 0x1;
    public CustomCommand? Custom;
    public CommandType Type = CommandType.None;
}