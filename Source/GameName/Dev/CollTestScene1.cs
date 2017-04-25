namespace GameName.Dev {

using System;

using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class CollTestScene1: Scene {
    public override void Init() {
        AddSystems(new FpsCounterSystem(updatesPerSec: 10),
                   new RenderingSystem(),
                   new CameraSystem(),
                   new PhysicsSystem());

        base.Init();

        InitCam();

        CreateBall(new Vector3(-3.5f, 0.0f, 0.0f), new Vector3( 1.0f, 0.0f, 0.0f));
        CreateBall(new Vector3( 3.5f, 1.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f));
        CreateBall(new Vector3( 3.5f, 14.0f, 0.0f), new Vector3(-1.0f, -3.0f, 0.0f));
        CreateBall(new Vector3( -3.5f, 14.0f, 0.0f), new Vector3(0.5f, -4.0f, 0.0f));
    }

    private int CreateBall(Vector3 p, Vector3 v) {
        var ball = AddEntity();

        AddComponent(ball, new CBody {
            Aabb     = new BoundingBox(-Vector3.One, Vector3.One),
            Position = p,
            Velocity = v
        });

        AddComponent<C3DRenderable>(ball, new CImportedModel {
            model = Game1.Inst.Content.Load<Model>("Models/DummySphere2")
        });

        AddComponent(ball, new CTransform {
            Position = p,
            Rotation = Matrix.Identity,
            Scale    = Vector3.One
        });

        return ball;
    }

    private int InitCam(float fov=90.0f) {
        // Convert to rads
        fov *= (float)Math.PI/180.0f;

        var zNear = 0.01f;
        var zFar  = 100.0f;

        var ar  = Game1.Inst.GraphicsDevice.Viewport.AspectRatio;
        var cam = AddEntity();

        AddComponent(cam, new CCamera {
            Projection     = Matrix.CreatePerspectiveFieldOfView(fov, ar, zNear, zFar),
            ClipProjection = Matrix.CreatePerspectiveFieldOfView(fov, ar, zNear, zFar)
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
