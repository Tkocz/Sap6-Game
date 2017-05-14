using EngineName.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Components {
    /// <summary>
    /// Flock component. Stores necessary data for keeping flocks
    /// </summary>
    public class CFlock : EcsComponent {
        /// <summary>
        /// The entity keys of flock members
        /// </summary>
        public List<int> Members = new List<int>();
        /// <summary>
        /// Position of the flocks center
        /// </summary>
        public Vector3 Centroid { get; set; }
        /// <summary>
        /// The average velocity of the flock
        /// </summary>
        public Vector3 AvgVelocity { get; set; }
        /// <summary>
        /// The maximum radius of the flock
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Preferred movement speed of the flock
        /// </summary>
        public float PreferredMovementSpeed { get; set; } = 30f;
        /// <summary>
        /// The distance for separation between flock members
        /// </summary>
        public float SeparationDistance = 2.5f;
        /// <summary>
        /// Separation factor
        /// </summary>
        public float SeparationFactor = 1.5f;
        /// <summary>
        /// Alignment factor
        /// </summary>
        public float AlignmentFactor = 1.0f;
        /// <summary>
        /// Cohesion factor
        /// </summary>
        public float CohesionFactor = 1.0f;
    }
}
