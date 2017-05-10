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
public sealed class Collisions: Scene {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Camera entity ID.</summary>
    private int mCamID;

    /// <summary>Whether slow motion is enabled.</summary>
    private bool mSlowMo = true;

    /// <summary>Whether the user can toggle slow motion - used to prevent accidental
    ///          spamming.</summary>
    private bool mCanToggleSlowMo;

    /// <summary>Total draw time.</summary>
    private float mDrawT;

    /// <summary>Total update time.</summary>
    private float mUpdT;

    /// <summary>Slow motion factor.</summary>
    private float mSlowMoFactor = 1.0f;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        PhysicsSystem physics;

        AddSystems(physics = new PhysicsSystem(),
                              new CameraSystem(),
                           new RenderingSystem());

        physics.Bounds = new BoundingBox(-5.0f*Vector3.One, 5.0f*Vector3.One);

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        mCamID = InitCam();

        var rnd = new System.Random();

        // Colors to pick from when spawning balls.
        var cols = new [] {
            new Vector3(1.0f, 0.8f, 0.0f),
            new Vector3(0.2f, 0.8f, 0.1f),
        };

        // Spawn a few balls.
        for (var i = 0; i < 300; i++) {
            var r = 0.06f + (float)rnd.NextDouble()*0.1f;
            CreateBall(new Vector3(-0.4f + i*0.005f, 2.0f + 0.2f*i, 0.1f*(float)Math.Cos(i)),
                       new Vector3(0.0f           , 0.0f          , 0.0f                   ),
                       r,
                       cols[rnd.Next(cols.Length)]);
        }

        var dist = 2.2f;
        for (var i = 0; i < 3; i++) {
            var a = i - 1.0f;
            var b = a + 0.25f;
            var c = b + 0.25f;
            var d = c + 0.25f;

            var sf = 0.9f + 0.2f*i;
            var sf2 = sf*0.75f;
            var size = new Vector3(1.0f, 0.05f, 1.0f)*sf;
            var rot  = 35.0f/(1.0f + 0.4f*i);

            CreateBox(1.3f*sf2*Vector3.Right + dist*a*Vector3.Up + 1.3f*sf2*Vector3.Forward,
                      size,
                      Vector3.Backward + Vector3.Right, rot);
            CreateBox(1.3f*sf2*Vector3.Left + dist*b*Vector3.Up + 1.3f*sf2*Vector3.Forward,
                      size,
                      Vector3.Backward - Vector3.Right, -rot);

            CreateBox(1.3f*sf2*Vector3.Right + dist*c*Vector3.Up + 1.3f*sf2*Vector3.Backward,
                      size,
                      Vector3.Backward - Vector3.Right, rot);
            CreateBox(1.3f*sf2*Vector3.Left + dist*d*Vector3.Up + 1.3f*sf2*Vector3.Backward,
                      size,
                      Vector3.Backward + Vector3.Right, -rot);
        }

        CreateBox(new Vector3(0.0f, -4.9f, 0.0f),
                  new Vector3(5.0f, 0.1f, 5.0f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));

        CreateBox(new Vector3(-4.9f, 0.0f, 0.0f),
                  new Vector3(0.1f, 5.0f, 5.0f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));

        CreateBox(new Vector3(0.0f, 0.0f, -4.9f),
                  new Vector3(5.0f, 5.0f, 0.1f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this method.</param>
    public override void Update(float t, float dt) {
        if (mSlowMo) {
            if (mSlowMoFactor > 0.02f) {
                mSlowMoFactor -= 4.0f*dt;
            }
            else if (mSlowMoFactor < 0.02f) {
                mSlowMoFactor = 0.02f;
            }
        }
        else {
            if (mSlowMoFactor < 1.0f) {
                mSlowMoFactor += 2.0f*dt;
            }
            else if (mSlowMoFactor > 1.0f) {
                mSlowMoFactor = 1.0f;
            }
        }

        dt *= mSlowMoFactor;

        mUpdT += dt;

        base.Update(mUpdT, dt);
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt)  {
        if (mSlowMo) {
            dt *= 0.1f;
        }

        mDrawT += dt;

        Game1.Inst.GraphicsDevice.Clear(Color.White);

        base.Draw(mDrawT, dt);

        float CAM_SPEED = 5.0f;
        if (mSlowMo) {
            CAM_SPEED *= 10.0f;
        }

        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Escape)) {
            Game1.Inst.LeaveScene();
            return;
        }

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
    /// <param name="col">The ball color.</param>
    private int CreateBall(Vector3 p, Vector3 v, float r, Vector3 col) {
        var ball = AddEntity();

        AddComponent(ball, new CBody { Aabb        = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius      = r,
                                       LinDrag     = 0.2f,
                                       Velocity    = v,
                                       Restitution = 0.7f,
                                       EnableRot = true });

        AddComponent(ball, new CTransform { Position = p,
                                            Rotation = Matrix.Identity,
                                            Scale    = r*Vector3.One });

        AddComponent<C3DRenderable>(ball, new CImportedModel {
            model  = Game1.Inst.Content.Load<Model>("Models/DummySphere"),
            material = new AdsMaterial(0.15f*col,
                                             col,
                                             Vector3.One,
                                             50.0f,
                                             Game1.Inst.Content.Load<Texture2D>("Textures/Checker"))
        });

        return ball;
    }

    /// <summary>Creates a static, oriented bounding box.</summary>
    /// <param name="pos">The position of the box.</param>
    /// <param name="dim">The box dimensions, or size.</param>
    /// <param name="rotAxis">The axis of rotation (concerning the box's transformation).</param>
    /// <param name="rotDeg">The rotation, in degrees (concerning the box's transformation).</param>
    /// <param name="col">The color (optional).</param>
    private void CreateBox(Vector3  pos,
                           Vector3  dim,
                           Vector3  rotAxis,
                           float    rotDeg,
                           Vector3? col=null)
    {
        var rotRad = MathHelper.ToRadians(rotDeg);

        var box1 = AddEntity();

        if (col == null) {
            col = new Vector3(0.2f, 0.4f, 1.0f);
        }

        AddComponent<C3DRenderable>(box1, new CImportedModel {
            model  = Game1.Inst.Content.Load<Model>("Models/DummyBox"),
            material = new AdsMaterial(0.2f*col.Value,
                                            col.Value,
                                            new Vector3(1.0f, 1.0f, 1.0f),
                                            30.0f)
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
                                        Target = new Vector3(0.0f, 3.0f, 0.0f) });

        AddComponent(cam, new CTransform { Position = new Vector3(4.0f, 6.5f, 4.0f),
                                           Rotation = Matrix.Identity,
                                           Scale    = Vector3.One });

        return cam;
    }
}

}
