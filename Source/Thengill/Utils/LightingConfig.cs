using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Utils {
    public class LightingConfig {
        public bool FogEnabled = true;
        public float FogStart = 35;
        public float FogEnd = 100;
        public Vector3 ClearColor = new Vector3(0.4f, 0.6f, 0.8f);
        public Vector3 AmbientColor = new Vector3(0.2f);
        public Vector3 Direction;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;
    }
}
