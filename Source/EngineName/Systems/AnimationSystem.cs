using EngineName.Components;
using EngineName.Components.Renderable;
using EngineName.Core;

namespace EngineName.Systems {
    /// <summary>
    /// System for animating models. Switches the model to be rendered for each draw call.
    /// </summary>
    public class AnimationSystem : EcsSystem {
        public override void Draw(float t, float dt) {
            foreach(var animation in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                var animationComponent = animation.Value as CAnimation;
                if (animationComponent == null) continue;
                if (Game1.Inst.Scene.EntityHasComponent<CHealth>(animation.Key)) {
                    var health = (CHealth)Game1.Inst.Scene.GetComponentFromEntity<CHealth>(animation.Key);
                    if (health.Health <= 0)
                        continue;
                }
                if (animationComponent.CurrentKeyframe >= animationComponent.Keyframes.Count)
                    animationComponent.CurrentKeyframe = 0;
                animationComponent.model = animationComponent.Keyframes[animationComponent.CurrentKeyframe++];
            }
        }
    }
}
