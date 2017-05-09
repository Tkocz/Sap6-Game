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

    /// <summary>Creaters a new render target.</summary>
    /// <returns>A new render target.</returns>
    /// <seealso cref="SetRT"/>
    public static RenderTarget2D CreateRT() =>
        new RenderTarget2D(Game1.Inst.GraphicsDevice,
                           Game1.Inst.GraphicsDevice.PresentationParameters.BackBufferWidth,
                           Game1.Inst.GraphicsDevice.PresentationParameters.BackBufferHeight,
                           false,
                           Game1.Inst.GraphicsDevice.PresentationParameters.BackBufferFormat,
                           DepthFormat.Depth24);

    /// <summary>Sets the current render target (pass <see langword="null"/> to restore the default
    ///          render target) to the specified render target.</summary>
    /// <param name="rt">The render target to activate.</param>
    /// <seealso cref="CreateRT"/>
    public static void SetRT(RenderTarget2D rt) => Game1.Inst.GraphicsDevice.SetRenderTarget(rt);

    /// <summary>Draws a full-screen quad (handy for applying effects).</summary>
    /// <param name="fx">The effect to use.</param>
    public static void DrawFsQuad(Effect fx) {
        // TODO: Oh lord, don't realloc here. Setup a VBO/IBO!!
        var verts = new [] {
            new VertexPositionTexture { Position          = new Vector3(-1.0f, -1.0f, 0.0f),
                                        TextureCoordinate = new Vector2( 0.0f,  1.0f) },
            new VertexPositionTexture { Position          = new Vector3(-1.0f,  1.0f, 0.0f),
                                        TextureCoordinate = new Vector2( 0.0f,  0.0f) },
            new VertexPositionTexture { Position          = new Vector3( 1.0f,  1.0f, 0.0f),
                                        TextureCoordinate = new Vector2( 1.0f,  0.0f) },
            new VertexPositionTexture { Position          = new Vector3( 1.0f, -1.0f, 0.0f),
                                        TextureCoordinate = new Vector2( 1.0f,  1.0f) },
        };

        var indices = new short[6] { 0, 1, 2, 0, 2, 3 };


        // TODO: Should probably back up state here and then restore it.
        // Game1.Inst.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        // Game1.Inst.GraphicsDevice.RasterizerState = new RasterizerState {
        //     CullMode = CullMode.None
        // };

        foreach (var pass in fx.CurrentTechnique.Passes) {
            pass.Apply();
            Game1.Inst.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList,
                verts, 0, 4,
                indices, 0, 2);
        }
    }

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
