namespace Thengill.Components {

//--------------------------------------
// USINGS
//--------------------------------------

using Core;

using Microsoft.Xna.Framework;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Represents a static, physical oriented bounding box. NOTE: The orientation is stored in
///          <see cref="CTransform"/>.</summary>
public sealed class CBox: EcsComponent {
    /// <summary>The box.</summary>
    public BoundingBox Box;

    /// <summary>The transform inverse matrix.</summary>
    public Matrix InvTransf;
}

}
