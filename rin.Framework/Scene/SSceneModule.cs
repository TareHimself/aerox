﻿using rin.Framework.Core;
using rin.Framework.Views;

namespace rin.Framework.Scene;

[Module(typeof(SViewsModule))]
public class SSceneModule : IModule, ISingletonGetter<SSceneModule>
{
    public void Startup(SRuntime runtime)
    {
        //throw new NotImplementedException();
    }

    public void Shutdown(SRuntime runtime)
    {
        //throw new NotImplementedException();
    }

    public Scene CreateScene()
    {
        return new Scene();
    }

    public static SSceneModule Get()
    {
        return SRuntime.Get().GetModule<SSceneModule>();
    }
}