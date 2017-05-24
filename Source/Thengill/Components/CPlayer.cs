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
    public class CPlayer : EcsComponent {
        public float StartTime { get; set; }
        public bool IsAttacking = false;
        public float AnimationTime = 0.5f;
        public float AnimationProgress = 0.0f;
        
    }
}
