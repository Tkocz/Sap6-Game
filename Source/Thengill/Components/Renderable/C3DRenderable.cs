using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Thengill.Shaders;

namespace Thengill.Components.Renderable
{
    public class C3DRenderable:CRenderable
    {
        public Dictionary<int, MaterialShader> materials;
        public float specular = 1.0f;
        public bool enableVertexColor = false;

        public Model model;
        public Func<float, Matrix> animFn = null;
    }
}
