namespace EngineName.Components
{

using Core;

using Microsoft.Xna.Framework;

/// <summary>Provides a physical representation-component for simulating physical effects on
/// entities in the game world.</summary>
public sealed class CBody: EcsComponent {
    /// <summary>The axis-aligned bounding box.</summary>
    public BoundingBox Aabb

    /// <summary>The scalar inverse of the mass, in kilograms.</summary>
    public float InvMass = 1.0f;

    /// <summary>The position in world-space as a displacement from the origin, in
    ///          meters.</summary>
    public Vector3 Position

    /// <summary>The velocity, in meters per second.</summary>
    public Vector3 Velocity
}

}
