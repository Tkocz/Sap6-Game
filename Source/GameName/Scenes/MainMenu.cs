namespace GameName.Scenes {

//--------------------------------------
// USINGS
//--------------------------------------

using System;

using EngineName;

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
        foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()) {
            if (!type.IsSubclassOf(typeof (Scene))) {
                continue;
            }

            CreateLabel(type.Name, () => {
                Game1.Inst.EnterScene((Scene)Activator.CreateInstance(type));
            });
        }

        CreateLabel("Quit", () => {
            Game1.Inst.Exit();
        });
    }

}

}
