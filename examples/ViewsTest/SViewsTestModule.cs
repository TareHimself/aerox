﻿using rin.Framework.Core;
using rin.Framework.Core.Animation;
using rin.Framework.Core.Extensions;
using rin.Framework.Core.Math;
using rin.Framework.Graphics;
using rin.Framework.Graphics.Windows;
using rin.Framework.Views;
using rin.Framework.Views.Animation;
using rin.Framework.Views.Composite;
using rin.Framework.Views.Content;
using rin.Framework.Views.Events;
using rin.Framework.Views.Graphics;
using rin.Framework.Views.Graphics.Quads;
using rin.Framework.Views.Layouts;
using Rect = rin.Framework.Views.Composite.Rect;
using Utils = rin.Framework.Graphics.Utils;

namespace ViewsTest;

[Module(typeof(SViewsModule)),AlwaysLoad]
public class SViewsTestModule : IModule
{

    class WrapContainer : Button
    {
        private readonly View _content;
        public WrapContainer(View content)
        {
            _content = content;
            Child = new Sizer()
            {
                WidthOverride = 300,
                HeightOverride = 300,
                Child = content,
                Padding = 10.0f,
            };
            content.Pivot = 0.5f;
            OnReleased += (@event, button) =>
            {
               _content.StopAll().RotateTo(360,0.5f).After().Do(() => _content.Angle = 0.0f);
            };
        }

        protected override Vec2<float> LayoutContent(Vec2<float> availableSpace)
        {
            var size = base.LayoutContent(availableSpace);
            _content.Translate = _content.Size * .5f;
            return size;
        }
    }
    public void TestWrapping(WindowRenderer renderer)
    {
        if (SViewsModule.Get().GetWindowSurface(renderer) is { } surf)
        {
            var list = new WrapList()
            {
                Axis = Axis.Row,
            };

            surf.Add(new Panel()
            {
                
                Slots =
                [
                    new PanelSlot()
                    {
                        Child = new ScrollList()
                        {
                            Slots = [
                                new ListSlot
                                {
                                    Child = list,
                                    Fit = CrossFit.Fill
                                }
                            ],
                            Clip = Clip.None
                        },
                        MinAnchor = 0.0f,
                        MaxAnchor = 1.0f
                    },
                    new PanelSlot
                    {
                        // Child = new Rect                                    
                        // {
                        //     Child = new FpsWidget
                        //     {
                        //         FontSize = 30
                        //     },
                        //     Padding = new Padding(20.0f),
                        //     BorderRadius = 10.0f,
                        //     BackgroundColor = Color.Black.Clone(a: 0.7f)
                        // },
                        Child = new BackgroundBlur()                                
                        {
                            Child = new FpsWidget
                            {
                                FontSize = 30
                            },
                            Padding = new Padding(20.0f),
                            Strength = 20.0f
                        },
                        SizeToContent = true,
                        MinAnchor = new Vec2<float>(1.0f, 0.0f),
                        MaxAnchor = new Vec2<float>(1.0f, 0.0f),
                        Alignment = new Vec2<float>(1.0f, 0.0f)
                    }
                ]
            });

            surf.Window.OnDrop += (e) =>
            {
                Task.Run(() =>
                {
                    foreach (var objPath in e.Paths)
                    {
                        list.Add(new WrapContainer(new AsyncFileImage(objPath)
                        {
                            BorderRadius = 30.0f
                        }));
                    }
                });
            };

            var rand = new Random();
            surf.Window.OnKey += (e) =>
            {
                if (e is { State: InputState.Pressed, Key: InputKey.Equal })
                {
                    Task.Run(() => Platform.SelectFile("Select Images", filter: "*.png;*.jpg;*.jpeg", multiple: true))
                        .After(
                            (p) =>
                            {
                                foreach (var path in p)
                                    list.Add(new WrapContainer(new AsyncFileImage(path)
                                        {
                                            BorderRadius = 30.0f
                                        }));
                            });
                }

                if (e is { State: InputState.Pressed, Key: InputKey.Minus })
                {
                    list.Add(new WrapContainer(new PrettyView()
                    {
                        //Pivot = 0.5f,
                    }));
                }

                if (e is { State: InputState.Pressed, Key: InputKey.Zero })
                {
                    list.Add(new WrapContainer(new Canvas
                    {
                        Paint = ((canvas, transform, cmds) =>
                        {
                            var rect = Quad.Rect(transform, canvas.GetContentSize());
                            rect.Mode = Quad.RenderMode.ColorWheel;
                            cmds.AddQuads(rect);
                        })
                    }));
                }
            };
        }
    }


    class TestAnimationSizer : Sizer
    {
        private float _width;

