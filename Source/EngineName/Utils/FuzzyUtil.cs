using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Utils {
    public static class FuzzyUtil {
        private static float Very(float mu) {
            return (float)Math.Pow(mu, 2);
        }
        private static float Slightly(float mu) {
            return (float)Math.Pow(mu, 1.7);
        }
        private static float Little(float mu) {
            return (float)Math.Pow(mu, 1.3);
        }

        public static double MagicFunction(double x, double top) {
            return (1 / (1 + Math.Pow((x - top) / 10, 2)));
        }
    }
    public class FuzzyNumber {
        public FuzzyNumber(double value) {
            m_value = value;
        }
        private double m_value;

        public double Value {
            get { return m_value; }
            set { m_value = value; }
        }

        public bool IsTrue() {
            return m_value > 0.5;
        }

        public static FuzzyNumber operator |(FuzzyNumber A, FuzzyNumber B) {
            return new FuzzyNumber(Math.Max(A.m_value, B.m_value));
        }

        public static FuzzyNumber operator &(FuzzyNumber A, FuzzyNumber B) {
            return new FuzzyNumber(Math.Min(A.m_value, B.m_value));
        }

        public static FuzzyNumber operator !(FuzzyNumber A) {
            return new FuzzyNumber(1.0 - A.Value);
        }
    }
}
