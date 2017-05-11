using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Core;
using Microsoft.Xna.Framework;

namespace EngineName.Components
{
    public class CTransform:EcsComponent
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Matrix Rotation = Matrix.Identity;
        public Matrix Frame = Matrix.Identity;
        public float Heading = 0.0f;
    }
}
