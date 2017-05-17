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
        private CText _textinput = null;
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
            foreach (var keyval in Game1.Inst.Scene.GetComponents<C2DRenderable>())
            {
                var peertext = Game1.Inst.Scene.GetComponentFromEntity<C2DRenderable>(keyval.Key) as CText;
                if (peertext != null && peertext.position == new Vector2(300, 20))
                {
                    peertext.format = "Connected to ";
                    peertext.format = +peerdata.Count + " peers ";
                    peerdata.ForEach(x => peertext.format += " " + x);
                }
            }

        }
        private void handleKey(Keys key)
        {
            //dumbass search
            if (_textinput == null) { 
                foreach (var keyval in Game1.Inst.Scene.GetComponents<C2DRenderable>())
                {
                    if (_textinput != null)
                        continue;
                    _textinput = Game1.Inst.Scene.GetComponentFromEntity<C2DRenderable>(keyval.Key) as CText;
                    if (_textinput == null || _textinput.position != new Vector2(300, 750))
                        return;
                }
                if(_textinput == null)
                    return;
            }

            if (key == Keys.Back)
            {
                if (_textinput.format.Length > 0)
                    _textinput.format = _textinput.format.Remove(_textinput.format.Length - 1);
            }
            else if (key == Keys.Enter)
            {
                Game1.Inst.Scene.Raise("send_to_peer", _textinput.format);
                writeToChat(_textinput.format);
                _textinput.format = "";
            }
            else if (key == Keys.LeftControl)
            {
                Game1.Inst.Scene.Raise("search_for_peers", null);
            }
            else if (key == Keys.RightControl)
            {
                Game1.Inst.Scene.Raise("send_setup_game", null);
            }
            else
            {
                if (key == Keys.Space)
                {
                    _textinput.format += " ";
                }
                else if (key == Keys.OemSemicolon)
                {
                    _textinput.format += "O";
                }
                else if (key == Keys.OemOpenBrackets)
                {
                    _textinput.format += "A";
                }
                else if (key == Keys.OemQuotes)
                {
                    _textinput.format += "A";
                }
                else
                    _textinput.format += key.ToString();
            }
            _textinput.format = _textinput.format.ToLower();

        }
        public override void Update(float t, float dt)
        {

      

        }
    }
}
