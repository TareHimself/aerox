﻿using System.Runtime.InteropServices;
using rin.Core;
using rin.Core.Math;
using rin.Windows;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.VkStructureType;
using static TerraFX.Interop.Vulkan.Vulkan;
using NativeGraphicsMethods = rin.Graphics.NativeMethods;
namespace rin.Graphics;

/// <summary>
///     Handle's rendering on a <see cref="Windows" />
/// </summary>
public partial class WindowRenderer : Disposable
{
    private const uint FramesInFlight = 2;
    private const VkFormat SwapchainFormat = VkFormat.VK_FORMAT_B8G8R8A8_UNORM;
    private readonly SGraphicsModule _module;
    private readonly VkSurfaceKHR _surface;
    private readonly Window _window;
    private Frame[] _frames = [];
    private ulong _framesRendered;
    private bool _resizing;
    private bool _resizePending = false;
    private VkSwapchainKHR _swapchain;
    private VkImage[] _swapchainImages = [];
    private readonly HashSet<VkPresentModeKHR> _supportedPresentModes;
    public double LastFrameTime = 0.0;

    private VkExtent2D _swapchainSize = new()
    {
        width = 0,
        height = 0
    };

    private VkImageView[] _swapchainViews = [];
    private VkViewport _viewport;
    
    public event Action<Frame>? OnDraw;
    public event Action<Frame, VkImage, VkExtent2D>? OnCopyToSwapchain;

    public event Action<Vector2<uint>>? OnResize;


    public WindowRenderer(SGraphicsModule module, Window window)
    {
        _window = window;
        _surface = CreateSurface();
        _module = module;
        _supportedPresentModes = _module.GetPhysicalDevice().GetSurfacePresentModes(_surface).ToHashSet();
    }

    public WindowRenderer(SGraphicsModule module, Window window, VkSurfaceKHR surface)
    {
        _window = window;
        _surface = surface;
        _module = module;
        _supportedPresentModes = _module.GetPhysicalDevice().GetSurfacePresentModes(_surface).ToHashSet();
    }

    public unsafe VkSurfaceKHR CreateSurface()
    {
        var instance = SRuntime.Get().GetModule<SGraphicsModule>().GetInstance();
        var surface = new VkSurfaceKHR();
        NativeGraphicsMethods.CreateSurface(instance, _window.GetPtr(),&surface);
        return surface;
    }

    public void Init()
    {
        CreateSwapchain();
        InitFrames();
        _window.OnResized += OnWindowResized;
        _window.OnRefresh += OnWindowRefreshed;
        // _window.OnMaximized += OnMaximized;
        // _window.OnFocused += OnFocused;
    }


    protected void OnWindowResized(Window.ResizeEvent e)
    {
        _resizePending = true;
    }
    
    protected void OnWindowRefreshed(Window.RefreshEvent e)
    {
        _resizePending = true;
    }
    
    // protected void OnMaximized(Window.MaximizedEvent e)
    // {
    //     CheckSwapchainSize();
    // }
    //
    // protected void OnFocused(Window.FocusEvent e)
    // {
    //     CheckSwapchainSize();
    // }

    protected override void OnDispose(bool isManual)
    {
        // _window.OnFocused -= OnFocused;
        // _window.OnMaximized -= OnMaximized;
        _window.OnRefresh -= OnWindowRefreshed;
        _window.OnResized -= OnWindowResized;
        foreach (var frame in _frames) frame.Dispose();

        _frames = [];
        DestroySwapchain();
        unsafe
        {
            vkDestroySurfaceKHR(_module.GetInstance(), _surface, null);
        }
    }

    private unsafe void CreateSwapchain()
    {
        _viewport.minDepth = 0.0f;
        _viewport.maxDepth = 1.0f;
        _viewport.width = _window.PixelSize.X;
        _viewport.height = _window.PixelSize.Y;

        if (_viewport.height == 0 || _viewport.width == 0) return;
        var device = _module.GetDevice();
        var physicalDevice = _module.GetPhysicalDevice();
        var format = _module.GetSurfaceFormat();
        var presentMode = 
            _supportedPresentModes.Contains(VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR)
            ? VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR
            : _supportedPresentModes.First();
        NativeGraphicsMethods.CreateSwapchain(
            device,
            physicalDevice,
            _surface,
            (int)format.format,
            (int)format.colorSpace,
            (int)presentMode,
            (uint)_viewport.width,
            (uint)_viewport.height,
            (swapchain, swapchainImages, numSwapchainImages, swapchainImageViews, numSwapchainImageViews) =>
            {
                _swapchain = new VkSwapchainKHR(swapchain);
                var images = (VkImage*)swapchainImages;
                var imageViews = (VkImageView*)swapchainImageViews;

                _swapchainImages = new VkImage[numSwapchainImages];

                for (var i = 0; i < numSwapchainImages; i++) _swapchainImages[i] = images[i];

                _swapchainViews = new VkImageView[numSwapchainImageViews];

                for (var i = 0; i < numSwapchainImages; i++) _swapchainViews[i] = imageViews[i];
            });
        _swapchainSize = new VkExtent2D()
        {
            width= _window.PixelSize.X,
            height = _window.PixelSize.Y
        };
    }

