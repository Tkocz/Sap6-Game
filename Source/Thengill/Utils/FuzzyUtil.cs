using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Utils {
    public static class FuzzyUtil {
        /// <summary>
        /// Hedge function for determining the hedge "very"
        /// </summary>
        /// <param name="mu">The FuzzyNumber to be changed by hedge function</param>
        /// <returns>FuzzyNumber after hedge</returns>
        public static FuzzyNumber Very(FuzzyNumber mu) {
            return new FuzzyNumber(Math.Pow(mu.Value, 2));
        }
        /// <summary>
        /// Hedge function for determining the hedge "slightly"
        /// </summary>
        /// <param name="mu">The FuzzyNumber to be changed by hedge function</param>
        /// <returns>FuzzyNumber after hedge</returns>
        public static FuzzyNumber Slightly(FuzzyNumber mu) {
            return new FuzzyNumber(Math.Pow(mu.Value, 1.7));
        }
        /// <summary>
        /// Hedge function for determining the hedge "little"
        /// </summary>
        /// <param name="mu">The FuzzyNumber to be changed by hedge function</param>
        /// <returns>FuzzyNumber after hedge</returns>
        public static FuzzyNumber Little(FuzzyNumber mu) {
            return new FuzzyNumber(Math.Pow(mu.Value, 1.3));
        }
        /// <summary>
        /// Gaussian membership function for determining fuzzy value
        /// </summary>
        /// <param name="x">X variable is the input for determining the results</param>
        /// <param name="top">The X-value which returns 1.0</param>
        /// <param name="width">The width of the curve</param>
        /// <returns>FuzzyNumber of membership function</returns>
        public static FuzzyNumber GaussMF(double x, double top, double width) {
            return new FuzzyNumber(1 / (1 + Math.Pow((x - top) / width, 2)));
        }
        /// <summary>
        /// Sigmoidal membership function for determining fuzzy value
        /// </summary>
        /// <param name="x">X variable is the input for determining the results</param>
        /// <param name="offset">The offset of the curve</param>
        /// <param name="width">The width of the curve, negative number if ascending curve, positive number if descending curve</param>
        /// <returns>FuzzyNumber of membership function</returns>
        public static FuzzyNumber SigMF(double x, double offset, double width) {
            return new FuzzyNumber(1 /(1 + Math.Exp(width*(x-offset))));
        }
    }
    /// <summary>
    /// Class for fuzzy numbers and logic
    /// </summary>
    public class FuzzyNumber {
        private double m_value;
        public FuzzyNumber(double value) {
            m_value = value;
        }
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
