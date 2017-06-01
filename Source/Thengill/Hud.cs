

namespace Thengill {

using System;
using System.Collections.Generic;

using Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Hud {
    public enum VerticalAnchor {
        Top,
        Center,
        Bottom,
    };
    public enum HorizontalAnchor {
        Left,
        Center,
        Right
    };
    public abstract class Visual {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract void Draw(SpriteBatch sb, int x, int y);
    };

        public class SpriteVisual: Visual {
        private float mScale;
        private Texture2D mSprite;

        public override int Width {
            get { return (int)(mSprite.Width * mScale); }
        }

        public override int Height {
            get { return (int)(mSprite.Height * mScale); }
        }

        public SpriteVisual(string asset, float scale=1.0f) {
            mSprite = Game1.Inst.Content.Load<Texture2D>(asset);
            mScale  = scale;
        }

        public override void Draw(SpriteBatch sb, int x, int y) {
            sb.Draw(texture         : mSprite,
                    position        : new Vector2(x, y),
                    sourceRectangle : null,
                    color           : Color.White,
                    rotation        : 0.0f,
                    origin          : Vector2.Zero,
                    scale           : mScale,
                    effects         : SpriteEffects.None,
                    layerDepth      : 0.0f);
        }
    }

    public class TextVisual: Visual {
        private SpriteFont mFont;

        public Func<string> Str { get; }
        public Color Color { get; set; }
        public override int Width {
            get { return (int)mFont.MeasureString(Str()).X; }
        }

        public override int Height {
            get { return (int)mFont.MeasureString(Str()).Y; }
        }

        public TextVisual(Func<string> str, Color color, SpriteFont font=null) {
            Str = str;
            Color = color;
            if (font == null) {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward");
            }

            mFont = font;
        }

        public override void Draw(SpriteBatch sb, int x, int y) {
            GfxUtil.DrawText(sb, x, y, Str(), mFont, Color);
        }
    }

    public abstract class Elem {
        public int X { get; set; }
        public int Y { get; set; }

        public abstract void Draw(SpriteBatch sb);

        public abstract void Update(MouseState m, KeyboardState kb);
    }

    public class ButtonElem: Elem {
        public const int S_UP = 0;
        public const int S_DOWN = 1;

        private int mState = S_UP;

        // Up and down graphics.
        private readonly Visual[] mVisuals = new Visual[2];

        private Action mOnClick;

        public ButtonElem(int x, int y, Visual up, Visual down=null) {
            if (down == null) {
                down = up ;
            }

            X = x;
            Y = y;

            mVisuals[S_UP]   = up;
            mVisuals[S_DOWN] = down;
        }

        public ButtonElem OnClick(Action cb) {
            mOnClick = cb;
            return this;
        }

        public override void Draw(SpriteBatch sb) {
            mVisuals[mState].Draw(sb, X, Y);
        }

        public override void Update(MouseState m, KeyboardState kb) {
            var visual = mVisuals[mState];

            var x = X;
            var y = Y;
            var w = visual.Width;
            var h = visual.Height;

            if (mState == S_DOWN & m.LeftButton != ButtonState.Pressed) {
                mState = S_UP;
                mOnClick?.Invoke();
                return;
            }

            if (m.X < x || m.X > (x+w) || m.Y < y || m.Y > (y+h)) {
                return;
            }

            if (m.LeftButton == ButtonState.Pressed) {
                mState = S_DOWN;
            }
        }
    }

    private SpriteBatch mSB;

    private readonly Dictionary<string,Elem> mElems = new Dictionary<string,Elem>();

    public Hud() {
        mSB = new SpriteBatch(Game1.Inst.GraphicsDevice);
            Game1.Inst.Scene.OnEvent("removeHudElement", data => { RemoveElement((string)data); });
        }

    public void RemoveElement(string item)
    {
        string keytoremove = "";
        foreach (var key in mElems.Keys)
        {
            if (key.Contains(item))
            {
                keytoremove = key;
                break;
            }
        }
        
        if(!string.IsNullOrEmpty(keytoremove))
        {
            mElems.Remove(keytoremove);
        }
    }

    public ButtonElem Button(string name, int x, int y, Visual up, Visual down=null, VerticalAnchor vertAnchor = VerticalAnchor.Top, HorizontalAnchor horAnchor = HorizontalAnchor.Left) {
        if (horAnchor == HorizontalAnchor.Center)
            x -= (int)(up.Width * 0.5f);
        if (horAnchor == HorizontalAnchor.Right)
            x -= up.Width;
        if(vertAnchor == VerticalAnchor.Center)
            y += (int)(up.Height*0.5f);
        if (vertAnchor == VerticalAnchor.Bottom)
            y -= up.Height;

        var button = new ButtonElem(x, y, up, down);

        mElems.Add(name,button);

        return button;
    }

    public TextVisual Text(Func<string> str, Color color) {
        return new TextVisual(str, color);
    }

    public SpriteVisual Sprite(string asset, float scale=1.0f) {
        return new SpriteVisual(asset, scale);
    }

    public void Draw(int player) {
        mSB.Begin(SpriteSortMode.Deferred);
        foreach (var elem in mElems.Values) {
            elem.Draw(mSB);
        }

        mSB.End();
    }

    public void Update() {
        var m  = Mouse.GetState();
        var kb = Keyboard.GetState();

        foreach (var elem in mElems.Values) {
            elem.Update(m, kb);
        }
    }
}

}
