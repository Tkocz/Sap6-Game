using Thengill.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Components {
    public class CHealth : EcsComponent {
        public float MaxHealth;
        public float Health;
        public float DamageResistance = 1.0f;
        public float InvincibilityTime = 0;
    }
}
