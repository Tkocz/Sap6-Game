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
        private int maxFlocks = 55;
        private int maxPowerUps = 55;
        private int maxTriggers = 55;
        private string[] maps = new string[]{
                "Square_island_4x4",
                "DinoIsland06"
            };
        private int selectedMap = 0;
        /// <summary>Initializes the scene.</summary>
        public override void Init() {
            base.Init();
            
            CreateLabel("Map: " + maps[selectedMap], () => { // Map Select
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
            }, () => { // Map Increase
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
            }, () => { // Map Decrease
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
            });

            CreateLabel("Flocks of Animals: " + numFlocks, () => { // Animals Select
                numFlocks = (numFlocks + 5) % maxFlocks;
                UpdateText("Flocks of Animals: " + numFlocks);
            }, () => { // Animals Increase
                numFlocks = (numFlocks + 5) % maxFlocks;
                UpdateText("Flocks of Animals: " + numFlocks);
            }, () => { // Animals Decrease
                numFlocks = numFlocks > 0 ? (numFlocks - 5) % maxFlocks : maxFlocks - 5;
                UpdateText("Flocks of Animals: " + numFlocks);
            });

            CreateLabel("Number of Power-Ups: " + numPowerUps, () => { // Powerups Select
                numPowerUps = (numPowerUps + 5) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
            }, () => { // Powerups Increase
                numPowerUps = (numPowerUps + 5) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
            }, () => { // Powerups Decrease
                numPowerUps = numPowerUps > 0 ? (numPowerUps - 5) % maxPowerUps : maxPowerUps - 5;
                UpdateText("Number of Power-Ups: " + numPowerUps);
            });

            CreateLabel("Number of Triggers: " + numTriggers, () => { // Triggers Select
                numTriggers = (numTriggers + 5) % maxTriggers;
                UpdateText("Number of Triggers: " + numTriggers);
            }, () => { // Triggers Increase
                numTriggers = (numTriggers + 5) % maxTriggers;
                UpdateText("Number of Triggers: " + numTriggers);
            }, () => { // Triggers Decrease
                numTriggers = numTriggers > 0 ? (numTriggers - 5) % maxTriggers : maxTriggers - 5;
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