    private void RequestResize()
    {
        _resizePending = true;
    }
    private bool CheckSwapchainSize()
    {
        if (_resizing) return true;

        if (_swapchainSize.width == _window.PixelSize.X &&
            _swapchainSize.height == _window.PixelSize.Y) return false;

        _resizing = true;
        _module.WaitDeviceIdle();
        // foreach (var frame in _frames)
        // {
        //     frame.WaitForLastDraw();
        //     frame.Reset();
        // }
        DestroySwapchain();
        CreateSwapchain();
        OnResize?.Invoke(new Vector2<uint>(_swapchainSize.width,_swapchainSize.height));
        _resizing = false;
        _resizePending = false;
        return true;
    }


    private unsafe void DestroySwapchain()
    {
        var device = _module.GetDevice();
        foreach (var view in _swapchainViews) vkDestroyImageView(device, view, null);
        _swapchainViews = [];
        _swapchainImages = [];


        if (_swapchain.Value != 0) vkDestroySwapchainKHR(device, _swapchain, null);
        _swapchain = new VkSwapchainKHR();
    }

    private void InitFrames()
    {
        var frames = new List<Frame>();

        for (var i = 0; i < FramesInFlight; i++) frames.Add(new Frame(this));

        _frames = frames.ToArray();
    }

    public bool ShouldDraw()
    {
        return !_resizing && _viewport is { height: > 0, width: > 0 };
    }

    public Window GetWindow()
    {
        return _window;
    }

    public Frame GetCurrentFrame()
    {
        return _frames[_framesRendered % FramesInFlight];
    }

    public VkExtent3D GetSwapchainExtent()
    {
        return new VkExtent3D
        {
            width = _swapchainSize.width,
            height = _swapchainSize.height,
            depth = 1
        };
    }

    private void Submit(Frame frame, uint swapchainImageIndex)
    {
        var cmd = frame.GetCommandBuffer();
        var queue = _module.GetGraphicsQueue();
        _module.SubmitToQueue(queue, frame.GetRenderFence(), new VkCommandBufferSubmitInfo[]
                {
                    new()
                    {
                        sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_SUBMIT_INFO,
                        deviceMask = 0,
                        commandBuffer = cmd
                    }
                }, new VkSemaphoreSubmitInfo[]
                {
                    new()
                    {
                        sType = VK_STRUCTURE_TYPE_SEMAPHORE_SUBMIT_INFO,
                        semaphore = frame.GetRenderSemaphore(),
                        value = 1,
                        stageMask = VkPipelineStageFlags2.VK_PIPELINE_STAGE_2_ALL_GRAPHICS_BIT
                    }
                },
                new VkSemaphoreSubmitInfo[]
                {
                    new()
                    {
                        sType = VK_STRUCTURE_TYPE_SEMAPHORE_SUBMIT_INFO,
                        semaphore = frame.GetSwapchainSemaphore(),
                        value = 1,
                        stageMask = VkPipelineStageFlags2.VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT
                    }
                });
            unsafe
            {
                var renderSemaphore = frame.GetRenderSemaphore();
                var swapchain = _swapchain;
                var imIdx = swapchainImageIndex + 0;
                var presentInfo = new VkPresentInfoKHR
                {
                    sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR,
                    pWaitSemaphores = &renderSemaphore,
                    waitSemaphoreCount = 1,
                    pSwapchains = &swapchain,
                    swapchainCount = 1,
                    pImageIndices = &imIdx
                };
                try
                {
                    vkQueuePresentKHR(queue, &presentInfo);
                }
                catch (Exception e)
                {
                   RequestResize();

                    Console.WriteLine(e);
                    throw;
                }
            }
    }

    protected void DrawFrame()
    {
        
        //return;
        if (!ShouldDraw()) return;

        if (_resizePending && CheckSwapchainSize())
        {
            return;
        }

        var frame = GetCurrentFrame();
        var device = _module.GetDevice();

        frame.WaitForLastDraw();

        frame.Reset();

        uint swapchainImageIndex = 0;

        try
        {
            unsafe
            {
                vkAcquireNextImageKHR(device, _swapchain, ulong.MaxValue, frame.GetSwapchainSemaphore(),
                    new VkFence(),
                    &swapchainImageIndex);
            }
        }
        catch (Exception e)
        {
            RequestResize();

            Console.WriteLine(e);
            throw;
        }

        var cmd = frame.GetCommandBuffer();

        vkResetCommandBuffer(cmd, 0);

        var commandBeginInfo = new VkCommandBufferBeginInfo
        {
            sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
            flags = VkCommandBufferUsageFlags.VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT
        };


        var swapchainExtent = _swapchainSize;
        var drawExtent = swapchainExtent;

        unsafe
        {
            vkBeginCommandBuffer(cmd, &commandBeginInfo);
            
            cmd
                .SetRasterizerDiscard(false)
                .DisableMultiSampling();
        }

        if ((OnDraw?.GetInvocationList().Length ?? 0) > 0)
        {
            OnDraw?.Invoke(frame);
        
           cmd.ImageBarrier(_swapchainImages[swapchainImageIndex],
                VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
                VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);

            OnCopyToSwapchain?.Invoke(frame, _swapchainImages[swapchainImageIndex], swapchainExtent);

            cmd.ImageBarrier(_swapchainImages[swapchainImageIndex],
                VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                VkImageLayout.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR);
        }
        else
        {
            cmd.ImageBarrier(_swapchainImages[swapchainImageIndex],
                VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
                VkImageLayout.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR);
        }
       

        vkEndCommandBuffer(cmd);

        Submit(frame, swapchainImageIndex);

        _framesRendered++;
    }
    public void Draw()
    {
        var start = SRuntime.Get().GetTimeSeconds();
        DrawFrame();
        LastFrameTime = SRuntime.Get().GetTimeSeconds() - start;
    }


    
}