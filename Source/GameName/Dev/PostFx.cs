namespace GameName.Dev {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Shaders;
using EngineName.Systems;
using EngineName.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides a simple test case for post fx. Running it, you should see distortion of the
///          screen buffer..</summary>
public sealed class PostFx: Scene {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The distortion pixel shader.</summary>
    private Effect mDistortFX;

    /// <summary>Off-screen render-target used as texture for the pixel shader.</summary>
    private RenderTarget2D mRT;

    /// <summary>Used to create environment maps.</summary>
    private RenderingSystem mRenderer;

    /// <summary>The skybox renderer.</summary>
    private SkyBoxSystem mSkybox;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        AddSystems(new                    LogicSystem(),
                   new                  PhysicsSystem(),
                   new                   CameraSystem(),
                   mSkybox      = new SkyBoxSystem(),
                   mRenderer    = new RenderingSystem());

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        InitCam();

        // Spawn a few balls.
        for (var i = 0; i < 10; i++) {
            CreateBall(new Vector3(0.9f*i - 3.5f, 0.3f*i, 0.0f), // Position
                       new Vector3(         1.0f, 0.0f  , 0.0f), // Velocity
                       1.0f,                                     // Radius
                       false);                                   // Reflective
        }

        mRT = GfxUtil.CreateRT();

        mDistortFX = Game1.Inst.Content.Load<Effect>("Effects/Distort");
        mDistortFX.Parameters["SrcTex"].SetValue(mRT);
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt)  {
        GfxUtil.SetRT(mRT);
        Game1.Inst.GraphicsDevice.Clear(Color.White);
        base.Draw(t, dt);
        GfxUtil.SetRT(null);

        GfxUtil.DrawFsQuad(mDistortFX);
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Creates a new ball in the scene with the given position and velocity.</summary>
    /// <param name="p">The ball position, in world-space.</param>
    /// <param name="v">The initial velocity to give to the ball.</param>
    /// <param name="r">The ball radius.</param>
    private int CreateBall(Vector3 p, Vector3 v, float r=1.0f, bool reflective=false) {
        var ball = AddEntity();

        AddComponent(ball, new CBody { Aabb     = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius   = r,
                                       LinDrag  = 0.1f,
                                       Position = p,
                                       Velocity = v });

        AddComponent(ball, new CTransform { Position = p,
                                            Rotation = Matrix.Identity,
                                            Scale    = r*Vector3.One });

        EnvMapMaterial envMap = null;

        if (reflective) {
            envMap = new EnvMapMaterial(mRenderer,
                                        ball,
                                        (CTransform)GetComponentFromEntity<CTransform>(ball),
                                        EnvMapMaterial.CubeMapFX,
                                        mSkybox);

            AddComponent(ball, new CLogic { Fn    = (t, dt) => envMap.Update(),
                                            InvHz = 1.0f/30.0f });
        }

        AddComponent<C3DRenderable>(ball, new CImportedModel {
            material = reflective ? envMap : null,
            model  = Game1.Inst.Content.Load<Model>("Models/DummySphere")
        });


        return ball;
    }

    /// <summary>Sets up the camera.</summary>
    /// <param name="fovDeg">The camera field of view, in degrees.</param>
    /// <param name="zNear">The Z-near clip plane, in meters from the camera.</param>
    /// <param name="zFar">The Z-far clip plane, in meters from the camera..</param>
    private int InitCam(float fovDeg=60.0f, float zNear=0.01f, float zFar=1000.0f) {
        var aspect = Game1.Inst.GraphicsDevice.Viewport.AspectRatio;
        var cam    = AddEntity();
        var fovRad = fovDeg*2.0f*MathHelper.Pi/360.0f;
        var proj   = Matrix.CreatePerspectiveFieldOfView(fovRad, aspect, zNear, zFar);

        AddComponent(cam, new CCamera { ClipProjection = proj,
                                        Projection     = proj });

        AddComponent(cam, new CTransform { Position = new Vector3(0.0f, 0.0f, 18.0f),
                                           Rotation = Matrix.Identity,
                                           Scale    = Vector3.One });

        return cam;
    }
}

}
