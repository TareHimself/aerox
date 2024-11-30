using TerraFX.Interop.Vulkan;

namespace rin.Graphics.FrameGraph;

public static class Extensions
{
    public static IGraphBuilder AddPass(this IGraphBuilder builder,Action<IPass,IGraphBuilder> configure, Action<IPass,ICompiledGraph,Frame, VkCommandBuffer> run,bool terminal = false,string? name = null)
    {
        return builder.AddPass(new ActionPass(configure, run,terminal,name));
    }

    public static IDeviceImage AsImage(this IGraphResource resource) => resource is IDeviceImage asImage
        ? asImage
        : throw new Exception("Resource is not image");
    
    public static IDeviceBuffer AsMemory(this IGraphResource resource) => resource is IDeviceBuffer asMemory
        ? asMemory
        : throw new Exception("Resource is not memory");
}