        protected override void OnAddedToSurface(Surface surface)
        {
            base.OnAddedToSurface(surface);
            _width = WidthOverride.GetValueOrDefault(0);
        }

        public TestAnimationSizer()
        {
            _width = WidthOverride.GetValueOrDefault(0);
        }

        protected override void OnCursorEnter(CursorMoveEvent e)
        {
            this.StopAll().WidthTo(HeightOverride.GetValueOrDefault(0), easingFunction: EasingFunctions.EaseInOutCubic);
        }

        protected override void OnCursorLeave(CursorMoveEvent e)
        {
            this.StopAll().WidthTo(_width, easingFunction: EasingFunctions.EaseInOutCubic);
        }
    }

    private void TestAnimation(WindowRenderer renderer)
    {
        if (SViewsModule.Get().GetWindowSurface(renderer) is { } surf)
        {
            var list = new List()
            {
                Axis = Axis.Row,
            };

            surf.Add(new Panel()
            {
                Slots =
                [
                    new PanelSlot()
                    {
                        Child = list,
                        MinAnchor = 0.5f,
                        MaxAnchor = 0.5f,
                        Alignment = 0.5f,
                        SizeToContent = true
                    },
                    new PanelSlot
                    {
                        Child = new Rect
                        {
                            Child = new FpsWidget
                            {
                                FontSize = 30,
                                Content = "YOOOO"
                            },
                            Padding = new Padding(20.0f),
                            BorderRadius = 10.0f,
                            BackgroundColor = Color.Black.Clone(a: 0.7f)
                        },
                        SizeToContent = true,
                        MinAnchor = new Vec2<float>(1.0f, 0.0f),
                        MaxAnchor = new Vec2<float>(1.0f, 0.0f),
                        Alignment = new Vec2<float>(1.0f, 0.0f)
                    }
                ]
            });

            surf.Window.OnDrop += (e) =>
            {
                Task.Run(() =>
                {
                    foreach (var objPath in e.Paths)
                    {
                        list.Add(new TestAnimationSizer()
                        {
                            WidthOverride = 200,
                            HeightOverride = 800,
                            Child = new AsyncFileImage(objPath)
                            {
                                BorderRadius = 30.0f
                            },
                            Padding = 10.0f,
                        });
                    }
                });
            };
            var rand = new Random();
            surf.Window.OnKey += (e) =>
            {
                if (e is { State: InputState.Pressed, Key: InputKey.Equal })
                {
                    Task.Run(() => Platform.SelectFile("Select Images", filter: "*.png;*.jpg;*.jpeg", multiple: true))
                        .After(
                            (p) =>
                            {
                                foreach (var path in p)
                                    list.Add(new TestAnimationSizer()
                                    {
                                        WidthOverride = 200,
                                        HeightOverride = 800,
                                        Child = new AsyncFileImage(path)
                                        {
                                            BorderRadius = 30.0f
                                        },
                                        Padding = 10.0f,
                                    });
                            });
                }

                if (e is { State: InputState.Pressed, Key: InputKey.Minus })
                {
                    list.Add(new TestAnimationSizer()
                    {
                        WidthOverride = 200,
                        HeightOverride = 800,
                        Child = new PrettyView()
                        {
                            Pivot = 0.5f,
                        },
                        Padding = 10.0f,
                    });
                }

                if (e is { State: InputState.Pressed, Key: InputKey.Zero })
                {
                    list.Add(new Sizer()
                    {
                        WidthOverride = 200,
                        HeightOverride = 900,
                        Child = new Canvas
                        {
                            Paint = ((canvas, transform, cmds) =>
                            {
                                var rect = Quad.Rect(transform, canvas.GetContentSize());
                                rect.Mode = Quad.RenderMode.ColorWheel;
                                cmds.AddQuads(rect);
                            })
                        },
                        Padding = 10.0f,
                    });
                }
            };
        }
    }

    public void TestSimple(WindowRenderer renderer)
    {
        var surf = SViewsModule.Get().GetWindowSurface(renderer);
        //SWidgetsModule.Get().GetOrCreateFont("Arial").ConfigureAwait(false);
        // surf?.Add(new TextInputBox("I expect this very long text to wrap if the space is too small"));
        surf?.Add(new TextBox("A"));
    }

