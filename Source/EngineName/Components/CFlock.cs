using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components {
    public class CFlock : EcsComponent {
        public List<int> Members = new List<int>();

        public Vector3 Centroid { get; set; }
        public Vector3 AvgVelocity { get; set; }
        public float Radius;
    }
}
