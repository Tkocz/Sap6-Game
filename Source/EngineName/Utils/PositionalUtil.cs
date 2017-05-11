using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Utils {
    public class PositionalUtil {
        public static float Distance(Vector3 pos1, Vector3 pos2) {
            var positionDiff = pos1 - pos2;
            return (float)Math.Sqrt(
                Math.Pow(positionDiff.X, 2) + 
                Math.Pow(positionDiff.Y, 2) + 
                Math.Pow(positionDiff.Z, 2)
            );
        }
    }
}
