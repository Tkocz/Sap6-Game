namespace Thengill.Components {

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class CBillboard: EcsComponent {
    public Texture2D Tex;

    public Vector3 Pos;
    public float Scale = 1.0f;

}

}
