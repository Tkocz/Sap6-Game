using Thengill.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Components
{
    public class CCamera : EcsComponent
    {
        public Matrix View;
        public Matrix Projection;
        // Altered projection for culling a bit outside of normal projection
        public Matrix ClipProjection;
        public BoundingFrustum Frustum => new BoundingFrustum(View * ClipProjection);
        public Vector3 Target = Vector3.Zero;
        public Vector3 Position = Vector3.Zero;
        public float Heading  = 0f;
        public float Height     = -50;
        public float Distance   = 50;
        public CCamera(){}
        public CCamera(float height, float distance){
            Height = height;
            Distance = distance;
        }
    }
}
