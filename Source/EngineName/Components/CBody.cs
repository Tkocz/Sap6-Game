namespace EngineName.Components
{

using Microsoft.Xna.Framework;

public sealed class CTransform: EcsComponent {
    /// <summary>Gets or sets the scalar inverse of the mass, in kilograms.</summary>
    public float InvMass { get; set; }

    /// <summary>Gets or sets the position in world-space as a displacement from the origin, in
    ///          meters.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the velocity, in meters per second.</summary>
    public Vector3 Velocity { get; set; }
}

}
