﻿using System.Runtime.InteropServices;

namespace rin.Sdf;

/// <summary>
/// Generates a MSDF/MTSDF using <a href="https://github.com/Chlumsky/msdfgen">msdfgen</a>
/// </summary>
public partial class Generator : IDisposable
{
    
    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfNewContext")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr NativeNewContext();

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfFreeContext")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeFreeContext(IntPtr context);

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGlyphContextMoveTo")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeMoveTo(IntPtr context, ref Vector2 to);

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGlyphContextQuadraticBezierTo")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeQuadraticBezierTo(IntPtr context, ref Vector2 control,
        ref Vector2 to);

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGlyphContextCubicBezierTo")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeCubicBezierTo(IntPtr context, ref Vector2 control1,
        ref Vector2 control2,
        ref Vector2 to);

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGlyphContextLineTo")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeLineTo(IntPtr context, ref Vector2 to);

    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfEndContext")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeEnd(IntPtr context);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate void NativeGenerateDelegate(IntPtr data, uint width, uint height, uint byteSize);
    
    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGenerateMSDF")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeGenerateMsdf(IntPtr context,float angleThreshold,float pixelRange,[MarshalAs(UnmanagedType.FunctionPtr)] NativeGenerateDelegate callback);
    
    [LibraryImport(Dlls.AeroxMsdfNative, EntryPoint = "msdfGenerateMTSDF")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void NativeGenerateMtsdf(IntPtr context,float angleThreshold,float pixelRange,[MarshalAs(UnmanagedType.FunctionPtr)] NativeGenerateDelegate callback);

    
    private IntPtr _context = NativeNewContext();
    
    public Generator MoveTo(Vector2 point)
    {
        NativeMoveTo(_context, ref point);
        return this;
    }

    public Generator QuadraticBezierTo(Vector2 control, Vector2 point)
    {
        NativeQuadraticBezierTo(_context, ref control, ref point);
        return this;
    }
    
    
    public Generator CubicBezierTo(Vector2 control1,
        Vector2 control2,
        Vector2 point)
    {
        NativeCubicBezierTo(_context,ref control1,ref control2,ref point);
        return this;
    }
    
    public Generator LineTo(Vector2 point)
    {
        NativeLineTo(_context,ref point);
        return this;
    }
    
    /// <summary>
    /// Stop drawing the vector
    /// </summary>
    /// <returns></returns>
    public Generator End()
    {
        NativeEnd(_context);
        return this;
    }
    
    public void GenerateMsdf(float angleThreshold,float pixelRange,NativeGenerateDelegate callback)
    {
        NativeGenerateMsdf(_context,angleThreshold,pixelRange,callback);
    }

    public void GenerateMtsdf(float angleThreshold,float pixelRange,NativeGenerateDelegate callback)
    {
        NativeGenerateMtsdf(_context,angleThreshold,pixelRange,callback);
    }

    public SDFResult? GenerateMsdf(float angleThreshold,float pixelRange)
    {
        SDFResult? result = null;
        
        unsafe
        {
            GenerateMsdf(angleThreshold,pixelRange, (data, width, height, count) =>
            {
                result = new SDFResult(ByteBuffer.CopyFrom(data,(int)count))
                {
                    Width = (int)width,
                    Height = (int)height,
                    Channels = 3,
                };
            });
        }
        
        return result;
    }

    public SDFResult? GenerateMtsdf(float angleThreshold,float pixelRange)
    {
        SDFResult? result = null;

        unsafe
        {
            GenerateMtsdf(angleThreshold,pixelRange, (data, width, height, count) =>
            {
                result = new SDFResult(ByteBuffer.CopyFrom(data,(int)count))
                {
                    Width = (int)width,
                    Height = (int)height,
                    Channels = 4,
                };
            });
        }

        return result;
    }
    
    
    
    public Task<SDFResult?> GenerateMsdfAsync(float angleThreshold,float pixelRange) =>
        Task.Run(() => GenerateMsdf(angleThreshold, pixelRange));

    public Task<SDFResult?> GenerateMtsdfAsync(float angleThreshold, float pixelRange) =>
        Task.Run(() => GenerateMtsdf(angleThreshold, pixelRange));
    

    private void OnDispose()
    {
        if(_context != IntPtr.Zero) NativeFreeContext(_context);
        _context = IntPtr.Zero;
    }

    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }

    ~Generator()
    {
        OnDispose();
    }
}