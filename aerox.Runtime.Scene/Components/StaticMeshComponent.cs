﻿using System.Runtime.InteropServices;
using aerox.Runtime.Extensions;
using aerox.Runtime.Graphics.Material;
using aerox.Runtime.Math;
using aerox.Runtime.Scene.Graphics;
using aerox.Runtime.Scene.Graphics.Commands;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
namespace aerox.Runtime.Scene.Components;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StaticMeshPushConstants
{
    public Matrix4 Transform;
    public ulong vertexBufferAddress;
}

internal class StaticMeshDrawCommand : Command
{

    protected StaticMesh Mesh;
    protected MaterialInstance?[] Materials;
    protected Matrix4 Transform;
    public StaticMeshDrawCommand(StaticMesh mesh, MaterialInstance?[] materials,Matrix4 transform)
    {
        foreach (var materialInstance in materials)
        {
            materialInstance?.Reserve();
        }

        mesh.Reserve();
        Mesh = mesh;
        Materials = materials;
        Transform = transform;
    }

    protected override void OnDispose(bool isManual)
    {
        base.OnDispose(isManual);
        foreach (var materialInstance in Materials)
        {
            materialInstance?.Dispose();
        }

        Mesh.Dispose();
    }

    public override void Run(SceneFrame frame)
    {
        
        var cmd = frame.Raw.GetCommandBuffer();
        for (var i = 0; i < Mesh.Surfaces.Length; i++)
        {
            var surface = Mesh.Surfaces[i];
            var material = Materials.TryIndex(i) ?? Mesh.Materials.TryIndex(i) ?? frame.Drawer.GetDefaultMeshMaterial();
            
            if (frame.Drawer.GlobalBuffer is { } globalBuffer)
            {
                material.BindBuffer("scene", globalBuffer);
            }
            
            material.BindTo(frame);
            
            material.Push(cmd, new StaticMeshPushConstants()
            {
                Transform = Transform,
                vertexBufferAddress = Mesh.Geometry.VertexBufferAddress
            });
            
            vkCmdBindIndexBuffer(cmd,Mesh.Geometry.IndexBuffer.Buffer,0,VkIndexType.VK_INDEX_TYPE_UINT32);
            
            vkCmdDrawIndexed(cmd,surface.Count,1,surface.StartIndex,0,0);
        }
    }
}

public class StaticMeshComponent : RenderedComponent
{
    private StaticMesh? _mesh;

    public MaterialInstance?[] MaterialOverrides = [];

    public StaticMesh? Mesh
    {
        get => _mesh;
        set
        {
            _mesh = value;
            _mesh?.Reserve();
            MaterialOverrides = _mesh?.Materials.Select(c => (MaterialInstance?)null).ToArray() ?? [];
        }
    }

    public void SetMaterialOverride(MaterialInstance material)
    {
        
    }

    public override void Collect(SceneFrame frame, Matrix4 parentSpace)
    {
        base.Collect(frame, parentSpace);
        if (_mesh is { } mesh)
        {
            frame.AddCommand(new StaticMeshDrawCommand(mesh,[],RelativeTransform * parentSpace));
        }
    }

    protected override void OnDispose(bool isManual)
    {
        base.OnDispose(isManual);
        _mesh?.Dispose();
        foreach (var materialOverride in MaterialOverrides)
        {
            materialOverride?.Dispose();
        }
    }
}