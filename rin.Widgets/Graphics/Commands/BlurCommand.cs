﻿using System.Runtime.InteropServices;
using rin.Core;
using rin.Core.Math;
using rin.Graphics;
using rin.Graphics.Descriptors;
using rin.Graphics.Shaders;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;

namespace rin.Widgets.Graphics.Commands;

[StructLayout(LayoutKind.Sequential)]
public struct BlurPushConstants
{
    public required Matrix4 Projection;
    
    public required Matrix3 Transform;

    public required Vector2<float> Size;

    public required float BlurRadius;

    public required Vector4<float> Tint;
}

public class BlurCommand(Matrix3 transform, Vector2<float> size,float radius, Vector4<float> tint) : CustomCommand
{
    private static string _blurPassId = Guid.NewGuid().ToString();
    private readonly GraphicsShader _blurShader =
        GraphicsShader.FromFile(Path.Join(SRuntime.ResourcesDirectory, "shaders", "widgets", "blur.rsl"));

    public override bool WillDraw => true;


    // public static void ApplyBlurPass(WidgetFrame frame)
    // {
    //     var cmd = frame.Raw.GetCommandBuffer();
    //
    //     var drawImage = frame.Surface.GetDrawImage();
    //     var stencilImage = frame.Surface.GetStencilImage();
    //
    //     var size = frame.Surface.GetDrawSize();
    //
    //     var drawExtent = new VkExtent3D
    //     {
    //         width = (uint)size.X,
    //         height = (uint)size.Y
    //     };
    //
    //     cmd.BeginRendering(new VkExtent2D
    //     {
    //         width = drawExtent.width,
    //         height = drawExtent.height
    //     }, [
    //         SGraphicsModule.MakeRenderingAttachment(drawImage.View,
    //             VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
    //     ], stencilAttachment: SGraphicsModule.MakeRenderingAttachment(stencilImage.View,
    //         VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL));
    //
    //     frame.Raw.ConfigureForWidgets(size.Cast<uint>());
    // }

    public override void Run(WidgetFrame frame, uint stencilMask,DeviceBuffer.View? view = null)
    {
        frame.BeginMainPass();
        var cmd = frame.Raw.GetCommandBuffer();
        if (_blurShader.Bind(cmd,true))
        {
            var copyImage = frame.Surface.GetCopyImage();
            var descriptorSet = frame.Raw.GetDescriptorAllocator()
                .Allocate(_blurShader.GetDescriptorSetLayouts().First().Value);
            descriptorSet.WriteImages(_blurShader.Resources["SourceT"].Binding, new ImageWrite(copyImage,
                VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, ImageType.Texture, new SamplerSpec()
                {
                    Filter = ImageFilter.Linear,
                    Tiling = ImageTiling.ClampBorder
                }));

            cmd.BindDescriptorSets(VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS, _blurShader.GetPipelineLayout(),
                new []{descriptorSet});

            var pushResource = _blurShader.PushConstants.First().Value;
            var push = new BlurPushConstants()
            {
                Projection = frame.Projection,
                Size = size,
                BlurRadius = radius,
                Tint = tint,
                Transform = transform
            };
            cmd.PushConstant(_blurShader.GetPipelineLayout(), pushResource.Stages, push);
            vkCmdDraw(cmd,6,1,0,0);
        }
    }
}