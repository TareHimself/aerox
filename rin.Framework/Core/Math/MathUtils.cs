﻿using MathNet.Numerics.LinearAlgebra;

namespace rin.Framework.Core.Math;

public static class MathUtils
{
    public static Matrix<float> Translate(this Matrix<float> mat, Vec3<float> translation)
    {
        mat.SetColumn(
            0,
            mat.Column(0).Multiply(translation.X) +
            mat.Column(1).Multiply(translation.Y) +
            mat.Column(2).Multiply(translation.Z) +
            mat.Column(3)
        );
        return mat;
    }


    public static Matrix<float> Scale(this Matrix<float> mat, Vec3<float> scale)
    {
        mat.SetColumn(0, mat.Column(0).Multiply(scale.X));
        mat.SetColumn(1, mat.Column(1).Multiply(scale.Y));
        mat.SetColumn(2, mat.Column(2).Multiply(scale.Z));
        return mat;
    }

    public static Matrix<float> Rotate(this Matrix<float> mat, Quat rotation)
    {
        // mat.SetColumn(
        //     0,
        //     mat.Column(0).Multiply(translation.X) +
        //     mat.Column(1).Multiply(translation.Y) +
        //     mat.Column(2).Multiply(translation.Z) +
        //     mat.Column(3)
        // );
        return mat;
    }

    public static float InterpolateTo(float current, float target, float deltaTime, float speed)
    {
        var dist = target - current;
        var delta = speed * deltaTime;
        if (System.Math.Abs(delta) > System.Math.Abs(dist)) return target;
        if (dist < 0) delta *= -1.0f;

        return current + delta;
    }
    
    public static Vec2<float> InterpolateTo(Vec2<float> begin, Vec2<float> end, float deltaTime, float speed)
    {
        return new Vec2<float>(InterpolateTo(begin.X, end.X, deltaTime, speed),
            InterpolateTo(begin.Y, end.Y, deltaTime, speed));
    }

    /// <summary>
    ///     Interpolates from <see cref="begin" /> to <see cref="end" /> using <see cref="alpha" />. The default
    ///     <see cref="method" /> is <a href="https://en.wikipedia.org/wiki/Linear_interpolation">Linear</a>
    /// </summary>
    /// <param name="begin">The start</param>
    /// <param name="end">The end</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="method">An optional method for interpolation</param>
    /// <returns></returns>
    public static float Interpolate(float begin, float end, float alpha,
        Func<float, float, float, float>? method = null)
    {
        if (method == null) return System.Math.Clamp(begin + (end - begin) * alpha, begin, end);

        return System.Math.Clamp(method(begin, end, alpha), begin, end);
    }

    /// <summary>
    ///     Interpolates from <see cref="begin" /> to <see cref="end" /> using <see cref="alpha" />. The default
    ///     <see cref="method" /> is <a href="https://en.wikipedia.org/wiki/Linear_interpolation">Linear</a>
    /// </summary>
    /// <param name="begin">The start</param>
    /// <param name="end">The end</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="method">An optional method for interpolation</param>
    /// <returns></returns>
    public static Vec2<float> Interpolate(Vec2<float> begin, Vec2<float> end, float alpha,
        Func<float, float, float, float>? method = null)
    {
        return new Vec2<float>(Interpolate(begin.X, end.X, alpha, method),
            Interpolate(begin.Y, end.Y, alpha, method));
    }

    /// <summary>
    ///     Interpolates from <see cref="begin" /> to <see cref="end" /> using <see cref="alpha" />. The default
    ///     <see cref="method" /> is <a href="https://en.wikipedia.org/wiki/Linear_interpolation">Linear</a>
    /// </summary>
    /// <param name="begin">The start</param>
    /// <param name="end">The end</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="method">An optional method for interpolation</param>
    /// <returns></returns>
    public static Vec3<float> Interpolate(Vec3<float> begin, Vec3<float> end, float alpha,
        Func<float, float, float, float>? method = null)
    {
        return new Vec3<float>(Interpolate(begin.X, end.X, alpha, method),
            Interpolate(begin.Y, end.Y, alpha, method), Interpolate(begin.Z, end.Z, alpha, method));
    }

    /// <summary>
    ///     Interpolates from <see cref="begin" /> to <see cref="end" /> using <see cref="alpha" />. The default
    ///     <see cref="method" /> is <a href="https://en.wikipedia.org/wiki/Linear_interpolation">Linear</a>
    /// </summary>
    /// <param name="begin">The start</param>
    /// <param name="end">The end</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="method">An optional method for interpolation</param>
    /// <returns></returns>
    public static Vec4<float> Interpolate(Vec4<float> begin, Vec4<float> end, float alpha,
        Func<float, float, float, float>? method = null)
    {
        return new Vec4<float>(Interpolate(begin.X, end.X, alpha, method),
            Interpolate(begin.Y, end.Y, alpha, method), Interpolate(begin.Z, end.Z, alpha, method),
            Interpolate(begin.W, end.W, alpha, method));
    }

    /// <summary>
    ///     Interpolates from <see cref="begin" /> to <see cref="end" /> using <see cref="alpha" />. The default
    ///     <see cref="method" /> is <a href="https://en.wikipedia.org/wiki/Linear_interpolation">Linear</a>
    /// </summary>
    /// <param name="begin">The start</param>
    /// <param name="end">The end</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="method">An optional method for interpolation</param>
    /// <returns></returns>
    public static Quat Interpolate(Quat begin, Quat end, float alpha,
        Func<float, float, float, float>? method = null)
    {
        return new Quat(Interpolate(begin.X, end.X, alpha, method), Interpolate(begin.Y, end.Y, alpha, method),
            Interpolate(begin.Z, end.Z, alpha, method), Interpolate(begin.W, end.W, alpha, method));
    }


    public static double RadToDeg(double radians) => (radians * (180.0f / System.Math.PI));
}