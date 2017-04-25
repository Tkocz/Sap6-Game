namespace EngineName {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;

using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Systems;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//--------------------------------------
// CLASSES
//--------------------------------------

// TODO: Might be more sane to move this class to somewhere else eventually.

public abstract class MenuScene: Scene {
    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    public SpriteFont Font { get; set; }

    public Keys MoveUpKey   { get; set; } = Keys.Up;
    public Keys MoveDownKey { get; set; } = Keys.Down;
    public Keys SelectKey   { get; set; } = Keys.Enter;

    //--------------------------------------
    // NESTED TYPES
    //--------------------------------------

    private class MenuItem {
        /// <summary>The callback to invoke when the item is activated.</summary>
        public Action Select;
    }

    private class LabelMenuItem: MenuItem {
        public CText Text;
    }

    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Indicates whether the selection can be changed in the menu. Used to prevent
    ///          selection spamming.</summary>
    private bool mCanMove = true;

    private readonly List<MenuItem> mItems = new List<MenuItem>();

    private int mSelIndex;

    private CText mSelHighlight;

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    public override void Init() {
        AddSystems(new Rendering2DSystem(),
                   new FpsCounterSystem(updatesPerSec: 10));

        base.Init();

        Font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans");

        var eid = AddEntity();

        AddComponent               (eid, new CCamera {}); // Hack needed to enable text rendering.
        AddComponent<C2DRenderable>(eid, mSelHighlight = new CText {
            color    = Color.Black,
            font     = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
            format   = "--->",
            origin   = Vector2.Zero,
            position = new Vector2(50, 0)
        });
    }

    public override void Draw(float t, float dt) {
        base.Draw(t, dt);

        var kb = Keyboard.GetState();

        var canMove = true;

        if (kb.IsKeyDown(MoveUpKey)) {
            if (mCanMove) {
                mSelIndex -= 1;
                if (mSelIndex < 0) {
                    mSelIndex = mItems.Count - 1;
                }
            }

            canMove = false;
        }

        if (kb.IsKeyDown(MoveDownKey)) {
            if (mCanMove) {
                mSelIndex += 1;
                if (mSelIndex >= mItems.Count) {
                    mSelIndex = 0;
                }
            }

            canMove = false;
        }

        if (kb.IsKeyDown(SelectKey)) {
            if (mCanMove) {
                mItems[mSelIndex].Select();
            }

            canMove = false;
        }

        mCanMove = canMove;
        mSelHighlight.position.Y = ((LabelMenuItem)mItems[mSelIndex]).Text.position.Y;
    }

    //--------------------------------------
    // NON-PUBLIC METHODS
    //--------------------------------------

    // TODO: This is retarded but works in the meantime.
    private int X = 100;
    private int Y = 100;
    protected void CreateLabel(string text, Action cb) {
        var label = new LabelMenuItem {
            Select = cb,
            Text       = new CText {
                color    = Color.Black,
                font     = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format   = text,
                origin   = Vector2.Zero,
                position = new Vector2(X, Y)
            }
        };

        var eid = AddEntity();

        AddComponent               (eid, new CCamera {}); // Hack needed to enable text rendering.
        AddComponent<C2DRenderable>(eid, label.Text     );

        mItems.Add(label);

        Y += 30;
    }
}

}
