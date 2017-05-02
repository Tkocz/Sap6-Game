using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework.Input;

namespace EngineName.Systems
{
    public class ChatSystem : EcsSystem
    {
        public override void Init()
        {
            Game1.Inst.Scene.OnEvent("KeyToType", data => WriteToScreen((Keys)data));
        }

        private void WriteToScreen(Keys key)
        {
            var text = (CText)Game1.Inst.Scene.GetComponentFromEntity<C2DRenderable>(0);

            if (key == Keys.Back)
            {
                if (text.format.Length > 0)
                    text.format = text.format.Remove(text.format.Length - 1);
            }
            else if (key == Keys.Enter)
            {
                Game1.Inst.Scene.Raise("SendToPeer",text.format);
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
