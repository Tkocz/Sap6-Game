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
        private int textheight = 100;
        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("key_to_write", data => handleKey((Keys)data));
            Game1.Inst.Scene.OnEvent("network_data_text", data => writeToChat((string)data));
            Game1.Inst.Scene.OnEvent("peer_data", data => updatePeerStatus((List<string>)data));
        }

        private void writeToChat(string chatmessage)
        {
            var id = Game1.Inst.Scene.AddEntity();
            Game1.Inst.Scene.AddComponent<C2DRenderable>(id, new CText()
            {
                font = Game1.Inst.Content.Load<SpriteFont>("Fonts/DroidSans"),
                format = chatmessage,
                color = Color.White,
                position = new Vector2(300, textheight+=30),
                origin = Vector2.Zero
            });
        }

        private void updatePeerStatus(List<string> peerdata)
        {
            var peer = (CText)Game1.Inst.Scene.GetComponentFromEntity<C2DRenderable>(1);
            peer.format = "Connected to ";
            peer.format =+ peerdata.Count + " peers ";
            peerdata.ForEach(x => peer.format += " " + x);

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
                writeToChat(text.format);
                text.format = "";
            }
            else if (key == Keys.LeftControl)
            {
                Game1.Inst.Scene.Raise("search_for_peers", null);
            }
            else
            {
                if (key == Keys.Space)
                {
                    text.format += " ";
                }
                else if (key == Keys.OemSemicolon)
                {
                    text.format += "O";
                }
                else if (key == Keys.OemOpenBrackets)
                {
                    text.format += "A";
                }
                else if (key == Keys.OemQuotes)
                {
                    text.format += "A";
                }
                else
                    text.format += key.ToString();
            }
            text.format =  text.format.ToLower();

        }
        public override void Update(float t, float dt)
        {

      

        }
    }
}
