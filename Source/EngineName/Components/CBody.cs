namespace EngineName.Components
{

using Core;

using Microsoft.Xna.Framework;

/// <summary>Provides a physical representation-component for simulating physical effects on
/// entities in the game world.</summary>
public sealed class CBody: EcsComponent {
    /// <summary>The axis-aligned bounding box, used for coarse-phase collision detection.</summary>
    public BoundingBox Aabb;

    /// <summary>The body sphere radius. Currently, the physics system only supports sphere-sphere
    ///          collisions detection and resolution. During fine-phase collision detection, the
    ///          radius is used to solve the collision between two bodies as if they were
    ///          sphere-shaped.</summary>
    public float Radius = 1.0f;

    /// <summary>The scalar inverse of the mass, in kilograms.</summary>
    public float InvMass = 1.0f;

    /// <summary>The linear drag to apply to the body.</summary>
    public float LinDrag = 0.0f;

    /// <summary>The restitution coefficient.</summary>
    public float Restitution = 1.0f;

    /// <summary>The speed multiplier.</summary>

    public float SpeedMultiplier = 1.0f;
         
    /// <summary>The velocity, in meters per second.</summary>
    public Vector3 Velocity;
}

}
