using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Core;
using System;

namespace Thengill.Systems {
    /// <summary>
    /// System for animating models. Switches the model to be rendered for each draw call.
    /// </summary>
    public class AnimationSystem : EcsSystem {
        /// <summary>
        /// Animation frames per second. Could possibly be component specific.
        /// </summary>
        private int _fps = 40;
        public override void Draw(float t, float dt) {
            foreach(var animation in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                var animationComponent = animation.Value as CAnimation;
                if (animationComponent == null) continue;
                // Stop animation on dead entities (although not necessary if we just remove them instead, keeping if we want death animation or something)
                if (Game1.Inst.Scene.EntityHasComponent<CHealth>(animation.Key)) {
                    var health = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(animation.Key);
                    if (health.Health <= 0)
                        continue;
                }
                animationComponent.CurrentKeyframe += dt * _fps * animationComponent.AnimationSpeed;
                if (animationComponent.CurrentKeyframe >= animationComponent.Keyframes.Count)
                    animationComponent.CurrentKeyframe = 0;
                animationComponent.model = animationComponent.Keyframes[(int)Math.Floor(animationComponent.CurrentKeyframe)];
            }
        }
    }
}
