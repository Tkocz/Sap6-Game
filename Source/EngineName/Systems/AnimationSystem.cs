using EngineName.Components.Renderable;
using EngineName.Core;

namespace EngineName.Systems {
    /// <summary>
    /// System for animating models. Switches the model to be rendered for each draw call.
    /// </summary>
    public class AnimationSystem : EcsSystem {
        public override void Draw(float t, float dt) {
            foreach(var animation in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                if (animation.Value.GetType() != typeof(CAnimation))
                    continue;
                var animationComponent = (CAnimation)animation.Value;
                if (animationComponent.CurrentKeyframe >= animationComponent.Keyframes.Count)
                    animationComponent.CurrentKeyframe = 0;
                animationComponent.model = animationComponent.Keyframes[animationComponent.CurrentKeyframe++];
            }
        }
    }
}
