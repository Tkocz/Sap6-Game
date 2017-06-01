using Thengill.Core;

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
        /// <summary>
        /// Time which enables temporary locking of a certain AI state
        /// </summary>
        public float StateLockTime = 0;
    }
}
