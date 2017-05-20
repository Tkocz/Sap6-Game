namespace GameName.Dev {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using Thengill;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Core;
using Thengill.Shaders;
using Thengill.Systems;
using Thengill.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides a simple test case for particle systems. Running it, be able to see particle
/// effects.</summary>
public sealed class Particles: Scene {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Camera entity ID.</summary>
    private int mCamID;

    /// <summary>We store a refernece to the particle system so we can spawn particles
    ///          easily.</summary>
    private ParticleSystem mParticleSys;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        AddSystems(mParticleSys = new  ParticleSystem(),
                   new                  PhysicsSystem(),
                   new                RenderingSystem());

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        mCamID = InitCam();

        // Spawn a few balls.
        for (var i = 0; i < 20; i++) {
            CreateBall(new Vector3(0.9f*i - 3.5f, 0.3f*i, 0.0f), // Position
                       new Vector3(         1.0f, 0.0f  , 0.0f), // Velocity
                       1.0f);                                    // Radius
        }

        var rnd = new Random();
        Func<float> rndSize = () => 0.05f + 0.1f*(float)rnd.NextDouble();
        Func<Vector3> rndVel = () => new Vector3((float)rnd.NextDouble() - 0.5f,
                                                 (float)rnd.NextDouble() - 0.5f,
                                                 (float)rnd.NextDouble() - 0.5f);

        OnEvent("collision", data =>
            mParticleSys.SpawnParticles(3, () => new EcsComponent[] {
                new CParticle     { Position = ((PhysicsSystem.CollisionInfo)data).Position,
                                    Velocity = 12.0f*rndVel(),
                                    Life     = 0.7f,
                                    F        = () => new Vector3(0.0f, -9.81f, 0.0f) },
                new C3DRenderable { model = Game1.Inst.Content.Load<Model>("Models/DummySphere") },
                new CTransform    { Position = ((PhysicsSystem.CollisionInfo)data).Position,
                                    Rotation = Matrix.Identity,
                                    Scale    = rndSize()*Vector3.One } }));
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
    private int CreateBall(Vector3 p, Vector3 v, float r=1.0f) {
        var ball = AddEntity();

        AddComponent(ball, new CBody { Aabb     = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius   = r,
                                       LinDrag  = 0.1f,
                                       Velocity = v });

        AddComponent(ball, new CTransform { Position = p,
                                            Rotation = Matrix.Identity,
                                            Scale    = r*Vector3.One });

        AddComponent<C3DRenderable>(ball, new CImportedModel {
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
