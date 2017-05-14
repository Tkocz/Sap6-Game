using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components.Renderable {
    public class CAnimation : C3DRenderable {
        public List<Model> KeyFrames = new List<Model>();
        public int CurrentKeyFrame = 0;
    }
}