    public void TestBlur(WindowRenderer renderer)
    {
        var surf = SViewsModule.Get().GetWindowSurface(renderer);

        if (surf == null) return;


        var switcher = new Switcher();

        var infoText = new TextBox("Background Blur Using Blit", 40);

        surf.Add(new Panel
        {
            Slots =
            [
                new PanelSlot
                {
                    Child = switcher,
                    MaxAnchor = 1.0f
                },
                // new PanelSlot
                // {
                //     Child = new Canvas
                //     {
                //         Paint = (self, transform, d) =>
                //         {
                //             d.AddRect(transform, self.GetContentSize(), color: Color.Black.Clone(a: 0.6f));
                //         }
                //     },
                //     MinAnchor = 0.0f,
                //     MaxAnchor = 1.0f
                // },
                new PanelSlot
                {
                    Child = infoText,
                    SizeToContent = true,
                    Alignment = 0f,
                    MinAnchor = 0f,
                    MaxAnchor = 0f
                },
                new PanelSlot
                {
                    Child = new TextInputBox("Example Text", 100),
                    SizeToContent = true,
                    Alignment = 0.5f,
                    MinAnchor = 0.5f,
                    MaxAnchor = 0.5f
                }
            ]
        });

        var frames = 0;
        SRuntime.Get().OnUpdate += d =>
        {
            frames++;
            infoText.Content = $"Frame {frames}"; //$"Focused ${panel.Surface?.FocusedWidget}";
        };

        surf.Window.OnKey += (e) =>
        {
            var switcherSlots = switcher.GetSlots().Count();
            if (switcherSlots > 0)
            {
                if (e is { State: InputState.Pressed or InputState.Repeat, Key: InputKey.Left })
                {
                    var newIndex = switcher.SelectedIndex - 1;
                    if (newIndex < 0)
                    {
                        newIndex = switcherSlots + newIndex;
                    }

                    switcher.SelectedIndex = newIndex;
                    return;
                }

                if (e is { State: InputState.Pressed or InputState.Repeat, Key: InputKey.Right })
                {
                    var newIndex = switcher.SelectedIndex + 1;
                    if (newIndex >= switcherSlots)
                    {
                        newIndex %= switcherSlots;
                    }

                    switcher.SelectedIndex = newIndex;
                    return;
                }
            }

            if (e is { State: InputState.Pressed, Key: InputKey.F })
            {
                switcher.RotateTo(0, 360, 2).After().RotateTo(360, 0, 2);
                return;
            }

            if (e is { State: InputState.Pressed, Key: InputKey.Equal })
            {
                var p = Platform.SelectFile("Select Images", filter: "*.png;*.jpg;*.jpeg", multiple: true);
                foreach (var path in p)
                    switcher.Add(new AsyncFileImage(path));
            }
        };
    }


    public void TestClip(WindowRenderer renderer)
    {
        if (SViewsModule.Get().GetWindowSurface(renderer) is { } surface)
        {
            surface.Add(new Sizer()
            {
                Padding = 20.0f,
                Child = new Rect()
                {
                    Child = new Fitter
                    {
                        Child = new AsyncFileImage(
                            @"C:\Users\Taree\Downloads\Wallpapers-20241117T001814Z-001\Wallpapers\wallpaperflare.com_wallpaper (49).jpg"),
                        FittingMode = FitMode.Cover,
                        Clip = Clip.Bounds
                    },
                    BackgroundColor = Color.Red
                }
            });
        }
    }

    public void TestText(WindowRenderer renderer)
    {
        var surf = SViewsModule.Get().GetWindowSurface(renderer);

        if (surf == null) return;

        var panel = surf.Add(new Panel()
        {
            Slots =
            [
                new PanelSlot
                {
                    Child = new TextBox("Test Text", 40),
                    SizeToContent = true,
                    Alignment = 0f,
                    MinAnchor = 0f,
                    MaxAnchor = 0f
                }
            ]
        });
    }

    private void OnWindowCreated(IWindow window)
    {
        window.OnCloseRequested += (_) =>
        {
            if (window.Parent != null)
            {
                window.Dispose();
            }
            else
            {
                SRuntime.Get().RequestExit();
            }
        };

        window.OnKey += (e =>
        {
            if (e is { Key: InputKey.Up, State: InputState.Pressed })
            {
                window.CreateChild(500, 500, "test child");
            }

            if (e is { Key: InputKey.Enter, State: InputState.Pressed, IsAltDown: true })
            {
                window.SetFullscreen(!window.IsFullscreen);
            }
        });
    }

    public void Startup(SRuntime runtime)
    {
        Utils.RunWindowsOnTick();
        Utils.RunDrawOnThread();
        
        SGraphicsModule.Get().OnRendererCreated += TestWrapping;
        SGraphicsModule.Get().OnWindowCreated += OnWindowCreated;
        SGraphicsModule.Get().CreateWindow(500, 500, "Rin View Test", new CreateOptions()
        {
            Visible = true,
            Decorated = true,
            Transparent = true,
            Focused = false
        });
        //TestText();
    }

    public void Shutdown(SRuntime runtime)
    {

    }
}