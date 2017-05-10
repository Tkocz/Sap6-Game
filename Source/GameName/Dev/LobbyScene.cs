using System.Threading;
using EngineName;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Logging;
using EngineName.Systems;
using EngineName.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameName.Scenes
{
    public class LobbyScene : Scene
    {
        private NetworkSystem _network;
        private int port = 50001;
        public LobbyScene(string[] args)
        {
            if (args.Length > 0 && args[0] == "player2")
            {
                port = 50002;
            }

        }

        public LobbyScene()
        {
        }

        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(Color.Aqua);
            base.Draw(t, dt);
        }

        public override void Init()
        {
            _network = new NetworkSystem(port);
            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                _network,
                new Rendering2DSystem(),
                new InputSystem(),
                new ChatSystem()
            );

#if DEBUG
            AddSystem(new DebugOverlay());
#endif
           
            base.Init();

            int player = AddEntity();
            AddComponent(player, new CInput());
            AddComponent(player, new CTransform() {Position = new Vector3(0, -40, 0), Scale = new Vector3(1f)});
            AddComponent<C2DRenderable>(player, new CText()
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format = "Type Here",
                color = Color.White,
                position = new Vector2(300, 750),
                origin = Vector2.Zero
            });

            var statusbar = AddEntity();
            AddComponent<C2DRenderable>(statusbar, new CText()
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format = "Waiting for players to join",
                color = Color.White,
                position = new Vector2(300, 20),
                origin = Vector2.Zero
            });
            Game1.Inst.Scene.OnEvent("startgamerequest", data =>
            {
                Game1.Inst.EnterScene(new WorldScene(_network));
            });
            //new Thread(NewThread).Start();

        }
    }
}
