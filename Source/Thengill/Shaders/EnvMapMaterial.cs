namespace Thengill.Shaders {

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

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new environment map.</summary>
    /// <param name="renderer">The renderer to use when rendering the environment maps.</param>
    /// <param name="eid">The id of the entity to which the environment map belongs.</param>
    /// <param name="transf">The transform component of the entity to environment map.</param>
    /// <param name="material">The effect to use as material. Normally this should be set to a cube
    ///                        map shader.</param>
    public EnvMapMaterial(RenderingSystem renderer,
                          int             eid,
                          CTransform      transf,
                          Effect          material,
                          Texture2D       bumpTex=null)
        : base(material)
    {
        mEid      = eid;
        mRenderer = renderer;
        mTransf   = transf;

        var device = Game1.Inst.GraphicsDevice;

        for (var i = 0; i < 6; i++) {
            mEnvRTs[i] = new RenderTarget2D(device,
                                            512,
                                            512,
                                            false,
                                            device.PresentationParameters.BackBufferFormat,
                                            DepthFormat.None,
                                            1,
                                            RenderTargetUsage.PreserveContents); // TODO: Needed?
        }

        ;
        mEffect.Parameters["BumpMap"].SetValue(bumpTex);
    }

    /// <summary>Initializes a new environment map.</summary>
    /// <param name="renderer">The renderer to use when rendering the environment maps.</param>
    /// <param name="eid">The id of the entity to which the environment map belongs.</param>
    /// <param name="transf">The transform component of the entity to environment map.</param>
    public EnvMapMaterial(RenderingSystem renderer,
                          int             eid,
                          CTransform      transf,
                          Texture2D       bumpTex=null)
        : this(renderer, eid, transf, Game1.Inst.Content.Load<Effect>("Effects/CubeMap"), bumpTex)
    {
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Called just before the material is rendered.</summary>
    public override void Prerender() {
        // Set up the shader params.
        for (var i = 0; i < 6; i++) {
            mEffect.Parameters["EnvTex" + i.ToString()].SetValue(mEnvRTs[i]);
        }
    }

    /// <summary>Updates the cube map by rendering each environment mapped cube face. NOTE: This is
    ///          really slow so don't do this too often.</summary>
    public void Update() {
        // Basically, what we do here is position cameras in each direction of the cube and render
        // to hidden targets, then use them as texture sources in the shader (see CubeMap.fx).

        var p   = mTransf.Position;
        var rot = Matrix.Identity;

        var r = Vector3.Transform(Vector3.Right   , rot);
        var l = Vector3.Transform(Vector3.Left    , rot);
        var u = Vector3.Transform(Vector3.Up      , rot);
        var d = Vector3.Transform(Vector3.Down    , rot);
        var b = Vector3.Transform(Vector3.Backward, rot);
        var f = Vector3.Transform(Vector3.Forward , rot);

        // TODO: Cache cameras, don't need to realloc them here.
        var cams = new CCamera[6];
        cams[0] = new CCamera { View = Matrix.CreateLookAt(p, p + r, d    ) };
        cams[1] = new CCamera { View = Matrix.CreateLookAt(p, p + l, d    ) };
        cams[2] = new CCamera { View = Matrix.CreateLookAt(p, p + u, b) };
        cams[3] = new CCamera { View = Matrix.CreateLookAt(p, p + d, f ) };
        cams[4] = new CCamera { View = Matrix.CreateLookAt(p, p + b, d    ) };
        cams[5] = new CCamera { View = Matrix.CreateLookAt(p, p + f, d    ) };

        var aspect = 1.0f;
        var fovRad = 90.0f*2.0f*MathHelper.Pi/360.0f;
        var zFar   = 100.0f;
        var zNear  = 0.01f;

        for (var i = 0; i < 6; i++) {
            cams[i].Projection = Matrix.CreatePerspectiveFieldOfView(fovRad, aspect, zNear, zFar);
            GfxUtil.SetRT(mEnvRTs[i]);

            Game1.Inst.GraphicsDevice.Clear(Color.Magenta);
            mRenderer.DrawScene(cams[i], mEid);
        }

        GfxUtil.SetRT(null);
    }
}

}
