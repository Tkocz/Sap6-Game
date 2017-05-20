using Thengill.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Components
{
    /// <summary>
    /// AI Component. Carries AI state and flock id.
    /// </summary>
    public class CAI : EcsComponent {
        /// <summary>
        /// State of AI
        /// </summary>
        public AiState State;
        /// <summary>
        /// Flock entity ID
        /// </summary>
        public int Flock = -1;
    }
}
