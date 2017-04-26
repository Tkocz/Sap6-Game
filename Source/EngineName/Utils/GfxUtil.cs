namespace EngineName.Utils {

//--------------------------------------
// USINGS
//--------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

/// <summary>Provides a plethora of graphics related functionality such as drawing text, sprites and
///          more.</summary>
public static class GfxUtil {
    //--------------------------------------
    // NON-PUBLIC FIELDS
    //--------------------------------------

    /// <summary>The default font.</summary>
    private static SpriteFont sDefFont;

    //--------------------------------------
    // PUBLIC PROPERTIES
    //--------------------------------------

    /// <summary>Gets or sets the default font used when drawing text.</summary>
    public static SpriteFont DefFont {
        get {
            if (sDefFont == null) {
                sDefFont = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans");
            }

            return sDefFont;
        }

        set {
            sDefFont = value;
        }
    }

    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Draws text on the screen, using the specified sprite batch object.</summary>
    /// <param name="sb">The sprite batch to use to draw the text.</param>
    /// <param name="x">The x-coordinate of the position to draw the text at, in screen
    ///                 space.</param>
    /// <param name="y">The y-coordinate of the position to draw the text at, in screen
    ///                 space.</param>
    /// <param name="text">The text to draw..</param>
    /// <param name="font">The font to use to draw the text.</param>
    /// <param name="color">The color of the text to draw.</param>
    public static void DrawText(SpriteBatch sb,
                                float       x,
                                float       y,
                                string      text,
                                SpriteFont  font=null,
                                Color?      color=null)
    {
        sb.DrawString(spriteFont : font ?? DefFont,
                      text       : text,
                      position   : new Vector2(x, y),
                      color      : color ?? Color.Black,
                      rotation   : 0.0f,
                      origin     : Vector2.Zero,
                      scale      : Vector2.One,
                      effects    : SpriteEffects.None,
                      layerDepth : 0.5f);
    }
}

}
