using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thengill.Components.Renderable {
    /// <summary>
    /// Animation component. Stores animation keyframes and inherits a renderable model from C3DRenderable
    /// </summary>
    public class CAnimation : CImportedModel {
        /// <summary>
        /// Models used as keyframes in animation
        /// </summary>
        public List<Model> Keyframes = new List<Model>();
        /// <summary>
        /// Current keyframe position
        /// </summary>
        public float CurrentKeyframe = 0;
        /// <summary>
        /// Animation speed multiplier
        /// </summary>
        public float AnimationSpeed = 1;
    }
}
