using EngineName.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components
{
    public class CAI : EcsComponent {
        public AiState State;
        public int Flock = -1;
    }
}
