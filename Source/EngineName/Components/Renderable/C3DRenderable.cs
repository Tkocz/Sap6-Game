using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using EngineName.Shaders;

namespace EngineName.Components.Renderable
{
    public class C3DRenderable:CRenderable
    {
        public float specular = 1.0f;
        public bool enableVertexColor = false;
        public MaterialShader material;
        public Model model;
    }
}
