namespace GameName.Dev {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using EngineName.Logging;
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

    /// <summary>Pseudo-random number generator.</summary>
    private Random mRnd = new Random();

    /// <summary>Delay between spawns.</summary>
    private float mSpawnInterval = 0.05f;

    /// <summary>Spawn timer.</summary>
    private float mSpawnTimer;

    /// <summary>The ball restitution.</summary>
    private float mRestitution = 0.7f;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        PhysicsSystem physics;

        AddSystems(            new LogicSystem(),
                   physics = new PhysicsSystem(),
                           new RenderingSystem());

        physics.Bounds = new BoundingBox(-2.5f*Vector3.One, 2.5f*Vector3.One);
        physics.InvSpatPartSize = 1.0f/0.5f;

#if DEBUG
        AddSystem(new DebugOverlay());
#endif

        base.Init();

        mCamID = InitCam();

        mRnd = new System.Random();

        // Spawn a few balls.
        for (var i = 0; i < 100; i++) {
            SpawnBall();
        }

        var dist = 1.1f;
        for (var i = 0; i < 3; i++) {
            var a = i - 1.0f;
            var b = a + 0.25f;
            var c = b + 0.25f;
            var d = c + 0.25f;

            var sf = 0.9f + 0.2f*i;
            var sf2 = sf*0.75f;
            var size = new Vector3(0.5f, 0.025f, 0.5f)*sf;
            var rot  = 35.0f/(1.0f + 0.4f*i);

            CreateBox(0.65f*sf2*Vector3.Right + dist*a*Vector3.Up + 0.65f*sf2*Vector3.Forward,
                      size,
                      Vector3.Backward + Vector3.Right, rot);
            CreateBox(0.65f*sf2*Vector3.Left + dist*b*Vector3.Up + 0.65f*sf2*Vector3.Forward,
                      size,
                      Vector3.Backward - Vector3.Right, -rot);

            CreateBox(0.65f*sf2*Vector3.Right + dist*c*Vector3.Up + 0.65f*sf2*Vector3.Backward,
                      size,
                      Vector3.Backward - Vector3.Right, rot);
            CreateBox(0.65f*sf2*Vector3.Left + dist*d*Vector3.Up + 0.65f*sf2*Vector3.Backward,
                      size,
                      Vector3.Backward + Vector3.Right, -rot);
        }

        CreateBox(new Vector3(0.0f, -2.4f, 0.0f),
                  new Vector3(2.5f, 0.1f, 2.5f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));

        CreateBox(new Vector3(-2.4f, 0.0f, 0.0f),
                  new Vector3(0.1f, 2.5f, 2.5f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));

        CreateBox(new Vector3(0.0f, 0.0f, -2.4f),
                  new Vector3(2.5f, 2.5f, 0.1f),
                  Vector3.Right, 0.0f,
                  new Vector3(0.02f, 0.02f, 0.02f));
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The time, in seconds, since the last call to this method.</param>
    public override void Update(float t, float dt) {
        if (mSlowMo) {
            if (mSlowMoFactor > 0.1f) {
                mSlowMoFactor -= 4.0f*dt;
            }
            else if (mSlowMoFactor < 0.1f) {
                mSlowMoFactor = 0.1f;
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

        mSpawnTimer -= dt;
        if (mSpawnTimer < 0.0f) {
            mSpawnTimer = mSpawnInterval;
            SpawnBall();            SpawnBall();            SpawnBall();
        }

        base.Update(mUpdT, dt);
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt)  {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.R)) {
            mRestitution += dt;
            if (mRestitution > 1.0f) {
                mRestitution = 1.0f;
            }

            Log.GetLog().Info($"Restitution: {mRestitution}");
        }

        if (kb.IsKeyDown(Keys.V)) {
            mRestitution -= dt;
            if (mRestitution < 0.0f) {
                mRestitution = 0.0f;
            }

            Log.GetLog().Info($"Restitution: {mRestitution}");
        }

        if (kb.IsKeyDown(Keys.E)) {
            mSpawnInterval += 0.1f*dt;

            Log.GetLog().Info($"Spawn interval: {mSpawnInterval}");
        }

        if (kb.IsKeyDown(Keys.C)) {
            mSpawnInterval -= 0.1f*dt;
            if (mSpawnInterval < 0.001f) {
                mSpawnInterval = 0.001f;
            }

            Log.GetLog().Info($"Spawn interval: {mSpawnInterval}");;
        }

        float CAM_SPEED = 2.5f;

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

        if (mSlowMo) {
            dt *= mSlowMoFactor;
        }

        mDrawT += dt;

        { // TODO: CameraSystem no longer supports anything but chase cam, so manual setup below.
            var cam = (CCamera)Game1.Inst.Scene.GetComponentFromEntity<CCamera>(mCamID);
            var camTransf = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(mCamID);
            cam.View = Matrix.CreateLookAt(camTransf.Position, cam.Target, Vector3.Up);
        }

        Game1.Inst.GraphicsDevice.Clear(Color.White);

        base.Draw(mDrawT, dt);
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Spawns a ball!</summary>
    private void SpawnBall() {
        // Colors to pick from when spawning balls.
        var cols = new [] {
            new Vector3(0.9f, 0.1f, 0.1f),
            new Vector3(0.9f, 0.9f, 0.9f),
        };

        var theta = 2.0f*MathHelper.Pi*(float)mRnd.NextDouble();
        CreateBall(new Vector3(0.5f*(float)Math.Cos(theta), 2.4f, 0.5f*(float)Math.Sin(theta)),
                   (float)mRnd.NextDouble()*Vector3.Up,
                   0.03f + (float)mRnd.NextDouble()*0.05f,
                   cols[mRnd.Next(cols.Length)]);
    }

    /// <summary>Creates a new ball in the scene with the given position and velocity.</summary>
    /// <param name="p">The ball position, in world-space.</param>
    /// <param name="v">The initial velocity to give to the ball.</param>
    /// <param name="r">The ball radius.</param>
    /// <param name="col">The ball color.</param>
    private int CreateBall(Vector3 p, Vector3 v, float r, Vector3 col) {
        var ball = AddEntity();

        var ttl = 8.0f;
        AddComponent(ball, new CLogic { Fn = (t, dt) => {
            ttl -= dt;
            if (ttl <= 0.0f) {
                Game1.Inst.Scene.RemoveEntity(ball);
            }
        }});

        AddComponent(ball, new CBody { Aabb        = new BoundingBox(-r*Vector3.One, r*Vector3.One),
                                       Radius      = r,
                                       LinDrag     = 0.2f,
                                       Velocity    = v,
                                       Restitution = mRestitution,
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
                                             Game1.Inst.Content.Load<Texture2D>("Textures/Ball"),
                                             Game1.Inst.Content.Load<Texture2D>("Textures/Grain"),
                                             0.1f)
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
                                        Target = new Vector3(0.0f, 1.5f, 0.0f) });

        AddComponent(cam, new CTransform { Position = new Vector3(2.0f, 3.2f, 2.0f),
                                           Rotation = Matrix.Identity,
                                           Scale    = Vector3.One });

        return cam;
    }
}

}
