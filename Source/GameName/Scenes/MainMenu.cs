namespace GameName.Scenes {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Reflection;

using EngineName;
using EngineName.Utils;
using EngineName.Components.Renderable;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//--------------------------------------
// CLASSES
//--------------------------------------

// NOTE: This scene is a WIP and will be changed continually to provide new options. Option *pages*
//       are scoped out of this scene and should thus be *separate* scenes entered into.

/// <summary>Provides the main menu.</summary>
public sealed class MainMenu: MenuScene {
    //--------------------------------------
    // PUBLIC METHODS
    //--------------------------------------

    /// <summary>Initializes the scene.</summary>
    public override void Init() {
        base.Init();

        // Ugly, but useful during development.
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!type.IsSubclassOf(typeof (Scene)) || type == GetType()) {
                continue;
            }

            CreateLabel(type.Name, () => {
                Game1.Inst.EnterScene((Scene)Activator.CreateInstance(type));
            });
        }

        CreateLabel("Single-Player", () => {
            Game1.Inst.EnterScene(new ConfigSceneMenu(false,null));
        });

        CreateLabel("Multi-Player", () => {

            Game1.Inst.EnterScene(new ConfigSceneMenu(true ,null));
        });

        CreateLabel("Collisions", () => {
            Game1.Inst.EnterScene(new Dev.Collisions());
        });

        CreateLabel("Quit", () => {
            Game1.Inst.Exit();
        });
        addArrow();

        var screenCenter = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f, Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f);
        string text = "GAME";
        var mFont = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward_Large");
        var textSize = mFont.MeasureString(text);
        int id = AddEntity();
        AddComponent<C2DRenderable>(id, new CText {
            color = Color.Black,
            font = mFont,
            format = text,
            origin = Vector2.Zero,
            position = new Vector2(
                screenCenter.X - textSize.X * 0.5f,
                screenCenter.Y - textSize.Y - 20
            )
        });
        text = "the game";
        mFont = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward");
        textSize = mFont.MeasureString(text);
        id = AddEntity();
        AddComponent<C2DRenderable>(id, new CText {
            color = Color.Black,
            font = mFont,
            format = text,
            origin = Vector2.Zero,
            position = new Vector2(
                screenCenter.X - textSize.X * 0.5f,
                screenCenter.Y
            )
        });

        SfxUtil.PlayMusic("Sounds/Music/MainMenu");

        OnEvent("selchanged", data => SfxUtil.PlaySound("Sounds/Effects/Click"));
    }

}

}
