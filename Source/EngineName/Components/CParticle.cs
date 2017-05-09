namespace EngineName.Components {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using Core;

using Microsoft.Xna.Framework;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Represents a single particle..</summary>
public sealed class CParticle: EcsComponent {
    /// <summary>The position in world-space as a displacement from the origin, in meters.</summary>
    public Vector3 Position;

    /// <summary>The velocity, in meters per second.</summary>
    public Vector3 Velocity;

    /// <summary>The net force function.</summary>
    public Func<Vector3> F = () => Vector3.Zero;

    /// <summary>The time-to-live, in seconds.</summary>
    public float Life = 2.0f;
}

}
