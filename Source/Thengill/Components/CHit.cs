using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Thengill.Core;

namespace Thengill.Components
{
    public class CHit : EcsComponent
    {
        public int PlayerId;
        public float StartTime { get; set; }
        public bool IsAttacking = false;
        public float AnimationTime = 0.5f;
        public float AnimationProgress = 0.0f;
        public Vector3 HitBoxOffset = new Vector3(0.5f, 0.5f, -0.5f);
    }
}
