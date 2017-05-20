using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill.Components;
using Thengill.Core;

namespace Thengill.Core
{
    public abstract class AiState {
        public AiState(int id)
        {
            entityId = id;
        }
        public int entityId;
        public abstract void Handle(float t, float dt);
    }
}
