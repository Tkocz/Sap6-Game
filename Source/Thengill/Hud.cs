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
            get { return mSprite.Width; }
        }

        public override int Height {
            get { return mSprite.Height; }
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

        public override int Width {
            get { return (int)mFont.MeasureString(Str()).X; }
        }

        public override int Height {
            get { return (int)mFont.MeasureString(Str()).Y; }
        }

        public TextVisual(Func<string> str, SpriteFont font=null) {
            Str = str;

            if (font == null) {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward");
            }

            mFont = font;
        }

        public override void Draw(SpriteBatch sb, int x, int y) {
            GfxUtil.DrawText(sb, x, y, Str(), mFont, Color.White);
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

    private readonly List<Elem> mElems = new List<Elem>();

    public Hud() {
        mSB = new SpriteBatch(Game1.Inst.GraphicsDevice);
    }

    public ButtonElem Button(int x, int y, Visual up, Visual down=null, VerticalAnchor vertAnchor = VerticalAnchor.Top, HorizontalAnchor horAnchor = HorizontalAnchor.Left) {
        if (horAnchor == HorizontalAnchor.Center)
            x -= (int)(up.Width * 0.5f);
        if (horAnchor == HorizontalAnchor.Right)
            x -= up.Width;
        if(vertAnchor == VerticalAnchor.Center)
            y += (int)(up.Height*0.5f);
        if (vertAnchor == VerticalAnchor.Bottom)
            y -= up.Height;

        var button = new ButtonElem(x, y, up, down);

        mElems.Add(button);

        return button;
    }

    public TextVisual Text(Func<string> str) {
        return new TextVisual(str);
    }

    public SpriteVisual Sprite(string asset, float scale=1.0f) {
        return new SpriteVisual(asset, scale);
    }

    public void Draw() {
        mSB.Begin(SpriteSortMode.Deferred);

        foreach (var elem in mElems) {
            elem.Draw(mSB);
        }

        mSB.End();
    }

    public void Update() {
        var m  = Mouse.GetState();
        var kb = Keyboard.GetState();

        foreach (var elem in mElems) {
            elem.Update(m, kb);
        }
    }
}

}
