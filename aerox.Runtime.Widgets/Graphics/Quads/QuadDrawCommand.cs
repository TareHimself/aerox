﻿using aerox.Runtime.Widgets.Graphics.Commands;

namespace aerox.Runtime.Widgets.Graphics.Quads;

public class QuadDrawCommand : BatchedCommand
{
    private readonly List<Quad> _quads = [];

    public QuadDrawCommand(IEnumerable<Quad> quads)
    {
        _quads.AddRange(quads);
    }
    
    public override IBatchRenderer GetBatchRenderer() => SWidgetsModule.Get().GetBatchRenderer<QuadBatchRenderer>();

    public IEnumerable<Quad> GetQuads() => _quads;
}