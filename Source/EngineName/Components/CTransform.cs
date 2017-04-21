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
        public Vector3 Position;
        public Vector3 Scale;
        public Matrix Rotation;
        public Matrix Frame;
    }
}
