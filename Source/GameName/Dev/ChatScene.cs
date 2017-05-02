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
    public class ChatScene : Scene
    {
        public override void Draw(float t, float dt)
        {
            Game1.Inst.GraphicsDevice.Clear(Color.Aqua);
            base.Draw(t, dt);
        }

        public override void Init()
        {

            AddSystems(
                new FpsCounterSystem(updatesPerSec: 10),
                new NetworkSystem(),
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
            AddComponent(player, new CTransform() { Position = new Vector3(0, -40, 0), Scale = new Vector3(1f) });
            AddComponent(player, new CChatOutput() {Message = "temp"});
            AddComponent<C2DRenderable>(player, new CText()
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                format = "temp",
                color = Color.White,
                position = new Vector2(300, 600),
                origin = Vector2.Zero
            });

            int eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CFPS
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                format = "Sap my Low-Poly Game",
                color = Color.White,
                position = new Vector2(300, 20),
                origin = Vector2.Zero//
            });
            eid = AddEntity();
            AddComponent<C2DRenderable>(eid, new CSprite
            {
                texture = Game1.Inst.Content.Load<Texture2D>("Textures/clubbing"),
                position = new Vector2(300, 300),
                color = Color.White
            });
            
  

            Log.Get().Debug("TestScene initialized.");
        }
    }
}
