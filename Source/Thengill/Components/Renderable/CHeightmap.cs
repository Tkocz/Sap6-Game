using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Thengill.Components.Renderable
{
    public class CHeightmap : C3DRenderable {
        public Texture2D Image;
        public Color[,] HeightData;
		public List<Vector4> EnvironmentSpawn = new List<Vector4>();
        internal Color[,] ColorMap;
        public float HeighestPoint;
        public float LowestPoint;
		public Dictionary<int, string> elements;
	}
}
