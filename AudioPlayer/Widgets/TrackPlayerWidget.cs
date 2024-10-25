﻿using System.Net;
using aerox.Runtime.Audio;
using aerox.Runtime.Graphics;
using aerox.Runtime.Math;
using aerox.Runtime.Widgets;
using aerox.Runtime.Widgets.Containers;
using aerox.Runtime.Widgets.Content;
using aerox.Runtime.Widgets.Events;
using aerox.Runtime.Widgets.Graphics;
using MathNet.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using TerraFX.Interop.Vulkan;
using Image = SixLabors.ImageSharp.Image;

namespace AudioPlayer.Widgets;

public class TrackPlayerWidget : Overlay
{
    private WText NameText => GetSlot(1)!.GetWidget<BackgroundBlur>()!.GetSlot(0)!.GetWidget<List>()!.GetSlot(0)!.GetWidget<WText>()!;
    private WText StatusText => GetSlot(1)!.GetWidget<BackgroundBlur>()!.GetSlot(0)!.GetWidget<List>()!.GetSlot(1)!.GetWidget<WText>()!;
    private readonly AudioStream _stream;

    public string Name
    {
        get => NameText.Content;
        set => NameText.Content = value;
    }

    public TrackPlayerWidget(string name, AudioStream stream) : base()
    {
        AddChild(new Sizer()
        {

        });
        AddChild(new BackgroundBlur(new List([
            new WText("NAME", 40)
            {
                Padding = new WidgetPadding()
                {
                    Top = 0.0f,
                    Bottom = 5.0f,
                }
            },
            new WText("00:00 - 00:00", 30)
            {
                Padding = new WidgetPadding()
                {
                    Top = 5.0f,
                    Bottom = 5.0f,
                }
            }
        ])
        {
            Direction = List.Axis.Vertical,
            Padding = new WidgetPadding(5.0f, 10.0f)
        }){
            Tint = Color.Black.Clone(a: 0.2f),
        });
        NameText.Content = name;

        _stream = stream;
        Padding = 10.0f;
        FetchCover().ConfigureAwait(false);
    }

    public async Task FetchCover()
    {
        // var data = (await SAudioPlayer.Get().SpClient.Search.GetTracksAsync($"{NameText.Content} official track")).FirstOrDefault();
        // if (data == null)
        // {
        //     Console.WriteLine($"Failed to find art for {NameText.Content}");
        //     return;
        // }
        //
        //
        // var thumb = data.Album.Images.MaxBy(c => c.Height * c.Width)!.Url;
        //     
        // Console.WriteLine($"Using thumb {thumb} for {NameText.Content}");
        // GetChildSlot(0)?.GetWidget<Sizer>()?.AddChild(new Fitter(new AsyncWebImage(thumb))
        // {
        //     FittingMode = FitMode.Cover
        // });
    }

    protected string FormatTime(double secs)
    {
        return
            $"{((int)Math.Floor(secs / 60)).ToString().PadLeft(2, '0')}:{((int)(secs % 60)).ToString().PadLeft(2, '0')}";
    }
    
    public override void CollectContent(TransformInfo info, DrawCommands drawCommands)
    {
        StatusText.Content = $"{FormatTime(_stream.Position)} - {FormatTime(_stream.Length)}";
        base.CollectContent(info, drawCommands);
    }

    protected override bool OnCursorDown(CursorDownEvent e) => _stream.IsPlaying ? _stream.Pause() : _stream.Play();

    protected override void OnDispose(bool isManual)
    {
        base.OnDispose(isManual);
        _stream.Dispose();
    }
}