namespace EngineName.Systems {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Components;
using Core;
using Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides debug overlay rendering for debugging the game. </summary>
public sealed class DebugOverlay: EcsSystem {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The aabb vertices.</summary>
    private VertexPosition[] mAabb;

    /// <summary>The effect used to draw bounding boxes.</summary>
    private BasicEffect mAabbEffect;

    /// <summary>The spritebatch to use for drawing.</summary>
    private SpriteBatch mSB;

    /// <summary>All registered debug strings.</summary>
    private readonly List<Func<float, float, string>> mStrFns =
        new List<Func<float, float, string>>();

    /// <summary>The debug overlay instance.</summary>
    private static DebugOverlay sInst;

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets the debug overlay instance.</summary>
    public static DebugOverlay Inst {
        get {
            if (sInst == null) {
                // sInst will be set in the constructor of this class.
                return new DebugOverlay();
            }

            return sInst;
        }
    }

    //--------------------------------------
    // PUBLIC CONSTRUCTORS
    //--------------------------------------

    /// <summary>Initializes a new debug overlay.</summary>
    public DebugOverlay() {
        // Set sInst to this without checking for already-existing isntance. This has the effect
        // that the singleton instance can change, but it's not an issue for the purpose of this
        // class and its usage.
        sInst = this;
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the debug overlay.</summary>
    public override void Init() {
        base.Init();

        CreateAabb();

        mAabbEffect = new BasicEffect(Game1.Inst.GraphicsDevice);
        mAabbEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 1.0f); // Magenta

        mSB = new SpriteBatch(Game1.Inst.GraphicsDevice);

        // NOTE: Do not add strings here! Add them where they make sense!
        DbgStr((t, dt) => $"Time:     {t:0.00}"           );
        DbgStr((t, dt) => $"FPS:      {1.0f/dt:0.00}"     );
        DbgStr((t, dt) => $"Entities: {Scene.NumEntities}");
    }

    /// <summary>Draws the overlay.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this
    ///                  method.</param>
    public override void Draw(float t, float dt) {
        base.Draw(t, dt);

        // Draw boxes before overlay text.
        DrawAABBs();

        var x = 16.0f;
        var y = 16.0f;

        mSB.Begin();

        foreach (var fn in mStrFns) {
            GfxUtil.DrawText(mSB, x, y, fn(t, dt), GfxUtil.DefFont, Color.Magenta);
            y += 24.0f;
        }

        mSB.End();
    }

    /// <summary>Adds a debug string to the overlay.</summary>
    /// <param name="fn">The debug string callback function.</param>
    [Conditional("DEBUG")]
    public void DbgStr(Func<float, float, string> fn) {
        mStrFns.Add(fn);
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Creates the initial aabb model used to draw bounding boxes.</summary>
    private void CreateAabb() {
        // TODO: This is a pretty ugly way of generating the vertices, should probably clean this
        //       up later.
        var n = 0;
        var v = new VertexPosition[24];

        for (var i = 0; i <= 1; i++) {
            for (var j = 0; j <= 1; j++) {
                for (var k = 0; k <= 1; k++) {
                    v[n++].Position = new Vector3(i - 0.5f, j - 0.5f, k - 0.5f);
                }
            }
        }

        for (var i = 0; i <= 1; i++) {
            for (var j = 0; j <= 1; j++) {
                for (var k = 0; k <= 1; k++) {
                    v[n++].Position = new Vector3(i - 0.5f, k - 0.5f, j - 0.5f);
                }
            }
        }

        for (var i = 0; i <= 1; i++) {
            for (var j = 0; j <= 1; j++) {
                for (var k = 0; k <= 1; k++) {
                    v[n++].Position = new Vector3(k - 0.5f, i - 0.5f, j - 0.5f);
                }
            }
        }

        mAabb = v;
    }

    /// <summary>Draws bounding boxes for all physical bodies.</summary>
    private void DrawAABBs() {
        var cam = (CCamera)Scene.GetComponents<CCamera>().Values.FirstOrDefault();

        if (cam == null) {
            // No cam, so we can't really draw anything.
            return;
        }

        // We don't want the bounding boxes to affect the depth buffer.
        Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

        mAabbEffect.Projection = cam.Projection;
        mAabbEffect.View       = cam.View;

        foreach (var component in Scene.GetComponents<CBody>().Values) {
            var body = (CBody)component;

            // Figure out the size of the aabb and scale our pre-computed aabb model accordingly.
            var x = body.Aabb.Max.X - body.Aabb.Min.X;
            var y = body.Aabb.Max.Y - body.Aabb.Min.Y;
            var z = body.Aabb.Max.Z - body.Aabb.Min.Z;

            mAabbEffect.World = Matrix.CreateScale(x, y, z)
                              * Matrix.CreateTranslation(body.Position);

            // Draw the 12 lines making up the bounding box.
            mAabbEffect.CurrentTechnique.Passes[0].Apply();
            Game1.Inst.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, mAabb, 0, 12);
        }
    }
}


}
