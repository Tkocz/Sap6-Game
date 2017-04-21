using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Core;
using Microsoft.Xna.Framework.Input;

namespace EngineName.Components
{
    public class CInput:EcsComponent
    {
        public Keys ForwardMovementKey = Keys.W;
        public Keys BackwardMovementKey = Keys.S;
        public Keys LeftMovementKey = Keys.A;
        public Keys RightMovementKey = Keys.D;
        public Keys XRotationPlus = Keys.Up;
        public Keys XRotationMinus = Keys.Down;
        public Keys YRotationPlus = Keys.Right;
        public Keys YRotationMinus = Keys.Left;
        public Keys ZRotationPlus = Keys.Z;
        public Keys ZRotationMinus = Keys.C;
    }
}
