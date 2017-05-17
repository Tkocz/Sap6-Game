using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Core
{
    public static class Enums
    {
        public enum MessageType
        {
            String,
            PlayerInfo,
            PeerInformation,
            CTransform,
            CBody,
            Entity,
            EntityLight,
            Vector3,
            Int32,
            CText,
            Unknown,
        }
    }
}
