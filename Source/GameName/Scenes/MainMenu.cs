namespace GameName.Scenes {

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

        CreateLabel("Alice", () => {
            EngineName.Logging.Log.Get().Debug("Alice selected");
        });

        CreateLabel("Bob", () => {
            EngineName.Logging.Log.Get().Debug("Bob selected");
        });

        CreateLabel("Charlie", () => {
            EngineName.Logging.Log.Get().Debug("Charlie selected");
        });
    }

}

}
