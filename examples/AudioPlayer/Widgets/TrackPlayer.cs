﻿using rin.Audio;
using rin.Core;
using rin.Core.Animation;
using rin.Core.Math;
using rin.Widgets;
using rin.Widgets.Animation;
using rin.Widgets.Containers;
using rin.Widgets.Content;
using rin.Widgets.Events;
using rin.Widgets.Graphics;

namespace AudioPlayer.Widgets;

public class TrackPlayer : Overlay
{
    private readonly TextBox _nameText = new TextBox("NAME", 40)
    {
        Padding = new Padding()
        {
            Top = 0.0f,
            Bottom = 5.0f,
        },
        WrapContent = true
    };

    private readonly TextBox _currentTimeText = new TextBox("00:00", 30)
    {
        Padding = new Padding()
        {
            Top = 5.0f,
            Bottom = 5.0f,
            Right = 5.0f
        }
    };

    private readonly TextBox _endTimeText = new TextBox("00:00", 30)
    {
        Padding = new Padding()
        {
            Top = 5.0f,
            Bottom = 5.0f,
            Left = 5.0f
        }
    };

    private readonly AudioStream _stream;
    private double _lastTime = SRuntime.Get().GetTimeSeconds();

    private readonly Panel _backgroundContainer = new Panel();

    public string Name
    {
        get => _nameText.Content;
        set => _nameText.Content = value;
    }

    public TrackPlayer(string name, AudioStream stream)
    {
        _nameText.Content = name;

        _stream = stream;

        Children =
        [
            _backgroundContainer,
            new List
            {
                Axis = Axis.Column,
                Padding = new Padding(5.0f, 10.0f),
                Slots =
                [
                    new ListContainerSlot
                    {
                        Child = _nameText
                    },
                    new ListContainerSlot
                    {
                        Fit = CrossFit.Fill,
                        Child = new FlexBox
                        {
                            Axis = Axis.Row,
                            Slots =
                            [
                                new FlexBoxSlot
                                {
                                    Child = new Sizer
                                    {
                                        Child = _currentTimeText,
                                        WidthOverride = 100
                                    }
                                },
                                new FlexBoxSlot
                                {
                                    Child = new Sizer
                                    {
                                        Child = new ProgressBar(() => (float)(_stream.Position / _stream.Length))
                                        {
                                            BackgroundColor = Color.Black,
                                            BorderRadius = 6.0f
                                        },
                                        HeightOverride = 8
                                    },
                                    Flex = 1,
                                    Align = CrossAlign.Center
                                },
                                new FlexBoxSlot
                                {
                                    Child = new Sizer
                                    {
                                        Child = _endTimeText,
                                        WidthOverride = 100
                                    }
                                }
                            ]
                        }
                    }
                ]
            }
        ];


        _endTimeText.Content = FormatTime(stream.Length);
        Padding = 10.0f;
        FetchCover().ConfigureAwait(false);
        Scale = new Vector2<float>(0.0f, 1.0f);
    }

    private async Task FetchCover()
    {
        var data = (await SAudioPlayer.Get().SpClient.Search.GetTracksAsync($"{_nameText.Content} official track"))
            .FirstOrDefault();
        if (data == null)
        {
            Console.WriteLine($"Failed to find art for {_nameText.Content}");
            return;
        }


        var thumb = data.Album.Images.MaxBy(c => c.Height * c.Width)!.Url;

        Console.WriteLine($"Using thumb {thumb} for {_nameText.Content}");
        _backgroundContainer.AddChild(new PanelSlot
        {
            Child = new AsyncWebCover(thumb)
            {
                BorderRadius = 20.0f
            },
            MinAnchor = 0.0f,
            MaxAnchor = 1.0f
        });
        this.ScaleTo(new Vector2<float>(0.0f, 1.0f), 1.0f, 0.2f,easingFunction: EasingFunctions.EaseInExpo);
    }

    private static string FormatTime(double secs)
    {
        return
            $"{((int)Math.Floor(secs / 60)).ToString().PadLeft(2, '0')}:{((int)(secs % 60)).ToString().PadLeft(2, '0')}";
    }

    public override void CollectContent(TransformInfo info, DrawCommands drawCommands)
    {
        _currentTimeText.Content = FormatTime(_stream.Position);
        base.CollectContent(info, drawCommands);
    }

    protected override bool OnCursorDown(CursorDownEvent e) => _stream.IsPlaying ? _stream.Pause() : _stream.Play();

    protected override void OnCursorEnter(CursorMoveEvent e)
    {
        base.OnCursorEnter(e);
        this.StopAll().TranslateTo(null, new Vector2<float>(40.0f, 0.0f), 0.2,
            easingFunction: EasingFunctions.EaseInOutCubic);
    }

    protected override void OnCursorLeave(CursorMoveEvent e)
    {
        base.OnCursorLeave(e);
        this.StopAll().TranslateTo(null, new Vector2<float>(0.0f, 0.0f), 0.2,
            easingFunction: EasingFunctions.EaseInOutCubic);
    }
}