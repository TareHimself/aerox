﻿using System.Runtime.InteropServices;
using rin.Core;
using rsl.Generator;
using rsl.Nodes;
using shaderc;
using TerraFX.Interop.Vulkan;

namespace rin.Graphics.Shaders;

public class ShaderManager : Disposable
{
    private readonly Dictionary<string, NativeBuffer<uint>> _spirv = [];
    private readonly Compiler _compiler;
    private readonly BackgroundTaskQueue<CompiledShader> _backgroundTask = new ();
    public event Action? BeforeDispose;
    public ShaderManager()
    {
        var opts = new Options()
        {
            Optimization = OptimizationLevel.Zero,
        };

        _compiler = new Compiler(opts);
    }
    
    public NativeBuffer<uint> CompileAstToSpirv(string id, VkShaderStageFlags stage, ModuleNode node)
    {
        var shader =
            "#version 450\n#extension GL_EXT_buffer_reference : require\n#extension GL_EXT_nonuniform_qualifier : require\n#extension GL_EXT_scalar_block_layout : require\n";

        var transpiler = new GlslGenerator();
        shader += transpiler.Run(node.Statements);
        
        using var shaderCompileResult = _compiler.Compile(shader, "<shader>", stage switch
        {
            VkShaderStageFlags.VK_SHADER_STAGE_VERTEX_BIT => ShaderKind.VertexShader,
            VkShaderStageFlags.VK_SHADER_STAGE_FRAGMENT_BIT => ShaderKind.FragmentShader,
            _ => throw new Exception("Unknown shader stage")
        });
        
        if (shaderCompileResult.Status != Status.Success)
            throw new Exception("Error compiling shader:\n" + shaderCompileResult.ErrorMessage);
        
        unsafe
        {
            
            var buff = new NativeBuffer<uint>((int)shaderCompileResult.CodeLength / Marshal.SizeOf<uint>());
            _spirv.Add(id,buff);
            Buffer.MemoryCopy(shaderCompileResult.CodePointer.ToPointer(),buff.GetPtr().ToPointer(),shaderCompileResult.CodeLength,shaderCompileResult.CodeLength);
            return buff;
        }
    }


    public Task<CompiledShader> StartShaderCompilation(Shader shader)
    {
        return _backgroundTask.Put(() => shader.Compile(this));
    }
    
    protected override void OnDispose(bool isManual)
    {
        BeforeDispose?.Invoke();
        _compiler.Dispose();
        foreach (var (_,buff) in _spirv)
        {
            buff.Dispose();
        }
    }
}