using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EngineName.Systems
{
    public class ChatSystem : EcsSystem
    {
        private int textheight = 320;
        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("key_to_write", data => handleKey((Keys)data));
            Game1.Inst.Scene.OnEvent("network_data_text", data => writeToChat((string)data));
        }

        private void writeToChat(string chatmessage)
        {
            var id = Game1.Inst.Scene.AddEntity();
            Game1.Inst.Scene.AddComponent<C2DRenderable>(id, new CText()
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/sector034"),
                format = chatmessage,
                color = Color.White,
                position = new Vector2(300, textheight+=20),
                origin = Vector2.Zero
            });
        }

        private void handleKey(Keys key)
        {
           var text = (CText)Game1.Inst.Scene.GetComponentFromEntity<C2DRenderable>(0);

            if (key == Keys.Back)
            {
                if (text.format.Length > 0)
                    text.format = text.format.Remove(text.format.Length - 1);
            }
            else if (key == Keys.Enter)
            {
                Game1.Inst.Scene.Raise("send_to_peer",text.format);
                text.format = "";
            }
            else
                text.format += key.ToString();
        }
        public override void Update(float t, float dt)
        {

      

        }
    }
}
