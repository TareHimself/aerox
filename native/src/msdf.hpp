﻿#pragma once
#include <macro.hpp>
#include <msdfgen/msdfgen.h>

struct GlyphContext
{
    msdfgen::Point2 position{};
    msdfgen::Shape * shape;
    msdfgen::Contour * contour;
    GlyphContext();
    ~GlyphContext();
};

struct Vec2
{
    float x;
    float y;

    operator msdfgen::Vector2()
    {
        return msdfgen::Vector2(x,y);
    }
};


EXPORT GlyphContext * msdfNewContext();

EXPORT void msdfFreeContext(GlyphContext * context);

EXPORT void msdfGlyphContextMoveTo(GlyphContext * context, Vec2* to);

EXPORT void msdfGlyphContextLineTo(GlyphContext * context, Vec2* to);

EXPORT void msdfGlyphContextQuadraticBezierTo(GlyphContext * context, Vec2* control,Vec2* to);

EXPORT void msdfGlyphContextCubicBezierTo(GlyphContext * context, Vec2* control1,Vec2* control2,Vec2* to);

EXPORT void msdfEndContext(GlyphContext * context);

using RenderMsdfCallback = void(__stdcall *)(void * data,uint32_t width,uint32_t height,uint32_t size);
EXPORT void msdfRenderContext(GlyphContext * context,RenderMsdfCallback callback);


