namespace EngineName.Systems {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;
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

        // Add as many strings as you like here. Anything needed to debug game.
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

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    /// <summary>Creates the initial aabb model used to draw bounding boxes.</summary>
    private void CreateAabb() {
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

    /// <summary>Adds a debug string to the overlay.</summary>
    /// <param name="fn">The debug string callback function.</param>
    private void DbgStr(Func<float, float, string> fn) {
        mStrFns.Add(fn);
    }

    /// <summary>Draws bounding boxes for all physical bodies.</summary>
    private void DrawAABBs() {
        var cam = (CCamera)Scene.GetComponents<CCamera>().Values.FirstOrDefault();

        // No cam, so we can't really draw anything.
        if (cam == null) {
            return;
        }

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

            mAabbEffect.CurrentTechnique.Passes[0].Apply();
            Game1.Inst.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, mAabb, 0, 12);
        }
    }
}


}
