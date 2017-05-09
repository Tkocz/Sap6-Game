namespace EngineName.Shaders {

//--------------------------------------
// USINGS
//--------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Represents an ambient-diffuse-specular material using the Phong reflectance
///          model.</summary>
public class AdsMaterial: MaterialShader {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The ambient color.</summary>
    private Vector3 mAmb;

    /// <summary>The diffuse color.</summary>
    private Vector3 mDif;

    /// <summary>The specular color.</summary>
    private Vector3 mSpe;

    /// <summary>The shininess constant.</summary>
    private float mK;

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new ADS material.</summary>
    /// <param name="amb">The ambient color.</param>
    /// <param name="dif">The diffuse color.</param>
    /// <param name="spe">The specular color.</param>
    /// <param name="k">The shininess constant.</param>
    public AdsMaterial(Vector3 amb, Vector3 dif, Vector3 spe, float k)
        : base(Game1.Inst.Content.Load<Effect>("Effects/AdsMaterial"))
    {
        mAmb = amb;
        mDif = dif;
        mSpe = spe;
        mK   = k;
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Sets up the shader parameters prior to rendering.</summary>
    public override void Prerender() {
        mEffect.Parameters["Amb"].SetValue(mAmb);
        mEffect.Parameters["Dif"].SetValue(mDif);
        mEffect.Parameters["Spe"].SetValue(mSpe);
        mEffect.Parameters["K"  ].SetValue(mK);
    }
}

}
