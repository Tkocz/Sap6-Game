using EngineName;
using EngineName.Utils;
using GameName.Scenes.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Scenes {
    class ConfigSceneMenu : MenuScene {
        private int numFlocks = 0;
        private int numPowerUps = 0;
        private int numTriggers = 0;
        private int maxFlocks = 10;
        private int maxPowerUps = 10;
        private int maxTriggers = 10;
        private string[] maps = new string[]{
                "Square_island_4x4",
                "DinoIsland06"
            };
        private int selectedMap = 0;
        /// <summary>Initializes the scene.</summary>
        public override void Init() {
            base.Init();
            
            CreateLabel("Map: " + maps[selectedMap], () => {
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
            });
            CreateLabel("Flocks of Animals: " + numFlocks, () => {
                numFlocks = (numFlocks + 1) % maxFlocks;
                UpdateText("Flocks of animals: " + numFlocks);
            });
            CreateLabel("Number of Power-Ups: " + numPowerUps, () => {
                numPowerUps = (numPowerUps + 1) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
            });
            CreateLabel("Number of Triggers: " + numTriggers, () => {
                numTriggers = (numTriggers + 1) % maxTriggers;
                UpdateText("Number of Triggers: " + numTriggers);
            });
            CreateLabel("Start Game", () => {
                var configs = new WorldSceneConfig(numFlocks, numPowerUps, numTriggers, maps[selectedMap], null);
                Game1.Inst.EnterScene(new WorldScene(configs));
            });
            CreateLabel("Return", () => {
                Game1.Inst.LeaveScene();
            });

            SfxUtil.PlayMusic("Sounds/Music/MainMenu");

            OnEvent("selchanged", data => SfxUtil.PlaySound("Sounds/Effects/Click"));
        }
    }
}
