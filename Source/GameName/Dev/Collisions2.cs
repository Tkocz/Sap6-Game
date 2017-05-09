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
using Microsoft.Xna.Framework.Input;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides a simple test case for collisions. Running it, you should see several spheres
///          colliding on the screen in a sane manner.</summary>
public sealed class Collisions2: Scene {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Camera entity ID.</summary>
    private int mCamID;

    /// <summary>Whether slow motion is enabled.</summary>
    private bool mSlowMo;

    /// <summary>Whether the user can toggle slow motion - used to prevent accidental
    ///          spamming.</summary>
    private bool mCanToggleSlowMo;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        PhysicsSystem physics;

        AddSystems(new                    LogicSystem(),
                   physics      =   new PhysicsSystem(),
                   new                   CameraSystem(),
                                  new RenderingSystem());

        physics.Bounds = new BoundingBox(-50.0f*Vector3.One, 50.0f*Vector3.One);

        // Cheap hack to make balls behave like small marbles. Could also just change the scale of
        // the world, but doesn't matter for this scene.
        physics.Gravity *= 10.0f;

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        mCamID = InitCam();

        var rnd = new System.Random();

        // Spawn a few balls.
        for (var i = 0; i < 200; i++) {
            var r = 0.7f + (float)rnd.NextDouble()*0.8f;
            CreateBall(new Vector3(-4.5f + i*0.05f, 20.0f + 2.0f*i, (float)Math.Cos(i)), // Position
                       new Vector3(0.0f           , 0.0f          , 0.0f              ), // Velocity
                       r);                                                               // Radius
        }

        var dist = 25.0f;
        for (var i = 0; i < 10; i++) {
            var a = i - 1.0f;
            var b = a + 0.25f;
            var c = b + 0.25f;
            var d = c + 0.25f;

            var size = new Vector3(25.0f, 0.5f, 25.0f);

            CreateBox(13.0f*Vector3.Right + dist*a*Vector3.Up + 10.0f*Vector3.Forward,
                      size,
                      Vector3.Backward + 0.5f*Vector3.Right, 15.0f);
            CreateBox(13.0f*Vector3.Left + dist*c*Vector3.Up + 10.0f*Vector3.Forward,
                      size,
                      Vector3.Backward - 0.5f*Vector3.Right, -15.0f);

            CreateBox(13.0f*Vector3.Right + dist*b*Vector3.Up + 10.0f*Vector3.Backward,
                      size,
                      Vector3.Backward - 0.5f*Vector3.Right, 15.0f);
            CreateBox(13.0f*Vector3.Left + dist*d*Vector3.Up + 10.0f*Vector3.Backward,
                      size,
                      Vector3.Backward + 0.5f*Vector3.Right, -15.0f);
        }
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this method.</param>
    public override void Update(float t, float dt) {
        if (mSlowMo) {
            t  *= 0.1f;
            dt *= 0.1f;
        }

        base.Update(t, dt);
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt)  {
        Game1.Inst.GraphicsDevice.Clear(Color.White);

        base.Draw(t, dt);

        const float CAM_SPEED = 25.0f;

        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Space)) {
            if (mCanToggleSlowMo) {
                mSlowMo = !mSlowMo;

                mCanToggleSlowMo = false;
            }
        }
        else {
            mCanToggleSlowMo = true;
        }

        if (kb.IsKeyDown(Keys.W)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d.Normalize();
            cam.Target += CAM_SPEED*dt*d;
            camTransf.Position += CAM_SPEED*dt*d;
        }

        if (kb.IsKeyDown(Keys.S)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d.Normalize();
            cam.Target -= CAM_SPEED*dt*d;
            camTransf.Position -= CAM_SPEED*dt*d;
        }

        if (kb.IsKeyDown(Keys.A)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d = new Vector3(d.Z, 0.0f, -d.X);
            d.Normalize();
            cam.Target += CAM_SPEED*dt*d;
            camTransf.Position += CAM_SPEED*dt*d;
        }

        if (kb.IsKeyDown(Keys.D)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d = new Vector3(d.Z, 0.0f, -d.X);
            d.Normalize();
            cam.Target -= CAM_SPEED*dt*d;
            camTransf.Position -= CAM_SPEED*dt*d;
        }

        if (kb.IsKeyDown(Keys.Q)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            cam.Target += CAM_SPEED*dt*Vector3.Up;
            camTransf.Position += CAM_SPEED*dt*Vector3.Up;
        }

        if (kb.IsKeyDown(Keys.Z)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            cam.Target -= CAM_SPEED*dt*Vector3.Up;
            camTransf.Position -= CAM_SPEED*dt*Vector3.Up;
        }

        if (kb.IsKeyDown(Keys.Up)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));

            cam.Target += CAM_SPEED*dt*Vector3.Up;
        }

        if (kb.IsKeyDown(Keys.Down)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));

            cam.Target -= CAM_SPEED*dt*Vector3.Up;
        }

        if (kb.IsKeyDown(Keys.Left)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d = new Vector3(d.Z, 0.0f, -d.X);
            d.Normalize();
            cam.Target += CAM_SPEED*dt*d;
        }

        if (kb.IsKeyDown(Keys.Right)) {
            var cam = ((CCamera)GetComponentFromEntity<CCamera>(mCamID));
            var camTransf = ((CTransform)GetComponentFromEntity<CTransform>(mCamID));

            var d = (cam.Target - camTransf.Position);
            d = new Vector3(d.Z, 0.0f, -d.X);
            d.Normalize();
            cam.Target -= CAM_SPEED*dt*d;
        }
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

        AddComponent(ball, new CBody { Aabb        = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius      = r,
                                       LinDrag     = 0.2f,
                                       Velocity    = v,
                                       Restitution = 0.3f});

        AddComponent(ball, new CTransform { Position = p,
                                            Rotation = Matrix.Identity,
                                            Scale    = r*Vector3.One });

        AddComponent<C3DRenderable>(ball, new CImportedModel {
            model  = Game1.Inst.Content.Load<Model>("Models/DummySphere")
        });

        return ball;
    }

    /// <summary>Creates a static, oriented bounding box.</summary>
    /// <param name="pos">The position of the box.</param>
    /// <param name="dim">The box dimensions, or size.</param>
    /// <param name="rotAxis">The axis of rotation (concerning the box's transformation).</param>
    /// <param name="rotDeg">The rotation, in degrees (concerning the box's transformation).</param>
    private void CreateBox(Vector3 pos, Vector3 dim, Vector3 rotAxis, float rotDeg) {
        var rotRad = MathHelper.ToRadians(rotDeg);

        var box1 = AddEntity();

        AddComponent<C3DRenderable>(box1, new CImportedModel {
            model  = Game1.Inst.Content.Load<Model>("Models/DummyBox")
        });

        var rot = Matrix.CreateFromAxisAngle(rotAxis, rotRad);
        var invRot = Matrix.Invert(rot);

        AddComponent(box1, new CTransform { Position = pos,
                                            Rotation = rot,
                                            Scale    = dim });

        AddComponent(box1, new CBox { Box = new BoundingBox(-dim, dim), InvTransf = invRot });
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
                                        Projection     = proj,
                                        Target = new Vector3(0.0f, 20.0f, 0.0f) });

        AddComponent(cam, new CTransform { Position = new Vector3(0.0f, 20.0f, 38.0f),
                                           Rotation = Matrix.Identity,
                                           Scale    = Vector3.One });

        return cam;
    }
}

}
