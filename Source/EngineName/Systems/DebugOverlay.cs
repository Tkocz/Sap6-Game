namespace EngineName.Systems {

using System;
using System.Collections.Generic;

using Core;
using Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>Provides debug overlay rendering for debugging the game. </summary>
public sealed class DebugOverlay: EcsSystem {
    /// <summary>The spritebatch to use for drawing.</summary>
    private SpriteBatch mSB;

    /// <summary>All registered debug strings.</summary>
    private readonly List<Func<float, float, string>> mStrFns =
        new List<Func<float, float, string>>();

    public override void Init() {
        base.Init();

        mSB = new SpriteBatch(Game1.Inst.GraphicsDevice);

        DbgStr((t, dt) => $"Time: {t:0.00}");
        DbgStr((t, dt) => $"FPS: {1.0f/dt:0.00}");
        DbgStr((t, dt) => $"Entities: {Scene.NumEntities}");
    }

    public override void Draw(float t, float dt) {
        base.Draw(t, dt);

        var x = 16.0f;
        var y = 16.0f;

        mSB.Begin();

        foreach (var fn in mStrFns) {
            GfxUtil.DrawText(mSB, x, y, fn(t, dt), GfxUtil.DefFont, Color.Magenta);
            y += 24.0f;
        }

        mSB.End();
    }

    private void DbgStr(Func<float, float, string> fn) {
        mStrFns.Add(fn);
    }
}


}
