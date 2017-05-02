namespace EngineName.Shaders {

//--------------------------------------
// USINGS
//--------------------------------------

using Components;
using Systems;
using Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides an environment map material.</summary>
public class EnvMapMaterial: MaterialShader {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The entity id.</summary>
    private readonly int mEid;

    /// <summary>The transform component of the entity to environment map.</summary>
    private readonly CTransform mTransf;

    /// <summary>The off-screen environment map render targets.</summary>
    private readonly RenderTarget2D[] mEnvRTs = new RenderTarget2D[6];

    /// <summary>The renderer to use when rendering the environment map.</summary>
    private readonly RenderingSystem mRenderer;

    /// <summary>Skybox renderer.</summary>
    private readonly SkyBoxSystem mSkybox;

    /// <summary>The default cube map material shader.,</summary>
    private static Effect sCubeMapFX;

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets the default cube map material shader.</summary>
    public static Effect CubeMapFX {
        get {
            if (sCubeMapFX == null) {
                sCubeMapFX = Game1.Inst.Content.Load<Effect>("Effects/CubeMap");
            }

            return sCubeMapFX;
        }
    }

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new environment map.</summary>
    /// <param name="renderer">The renderer to use when rendering the environment maps.</param>
    /// <param name="eid">The id of the entity to which the environment map belongs.</param>
    /// <param name="transf">The transform component of the entity to environment map.</param>
    /// <param name="material">The effect to use as material. Normally this should be set to
    ///                        <see cref="EnvMapMaterial.CubeMapFX"/></param>
    /// <param name="skybox">The skybox renderer.</param>
    public EnvMapMaterial(RenderingSystem renderer,
                          int             eid,
                          CTransform      transf,
                          Effect          material,
                          SkyBoxSystem    skybox=null)
        : base(material)
    {
        mEid      = eid;
        mRenderer = renderer;
        mTransf   = transf;
        mSkybox   = skybox;

        var device = Game1.Inst.GraphicsDevice;

        // Set up the shader params.
        for (var i = 0; i < 6; i++) {
            mEnvRTs[i] = new RenderTarget2D(device,
                                            512,
                                            512,
                                            false,
                                            device.PresentationParameters.BackBufferFormat,
                                            DepthFormat.None,
                                            1,
                                            RenderTargetUsage.PreserveContents); // TODO: Needed?

            material.Parameters["EnvTex" + i.ToString()].SetValue(mEnvRTs[i]);
        }
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Updates the cube map by rendering each environment mapped cube face. NOTE: This is
    ///          really slow so don't do this too often.</summary>
    public void Update() {
        // Basically, what we do here is position cameras in each direction of the cube and render
        // to hidden targets, then use them as texture sources in the shader (see CubeMap.fx).

        var p = mTransf.Position;

        // TODO: Cache cameras, don't need to realloc them here.
        var cams = new CCamera[6];
        cams[0] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Right   , Vector3.Down    ) };
        cams[1] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Left    , Vector3.Down    ) };
        cams[2] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Up      , Vector3.Backward) };
        cams[3] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Down    , Vector3.Forward ) };
        cams[4] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Backward, Vector3.Down    ) };
        cams[5] = new CCamera { View = Matrix.CreateLookAt(p, p + Vector3.Forward , Vector3.Down    ) };

        var aspect = 1.0f;
        var fovRad = 90.0f*2.0f*MathHelper.Pi/360.0f;
        var zFar   = 1000.0f;
        var zNear  = 0.01f;

        for (var i = 0; i < 6; i++) {
            cams[i].Projection = Matrix.CreatePerspectiveFieldOfView(fovRad, aspect, zNear, zFar);
            GfxUtil.SetRT(mEnvRTs[i]);
            Game1.Inst.GraphicsDevice.Clear(Color.Black);

            if (mSkybox != null) {
                mSkybox.DrawSkybox(cams[i]);
            }

            mRenderer.DrawScene(cams[i], mEid);
        }

        GfxUtil.SetRT(null);
    }
}

}
