using Thengill.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace Thengill.Components {
    public class CHealth : EcsComponent {
        /// <summary>
        /// Maximum health points
        /// </summary>
        public float MaxHealth;
        /// <summary>
        /// Current health points
        /// </summary>
        public float Health;
        /// <summary>
        /// Factor for damage resistance
        /// </summary>
        public float DamageResistance = 1.0f;
        /// <summary>
        /// Time left of invincibility
        /// </summary>
        public float InvincibilityTime = 0;
        /// <summary>
        /// Sound file to play when killed
        /// </summary>
        public string DeathSound;
    }
}
