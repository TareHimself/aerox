﻿using rin.Audio;
using rin.Core.Math;
using rin.Widgets;
using rin.Widgets.Containers;
using rin.Widgets.Graphics;
using Clip = rin.Widgets.Clip;
using Rect = rin.Widgets.Containers.Rect;

namespace AudioPlayer.Widgets;

public class MainPanel : Panel
{
    private readonly ScrollList _trackPlayers = new ScrollList()
    {
        Axis = Axis.Column,
        Clip = Clip.None//Clip.Bounds
    };

    public MainPanel()
    {
        var filePicker = new FilePicker();
        Slots =
        [
            new PanelSlot
            {
                Child = _trackPlayers,
                //SizeToContent = true,
                MinAnchor = 0.0f,
                MaxAnchor = 1.0f//new Vector2<float>(0.5f, 0.5f)
            },
            new PanelSlot
            {
                Child = new Rect
                {
                    Child = new FpsWidget
                    {
                        FontSize = 30,
                    },
                    Padding = new Padding(20.0f),
                    BorderRadius = 10.0f,
                    BackgroundColor = Color.Black.Clone(a: 0.7f)
                },
                SizeToContent = true,
                MinAnchor = new Vector2<float>(1.0f, 0.0f),
                MaxAnchor = new Vector2<float>(1.0f, 0.0f),
                Alignment = new Vector2<float>(1.0f, 0.0f)
            },
            new PanelSlot
            {
                Child = filePicker,
                MaxAnchor = 0.5f,
                MinAnchor = 0.5f,
                Alignment = 0.5f,
                SizeToContent = true
            }
        ];

        filePicker.OnFileSelected += s =>
        {
            if (s.Length == 0) return;
            OnFileSelected(s);
        };
    }
    
    private void OnFileSelected(string[] files)
    {
        foreach (var file in files)
        {
            var player = new TrackPlayer(Path.GetFileNameWithoutExtension(file), AudioStream.FromFile(file));
            _trackPlayers.AddChild(player);
        }
    }
}