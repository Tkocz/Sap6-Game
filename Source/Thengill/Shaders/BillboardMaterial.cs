namespace Thengill.Shaders {

//--------------------------------------
// USINGS
//--------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Represents a billboard material.</summary>
public class BillboardMaterial: MaterialShader {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    // /// <summary>Gets or sets the billboard position.</summary>
    //public Vector3 Pos { get; set; }

    /// <summary>Gets or sets the billboard texture.</summary>
    public Texture2D Tex { get; set; }

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new billboard material.</summary>
    public BillboardMaterial()
        : base(Game1.Inst.Content.Load<Effect>("Effects/BillboardMaterial"))
    {
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Sets up the shader parameters prior to rendering.</summary>
    public override void Prerender() {
        //mEffect.Parameters["BillboardPos"].SetValue(Pos);
        mEffect.Parameters["BillboardTex"].SetValue(Tex);
    }
}

}
