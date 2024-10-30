﻿using rin.Widgets.Graphics;
using rin.Widgets.Graphics.Commands;

namespace rin.Widgets;

public enum CommandType
{
    None,
    ClipDraw,
    ClipClear,
    BatchedDraw,
    CustomDraw
}

public class FinalDrawCommand
{
    public IBatch? Batch;
    public Clip[] Clips = [];
    public uint Mask = 0x1;
    public CustomCommand? Custom;
    public CommandType Type = CommandType.None;
}