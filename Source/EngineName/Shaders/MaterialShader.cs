namespace EngineName.Shaders {

//--------------------------------------
// USINGS
//--------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Represents a material shader used to render objects in the world.</summary>
public class MaterialShader {

    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The effect used to render the material in the GPU.</summary>
    internal readonly Effect mEffect;

    /// <summary>The model matrix.</summary>
    private Matrix mModel;

    /// <summary>The model matrix shader parameter.</summary>
    private EffectParameter mModelParam;

    /// <summary>The projection matrix.</summary>
    private Matrix mProj;

    /// <summary>The projection matrix shader parameter.</summary>
    private EffectParameter mProjParam;

    /// <summary>The view matrix.</summary>
    private Matrix mView;

    /// <summary>The view matrix shader parameter.</summary>
    private EffectParameter mViewParam;

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets or sets the model matrix.</summary>
    public Matrix Model {
        get {
            return mModel;
        }

        set {
            mModel = value;
            mModelParam.SetValue(value);
        }
    }

    /// <summary>Gets or sets the projection matrix.</summary>
    public Matrix Proj {
        get {
            return mProj;
        }

        set {
            mProj = value;
            mProjParam.SetValue(value);
        }
    }

    /// <summary>Gets or sets the view matrix.</summary>
    public Matrix View {
        get {
            return mView;
        }

        set {
            mView = value;
            mViewParam.SetValue(value);
        }
    }

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new material shader.</summary>
    /// <param name="effect">The underlying shader.</param>
    public MaterialShader(Effect effect) {
        mEffect = effect;

        mModelParam = effect.Parameters["Model"];
        mProjParam  = effect.Parameters["Proj"];
        mViewParam  = effect.Parameters["View"];
    }

    /// <summary>Called just before the material is rendered.</summary>
    public virtual void Prerender() {

    }
}

}
