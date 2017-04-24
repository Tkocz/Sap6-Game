namespace EngineName.Components
{

using Core;

using Microsoft.Xna.Framework;

/// <summary>Provides a physical representation-component for simulating physical effects on
/// entities in the game world.</summary>
public sealed class CBody: EcsComponent {
    /// <summary>Gets or sets the axis-aligned bounding box.</summary>
    public BoundingBox Aabb { get; set; }

    /// <summary>Gets or sets the scalar inverse of the mass, in kilograms.</summary>
    public float InvMass { get; set; }

    /// <summary>Gets or sets the position in world-space as a displacement from the origin, in
    ///          meters.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the velocity, in meters per second.</summary>
    public Vector3 Velocity { get; set; }
}

}
