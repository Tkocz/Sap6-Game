using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Components;
using EngineName.Core;

namespace EngineName.Core
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
