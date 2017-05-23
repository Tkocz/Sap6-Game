using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill.Core;

namespace Thengill.Components
{
    public class CPlayer :EcsComponent
    {
        public int AngleDir { get; set; }
        public float Angle { get; set; }
        public bool IsAttacking { get; set; }
        public float StartTime { get; set; }
        public float AnimationTime = 0.5f;
        public Matrix originalBones;

        public CPlayer() {
            Angle = 0.0f;
            IsAttacking = false;
            AngleDir = 1;
        }
    }
}
