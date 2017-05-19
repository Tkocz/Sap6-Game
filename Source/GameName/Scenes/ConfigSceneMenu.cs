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
using Microsoft.Xna.Framework.Graphics;

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
            "DinoIsland",
            "HeightMap",
        };
        private int selectedMap = 0;
        private bool mIsMultiplayer;
        private List<int> mPlayerList = new List<int>();
        private bool mMasterIsSet = false;
        private NetworkSystem _networkSystem;
        private List<int> labels = new List<int>();
        private int _map;
        private int _flocks;
        private int _powerups;
        private int _triggers;
        private int waiting;

        /// <summary>Initializes the scene.</summary>
        public ConfigSceneMenu(bool IsMultiplayer, string[] args) {
            mIsMultiplayer = IsMultiplayer;
            if (IsMultiplayer)
            {
                if (args != null && args.Length > 0)
                {
                    _networkSystem = new NetworkSystem(50002);
                }
                else
                    _networkSystem = new NetworkSystem();
                AddSystem(_networkSystem);
            }
        }
        public override void Init() {
            base.Init();
            
            var screenCenter = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f, Game1.Inst.GraphicsDevice.Viewport.Height * 0.5f);
            string text = "GAME";
            var largeFont = Game1.Inst.Content.Load<SpriteFont>("Fonts/FFFForward_Large");
            var textSize = largeFont.MeasureString(text);
            int id = AddEntity();
            AddComponent<C2DRenderable>(id, new CText {
                color = Color.Black,
                font = largeFont,
                format = text,
                origin = Vector2.Zero,
                position = new Vector2(
                    screenCenter.X - textSize.X * 0.5f,
                    screenCenter.Y - textSize.Y - 20
                )
            });
            text = "the game";
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

            if (mIsMultiplayer)
            {
                addWatingForPlayers();
                OnEvent("update_peers", updatePeers);
                OnEvent("selchanged", data => SfxUtil.PlaySound("Sounds/Effects/Click"));
            }
            else
            {

                SfxUtil.PlayMusic("Sounds/Music/MainMenu");
                CreateLabels();
            }

        }

        public void addWatingForPlayers()
        {
            waiting = AddEntity();
            var waitingtext = "Searching for players to join";
            var mes = mFont.MeasureString(waitingtext);
            AddComponent<C2DRenderable>(waiting,new CText
            {
                color = Color.Black,
                font =  mFont,
                format = waitingtext,
                origin = Vector2.Zero,
                position = new Vector2(Game1.Inst.GraphicsDevice.Viewport.Width * 0.5f- mes.X/2, Game1.Inst.GraphicsDevice.Viewport.Width *0.5f - 70 )
            });
        }

        private void CreateLabels()
        {
            addArrow();

            _map = CreateLabel("Map: " + maps[selectedMap], () =>
            {
                // Map Select
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
                sendMenuItem(_map);
            }, () =>
            {
                // Map Increase
                selectedMap = (selectedMap + 1) % maps.Length;
                UpdateText("Map: " + maps[selectedMap]);
                sendMenuItem(_map);
            }, () =>
            {
                // Map Decrease
                selectedMap = selectedMap > 0 ? (selectedMap - 1) % maps.Length : maps.Length-1;
                UpdateText("Map: " + maps[selectedMap]);
                sendMenuItem(_map);
            });
            labels.Add(_map);

            _flocks = CreateLabel("Flocks of Animals: " + numFlocks, () => { // Animals Select
                numFlocks = (numFlocks + 5) % maxFlocks;
                UpdateText("Flocks of Animals: " + numFlocks);
                sendMenuItem(_flocks);
            }, () => { // Animals Increase
                numFlocks = (numFlocks + 5) % maxFlocks;
                UpdateText("Flocks of Animals: " + numFlocks);
                sendMenuItem(_flocks);
            }, () => { // Animals Decrease
                numFlocks = numFlocks > 0 ? (numFlocks - 5) % maxFlocks : maxFlocks - 5;
                UpdateText("Flocks of Animals: " + numFlocks);
                sendMenuItem(_flocks);
            });
            labels.Add(_flocks);

            _powerups = CreateLabel("Number of Power-Ups: " + numPowerUps, () => { // Powerups Select
                numPowerUps = (numPowerUps + 5) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
                sendMenuItem(_powerups);
            }, () => { // Powerups Increase
                numPowerUps = (numPowerUps + 5) % maxPowerUps;
                UpdateText("Number of Power-Ups: " + numPowerUps);
                sendMenuItem(_powerups);
            }, () => { // Powerups Decrease
                numPowerUps = numPowerUps > 0 ? (numPowerUps - 5) % maxPowerUps : maxPowerUps - 5;
                UpdateText("Number of Power-Ups: " + numPowerUps);
                sendMenuItem(_powerups);
            });
            labels.Add(_powerups);

            _triggers = CreateLabel("Number of Triggers: " + numTriggers, () => { // Triggers Select
                numTriggers = (numTriggers + 5) % maxTriggers;
                UpdateText("Number of Triggers: " + numTriggers);
                sendMenuItem(_triggers);
            }, () => { // Triggers Increase
                numTriggers = (numTriggers + 5) % maxTriggers;
                UpdateText("Number of Triggers: " + numTriggers);
                sendMenuItem(_triggers);
            }, () => { // Triggers Decrease
                numTriggers = numTriggers > 0 ? (numTriggers - 5) % maxTriggers : maxTriggers - 5;
                UpdateText("Number of Triggers: " + numTriggers);
                sendMenuItem(_triggers);
            });
            labels.Add(_triggers);

            CreateLabel("Start Game", () =>
            {
                WorldSceneConfig configs = null;
                if (mIsMultiplayer) { 
                    Raise("send_start_game", maps[selectedMap]);
                    configs = new WorldSceneConfig(numFlocks, numPowerUps, numTriggers, maps[selectedMap], _networkSystem);
                    }
                else
                {
                    configs = new WorldSceneConfig(numFlocks, numPowerUps, numTriggers, maps[selectedMap], null);
                }
                Game1.Inst.EnterScene(new WorldScene(configs));
            });
            CreateLabel("Return", () => {
                Game1.Inst.LeaveScene();
            });

            
           
        }

        private void updateMenuItem(object data)
        {
            var menuItem = (MenuItem)data;
            if (EntityHasComponent<C2DRenderable>(menuItem.Id))
            {
                var text = (CText) GetComponentFromEntity<C2DRenderable>(menuItem.Id);
                text.format = menuItem.CText.format;
            }
            else
            {
                menuItem.CText.font = mFont;   
                AddComponent<C2DRenderable>(menuItem.Id,menuItem.CText);
            }
        }
        private void sendMenuItem(int id)
        {
            var ctext = (CText)GetComponentFromEntity<C2DRenderable>(id);
            Raise("send_menuitem", new MenuItem { CText = ctext, Id = id });
        }
        private void sendMenu()
        {
            foreach (var id in labels)
            {
               sendMenuItem(id);
            }
        }

        private void onStartGame(object data)
        {
            var mapName = (string) data;
            var configs = new WorldSceneConfig(0, 0, 0, mapName, _networkSystem);
            Game1.Inst.EnterScene(new WorldScene(configs));
        }

        private void updatePeers(object input) {
            var data  = input as List<NetworkPlayer>;
            if (data == null) return;

            if (!mMasterIsSet) {
                // find if i am master or slave
                IsSlave = !data[0].You;
                if (!IsSlave)
                {
                    CreateLabels();
                    sendMenu();
                }
                else
                {
                    OnEvent("network_menu_data_received", updateMenuItem);
                    OnEvent("startgame",onStartGame);
                }
                mMasterIsSet = true;
                RemoveEntity(waiting);
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
                var text = string.Format((i == 0 ? "M " : "") + "{0}", player.IP);
                var textSize = mFont.MeasureString(text);
                AddComponent<C2DRenderable>(id, new CText {
                    format = text,
                    color = player.You ? Color.Black : Color.Gray,
                    font = mFont,
                    origin = Vector2.Zero,
                    position = new Vector2(screenWidth - screenWidth * 0.1f - textSize.X, screenWidth * 0.05f + i * 30)
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
            fakeNetworkList.Add(new NetworkPlayer { IP = fakeNetworkList.Count == 1 ? "YOU" : "localhost", Time = DateTime.Now, You = fakeNetworkList.Count == 1 });
        }
    }
}
