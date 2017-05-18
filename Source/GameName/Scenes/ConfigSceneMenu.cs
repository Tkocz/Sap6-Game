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


            CreateLabel("Map: " + maps[selectedMap], () => {
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
            });
            CreateLabel("Flocks of Animals: " + numFlocks, () => {
                numFlocks = (numFlocks + 5) % maxFlocks;
                UpdateText("Flocks of Animals: " + numFlocks);
            });
            CreateLabel("Number of Power-Ups: " + numPowerUps, () => {
                numPowerUps = (numPowerUps + 5) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
            });
            CreateLabel("Number of Triggers: " + numTriggers, () => {
                numTriggers = (numTriggers + 5) % maxTriggers;
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
            OnEvent("update_peers", updatePeers);
            OnEvent("selchanged", data => SfxUtil.PlaySound("Sounds/Effects/Click"));
        }

        private void updatePeers(object input) {
            var data  = input as List<NetworkPlayer>;
            if (data == null) return;

            if (!mMasterIsSet) {
                // find if i am master or slave
                IsSlave = !data[0].You;
                mMasterIsSet = true;
            }
            // remove current player list
            foreach (var id in mPlayerList) {
                RemoveEntity(id);
            }
            // build new player list
            var screenWidth = Game1.Inst.GraphicsDevice.Viewport.Width;
            for (int i = 0; i < data.Count; i++) {
                var id = AddEntity();
                mPlayerList.Add(id);
                var player = data[i];
                var text = string.Format(id == 0 ? "M " : "" + "{0}", player.IP);
                var textSize = mFont.MeasureString(text);
                AddComponent<C2DRenderable>(id, new CText {
                    format = text,
                    color = player.You ? Color.White : Color.Gray,
                    font = mFont,
                    origin = Vector2.Zero,
                    position = new Vector2(screenWidth - screenWidth * 0.1f - textSize.X, screenWidth * 0.05f + mPlayerList.Count * 30)
                });
            }
        }

        public override void Draw(float t, float dt) {

            var keyboard = Keyboard.GetState();
            canMove = true;
            if (keyboard.IsKeyDown(Keys.A)) {
                if (mCanInteract) {
                    AddPlayer(false);
                    Raise("update_peers", fakeNetworkList);
                }
                canMove = false;
            }
            base.Draw(t, dt);
        }
        private List<NetworkPlayer> fakeNetworkList = new List<NetworkPlayer>();
        private void AddPlayer(bool slave) {
            fakeNetworkList.Add(new NetworkPlayer { IP = "localhost", Time = DateTime.Now, You = fakeNetworkList.Count == 0 });
        }
    }
}