namespace GameName.Dev {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;

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

/// <summary>Provides a simple test case for material shaders. Running it, you should see several
///          spheres colliding on the screen in a sane manner with interesting materials..</summary>
public sealed class Materials: Scene {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Camera entity ID.</summary>
    private int mCamID;

    /// <summary>Used to create environment maps.</summary>
    private RenderingSystem mRenderer;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        AddSystems(new                    LogicSystem(),
                   new                  PhysicsSystem() { Gravity = Vector3.Zero },
                   mRenderer    = new RenderingSystem());

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        mCamID = InitCam();

        // Spawn a few balls.
        for (var i = 0; i < 2; i++) {
            var r = i == 0 ? 6.0f : 1.0f;
            CreateBall(new Vector3(0.9f*i - 3.5f, 0.3f*i, 0.7f*i), // Position
                       new Vector3(         3.0f*(i-4), 2.0f*i  , 3.0f-i),   // Velocity
                       r,                                          // Radius
                       i == 0);                                    // Reflective
        }
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt)  {
        { // TODO: CameraSystem no longer supports anything but chase cam, so manual setup below.
            var cam = (CCamera)Game1.Inst.Scene.GetComponentFromEntity<CCamera>(mCamID);
            var camTransf = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(mCamID);
            cam.View = Matrix.CreateLookAt(camTransf.Position, cam.Target, Vector3.Up);
        }

        Game1.Inst.GraphicsDevice.Clear(Color.White);
        base.Draw(t, dt);
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Creates a new ball in the scene with the given position and velocity.</summary>
    /// <param name="p">The ball position, in world-space.</param>
    /// <param name="v">The initial velocity to give to the ball.</param>
    /// <param name="r">The ball radius.</param>
    /// <param name="reflective">Whether to use an environment mapped material.</param>
    private int CreateBall(Vector3 p, Vector3 v, float r=1.0f, bool reflective=false) {
        var ball = AddEntity();

        AddComponent(ball, new CBody { Aabb     = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius   = r,
                                       LinDrag  = 0.1f,
                                       Velocity = v });

        CTransform transf;
        AddComponent(ball, transf = new CTransform { Position = p,
                                                     Rotation = Matrix.Identity,
                                                     Scale    = r*Vector3.One });

        EnvMapMaterial envMap = null;

        if (reflective) {
            var rot = 0.0f;
            envMap = new EnvMapMaterial(mRenderer,
                                        ball,
                                        (CTransform)GetComponentFromEntity<CTransform>(ball),
                                        Game1.Inst.Content.Load<Texture2D>("Textures/Bumpmap0"));

            // TODO: If the camera moves, this needs to be done every frame.
            //envMap.SetCameraPos(new Vector3(9.0f, 12.0f, 18.0f));
            AddComponent(ball, new CLogic { Fn    = (t, dt) => {
                                                rot += 1.0f*dt;
                                                transf.Rotation = Matrix.CreateRotationX(rot)
                                                                * Matrix.CreateRotationY(0.7f*rot);

                                                envMap.Update();

                                            },
                                            InvHz = 1.0f/30.0f });
        }

        AddComponent<C3DRenderable>(ball, new CImportedModel {
                materials = new Dictionary<int, MaterialShader> {
                    {0, reflective ? envMap : null } },
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

        AddComponent(cam, new CTransform { Position = new Vector3(9.0f, 12.0f, 18.0f),
                                           Rotation = Matrix.Identity,
                                           Scale    = Vector3.One });

        return cam;
    }
}

}
