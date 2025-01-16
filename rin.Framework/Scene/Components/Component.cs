﻿using rin.Framework.Scene.Actors;

namespace rin.Framework.Scene.Components;

public class Component : IComponent
{
    public Actor? Owner { get; set; }
    public bool Active { get; protected set; }
    public virtual void Start()
    {
        Active = true;
    }

    public virtual void Stop()
    {
        Active = false;
    }

    public virtual void Update(double delta)
    {
        
    }
}