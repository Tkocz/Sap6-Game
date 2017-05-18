using EngineName;
using EngineName.Components.Renderable;
using EngineName.Utils;
using GameName.Scenes.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EngineName.Systems;

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
            "DinoIsland06",
            "DinoIsland06Mumbo"
        };
        private int selectedMap = 0;
        private bool mIsMultiplayer;
        private List<int> mPlayerList = new List<int>();
        private bool mMasterIsSet = false;

        /// <summary>Initializes the scene.</summary>
        public ConfigSceneMenu(bool IsMultiplayer) {
            mIsMultiplayer = IsMultiplayer;
        }
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
            //OnEvent("update_peers", updatePeers);
            OnEvent("selchanged", data => SfxUtil.PlaySound("Sounds/Effects/Click"));
        }

        private void updatePeers(List<NetworkPlayer> data) {
            if (!mMasterIsSet) {
                // find if i am master or slave (loop through and find out, maybe sort list after time)
                IsSlave = true;
                mMasterIsSet = true;
            }
            // remove current player list
            foreach (var id in mPlayerList) {
                RemoveEntity(id);
            }
            // build new player list
            foreach (var player in data) {
                var id = AddEntity();
                mPlayerList.Add(id);
                AddComponent<C2DRenderable>(id, new CText {
                    //format = player.text
                });
            }
        }

        public override void Draw(float t, float dt) {

            var keyboard = Keyboard.GetState();
            canMove = true;
            if (keyboard.IsKeyDown(Keys.A)) {
                if (mCanInteract) {
                    AddPlayer(true);
                }
                canMove = false;
            }
            base.Draw(t, dt);
        }
        private void AddPlayer(bool slave) {
            var screenWidth = Game1.Inst.GraphicsDevice.Viewport.Width;
            var id = AddEntity();
            var text = string.Format("Player{0}", mPlayerList.Count + 1);
            var textSize = mFont.MeasureString(text);
            var player = new CText {
                color = Color.Black,
                font = mFont,
                format = text,
                origin = Vector2.Zero,
                position = new Vector2(screenWidth - screenWidth * 0.1f - textSize.X, screenWidth * 0.05f + mPlayerList.Count * 30)
            };
        }
    }
}
