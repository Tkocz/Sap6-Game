namespace GameName.Dev {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;
using EngineName.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides a simple test case for collisions. Running it, you should see four spheres
//           colliding on the screen in a sane manner.</summary>
public sealed class CollisionPlayground: Scene {
    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        AddSystems(new    PhysicsSystem(),
                   new     CameraSystem(),
                   new  RenderingSystem());

        base.Init();

        InitCam();

        //                  -- position --                  -- velocity --
        for (var i = 0; i < 20; i++) {
            CreateBall(new Vector3(-3.5f+i*0.9f,  i*0.3f, 0.0f), new Vector3( 1.0f,  0.0f, 0.0f), 1.0f );
        }

        //OnEvent("collision", data => SfxUtil.PlaySound("Sounds/Effects/Collide"));
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

        AddComponent(ball, new CBody {
            Aabb     = new BoundingBox(-Vector3.One*r, Vector3.One*r),
            Radius   = r,
            LinDrag  = 1.0f,
            Position = p,
            Velocity = v
        });

        AddComponent<C3DRenderable>(ball, new CImportedModel {
            model = Game1.Inst.Content.Load<Model>("Models/DummySphere")
        });

        AddComponent(ball, new CTransform {
            Position = p,
            Rotation = Matrix.Identity,
            Scale    = Vector3.One * r
        });

        return ball;
    }

    /// <summary>Sets up the camera.</summary>
    /// <param name="fovDeg">The camera field of view, in degrees.</param>
    /// <param name="zNear">The Z-near clip plane, in meters from the camera.</param>
    /// <param name="zFar">The Z-far clip plane, in meters from the camera..</param>
    private int InitCam(float fovDeg=90.0f, float zNear=0.01f, float zFar=100.0f) {
        var aspect = Game1.Inst.GraphicsDevice.Viewport.AspectRatio;
        var cam    = AddEntity();
        var fovRad = fovDeg*2.0f*(float)Math.PI/360.0f;
        var proj   = Matrix.CreatePerspectiveFieldOfView(fovRad, aspect, zNear, zFar);

        AddComponent(cam, new CCamera {
            ClipProjection = proj,
            Projection     = proj,
        });

        AddComponent(cam, new CTransform {
            Position = new Vector3(0.0f, 0.0f, 10.0f),
            Rotation = Matrix.Identity,
            Scale    = Vector3.One
        });

        return cam;
    }
}

}
