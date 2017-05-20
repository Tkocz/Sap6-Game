using Thengill.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameName.Scenes.Utils {
    public class WorldSceneConfig {
        public int numFlocks;
        public int numPowerUps;
        public int numTriggers;
        public string map;
        public NetworkSystem network;

        public WorldSceneConfig(int numFlocks, int numPowerUps, int numTriggers, string map, NetworkSystem network) {
            this.numFlocks = numFlocks;
            this.numPowerUps = numPowerUps;
            this.numTriggers = numTriggers;
            this.map = map;
            this.network = network;
        }
    }
}
