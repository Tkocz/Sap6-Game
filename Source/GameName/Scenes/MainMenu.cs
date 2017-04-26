namespace GameName.Scenes {

//--------------------------------------
// USINGS
//--------------------------------------

using System;
using System.Reflection;

using EngineName;
using EngineName.Utils;

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

        CreateLabel("Quit", () => {
            Game1.Inst.Exit();
        });

        SfxUtil.PlayMusic("Sounds/Music/MainMenu");
    }

    /// <summary>Play a sound when the menu selection is changed.</summary>
    /// <param name="selIndex">Not used.</param>
    protected override void OnSelChanged(int selIndex) {
        SfxUtil.PlaySound("Sounds/Effects/Click");
    }

}

}
