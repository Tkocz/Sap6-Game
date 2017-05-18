namespace EngineName {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Collections.Generic;

using Components;
using Components.Renderable;
using Logging;
using Systems;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//--------------------------------------
// CLASSES
//--------------------------------------

// TODO: Might be more sane to move this class to somewhere else eventually.

/// <summary>Provides a base scene implementation for menus.</summary>
public abstract class MenuScene: Scene {
    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets or sets the font used to render menu text.</summary>
    public SpriteFont Font {
        get { return mFont; }
        set {
            if (value ==  mFont) {
                return;
            }

            // Update all current items to use the new font.
            foreach (var item in mItems) {
                item.Text.font = value;
            }

            mFont = value;
        }
    }

    /// <summary>Gets or sets the key used to move up amongst the menu items.</summary>
    public Keys MoveUpKey { get; set; } = Keys.Up;

    /// <summary>Gets or sets the key used to move down amongst the menu items.</summary>
    public Keys MoveDownKey { get; set; } = Keys.Down;

    /// <summary>Gets or sets the key used to select the highlighted menu item.</summary>
    public Keys SelectKey   { get; set; } = Keys.Enter;
    public Keys IncreaseKey   { get; set; } = Keys.Right;
    public Keys DecreaseKey   { get; set; } = Keys.Left;

        //--------------------------------------
        // NESTED TYPES
        //--------------------------------------

        /// <summary>Represents a menu item.</summary>
    private class MenuItem {
        /// <summary>The callback to invoke when the item is activated.</summary>
        public Action Select;

        /// <summary>The callback to invoke when the item is active and increased.</summary>
        public Action Increase;

        /// <summary>The callback to invoke when the item is activat and decreased.</summary>
        public Action Decrease;

        /// <summary>The text component used to render the item.</summary>
        public CText Text;

        }

    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>Indicates whether the selection can be changed in the menu. Used to prevent
    ///          selection spamming.</summary>
    private bool mCanInteract = true;

    /// <summary>The font used to render text in the menu.</summary>
    private SpriteFont mFont;

    /// <summary>The item that have been added to the menu.</summary>
    private readonly List<MenuItem> mItems = new List<MenuItem>();

    /// <summary>The index of the currently selceted menu item.</summary>
    private int mSelIndex;

    /// <summary>The menu selection highlight component used to render the selection highlight (e.g.
    ///          an arrow pointing to the currently highlighted item).</summary>
    private CText mSelHighlight;

    private float coolDown = 0.2f;
    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the menu.</summary>
    public override void Init() {
        AddSystems(new Rendering2DSystem());

        base.Init();

        mFont = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034");

        AddComponent<C2DRenderable>(AddEntity(), mSelHighlight = new CText {
            color    = Color.Black,
            font     = mFont,
            format   = "--->",
            origin   = Vector2.Zero,
            position = new Vector2(150, 0)
        });
    }

    /// <summary>Performs draw logic (and, in the case of the <see cref="MenuScene"/> class, some
    ///          update logic, because we only need to do it once per frame.)</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public override void Draw(float t, float dt) {
        // Position the selection highlight before delegating drawing.
        mSelHighlight.position.Y = mItems[mSelIndex].Text.position.Y;
        
        Game1.Inst.GraphicsDevice.Clear(Color.White);
        base.Draw(t, dt);

        var keyboard = Keyboard.GetState();
        coolDown -= dt;
        if (coolDown > 0.0f) mCanInteract = false;

        var canMove  = true;

        if (keyboard.IsKeyDown(MoveUpKey)) {
            if (mCanInteract) {
                mSelIndex -= 1;
                if (mSelIndex < 0) {
                    mSelIndex = mItems.Count - 1;
                }

                Raise("selchanged", mSelIndex);
            }

            canMove = false;
        }

        if (keyboard.IsKeyDown(MoveDownKey)) {
            if (mCanInteract) {
                mSelIndex += 1;
                if (mSelIndex >= mItems.Count) {
                    mSelIndex = 0;
                }

                Raise("selchanged", mSelIndex);
            }

            canMove = false;
        }

        if (keyboard.IsKeyDown(SelectKey)) {
            if (mCanInteract) {
                var s = mItems[mSelIndex].Text.format;
                Log.GetLog().Debug($"Selecting menu item: {s}");
                mItems[mSelIndex].Select();
            }

            canMove = false;
        }
        if (keyboard.IsKeyDown(DecreaseKey)) {
            if (mCanInteract) {
                var s = mItems[mSelIndex].Text.format;
                Log.GetLog().Debug($"Decreasing: {s}");
                mItems[mSelIndex].Decrease?.Invoke();
            }

            canMove = false;
        }
        if (keyboard.IsKeyDown(IncreaseKey)) {
            if (mCanInteract) {
                var s = mItems[mSelIndex].Text.format;
                Log.GetLog().Debug($"Increasing: {s}");
                mItems[mSelIndex].Increase?.Invoke();
            }

            canMove = false;
        }

        mCanInteract = canMove;
    }

        //--------------------------------------
        // NON-PUBLIC METHODS
        //--------------------------------------

        /// <summary>Creates a selectable menu label with the specified text and callback.</summary>
        /// <param name="text">The text to display on the label, in the menu.</param>
        /// <param name="cbSelect">The label callback to invoke when the label is selected.</param>
        /// <param name="cbIncrease">The label callback to invoke when the label is increased.</param>
        /// <param name="cbDecrease">The label callback to invoke when the label is decreased.</param>
        /// <param name="color">The label text color.</param>
        protected void CreateLabel(string text, Action cbSelect, Action cbIncrease = null, Action cbDecrease = null, Color? color=null) {
        // TODO: Super messy solution but it's ok for now. Need better positioning of items.

        var x = 300;
        var y = 100;

        if (mItems.Count > 0) {
            y = (int)mItems[mItems.Count - 1].Text.position.Y + 30;
        }

            // ---

        var label = new MenuItem {
            Select = cbSelect,
            Increase = cbIncrease,
            Decrease = cbDecrease,
            Text   = new CText {
                color    = color ?? Color.Black,
                font     = mFont,
                format   = text,
                origin   = Vector2.Zero,
                position = new Vector2(x, y)
            }
        };

        AddComponent<C2DRenderable>(AddEntity(), label.Text);
        mItems.Add(label);
    }

        protected void UpdateText(string text) {
            mItems[mSelIndex].Text.format = text;
        } 
}

}
