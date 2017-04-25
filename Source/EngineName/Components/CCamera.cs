using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components
{
    public class CCamera : EcsComponent
    {
        public Matrix View;
        public Matrix Projection;
        // Altered projection for culling a bit outside of normal projection
        public Matrix ClipProjection;
        public BoundingFrustum Frustum => new BoundingFrustum(View * ClipProjection);
        public Vector3 Target = Vector3.Zero;
        public float Heading  = 0f;
        public int Height     = -50;
        public int Distance   = 50;
    }
}
