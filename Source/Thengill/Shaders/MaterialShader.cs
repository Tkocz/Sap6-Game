namespace Thengill.Shaders {

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

    /// <summary>The camera position.</summary>
    private Vector3 mCamPos;

    /// <summary>The camera position shader parameter.</summary>
    private EffectParameter mCamPosParam;

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
    /// <summary>
    /// The fog start distance parameter.
    /// </summary>
    private EffectParameter mFogStartParam;
    /// <summary>
    /// The fog end distance parameter.
    /// </summary>
    private EffectParameter mFogEndParam;
    /// <summary>
    /// The fog color parameter.
    /// </summary>
    private EffectParameter mFogColorParam;
    /// <summary>
    /// The fog start distance
    /// </summary>
    private float mFogStart;
    /// <summary>
    /// The fog end distance
    /// </summary>
    private float mFogEnd;
    /// <summary>
    /// The fog color
    /// </summary>
    private Vector3 mFogColor;

        //--------------------------------------
        // PUBLIC PROPERTIES
        //--------------------------------------

        /// <summary>Gets or sets the camera position.</summary>
        public Vector3 CamPos {
        get {
            return mCamPos;
        }

        set {
            mCamPos = value;

            if (mCamPosParam != null) {
                mCamPosParam.SetValue(value);
            }
        }
    }

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
        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor {
            get {
                return mFogColor;
            }
            set {
                mFogColor = value;
                mFogColorParam.SetValue(value);
            }
        }
        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart {
            get {
                return mFogStart;
            }
            set {
                mFogStart = value;
                mFogStartParam.SetValue(value);
            }
        }
        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public float FogEnd {
            get {
                return mFogEnd;
            }
            set {
                mFogEnd = value;
                mFogEndParam.SetValue(value);
            }
        }

        //--------------------------------------
        // PUBLIC CONSTRUCTORS
        //--------------------------------------

        /// <summary>Initializes a new material shader.</summary>
        /// <param name="effect">The underlying shader.</param>
        public MaterialShader(Effect effect) {
        mEffect = effect;

        mCamPosParam = effect.Parameters["CamPos"];
        mModelParam  = effect.Parameters["Model"];
        mProjParam   = effect.Parameters["Proj"];
        mViewParam   = effect.Parameters["View"];
        mFogColorParam = effect.Parameters["FogColor"];
        mFogStartParam = effect.Parameters["FogStart"];
        mFogEndParam = effect.Parameters["FogEnd"];
        }

    /// <summary>Called just before the material is rendered.</summary>
    public virtual void Prerender() {
    }
}

}
