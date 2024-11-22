﻿using rin.Core;
using rin.Core.Animation;
using rin.Core.Extensions;
using rin.Core.Math;
using rin.Widgets;
using rin.Widgets.Animation;
using rin.Widgets.Containers;
using rin.Widgets.Content;
using rin.Widgets.Graphics.Commands;
using rin.Widgets.Events;
using rin.Widgets.Graphics;
using rin.Widgets.Graphics.Quads;

namespace AudioPlayer.Widgets;

public class FilePicker : Button
{
    private bool _hasInit = false;
    private double _animDuration = 0.1;

    public event Action<string[]>? OnFileSelected;
    
    protected readonly TextBox StatusText = new TextBox("Select File's");
    
    public FilePicker() : base()
    {
        BackgroundColor = Color.Red;
        BorderRadius = 10.0f;
        Padding = 20.0f;
    }

    protected override void OnAddedToSurface(Surface surface)
    {
        base.OnAddedToSurface(surface);
        if (_hasInit) return;
        _hasInit = true;
        StatusText.Padding = 20.0f;
        AddChild(StatusText);
        // this.RunAction(new SimpleAnimationAction((s, time) =>
        // {
        //     // var num = (float)Math.Sin(time);
        //     // s.Target.Angle = 60.0f * num;
        //     return true;
        // }));
    }

    protected void FileSelected(string[] files)
    {
        OnFileSelected?.Invoke(files);
        StatusText.Content = "Select File's";
    }

    protected override bool OnCursorDown(CursorDownEvent e)
    {
        StatusText.Content = "Selecting...";
        Platform.SelectFileAsync("Select File's To Play",multiple:true,filter:"*.wav;*.ogg;*.flac;*.mp3").Then(FileSelected).ConfigureAwait(false);
        return base.OnCursorDown(e);
    }

    protected override void OnDispose(bool isManual)
    {
        base.OnDispose(isManual);
        StatusText.Dispose();
    }

    protected override void OnCursorEnter(CursorMoveEvent e)
    {
        this
            .StopAll()
            .Transition(BackgroundColor, Color.Green, (c) => BackgroundColor = c,_animDuration)
            .Transition(BorderRadius,20.0f,(c) => BorderRadius = c,_animDuration);
        base.OnCursorEnter(e);
    }

    protected override void OnCursorLeave(CursorMoveEvent e)
    {
        this
            .StopAll()
            .Transition(BackgroundColor, Color.Red, (c) => BackgroundColor = c,_animDuration)
            .Transition(BorderRadius,10.0f,(c) => BorderRadius = c,_animDuration);
        base.OnCursorLeave(e);
    }
    
}