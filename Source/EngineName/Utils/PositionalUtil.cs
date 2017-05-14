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
        public static Quaternion GetRotation(Vector3 source, Vector3 dest, Vector3 up) {
            var dot = Vector3.Dot(source, dest);
            /*
            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(up, MathHelper.ToRadians(180.0f));
            }*/
            if (Math.Abs(dot - (1.0f)) < 0.000001f) {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Quaternion.Identity;
            }
            float rotAngle = (float)Math.Acos(dot);
            var rotAxis = Vector3.Cross(source, dest);
            rotAxis.Normalize();
            return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
        }
    }
}
