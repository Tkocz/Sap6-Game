using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components.Renderable
{
    public class CText : C2DRenderable
    {
        public SpriteFont font;
        public string format;
        public Vector2 origin;
    }
}
