using EngineName.Components.Renderable;
using EngineName.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineName.Systems {
    public class AnimationSystem : EcsSystem {
        public override void Draw(float t, float dt) {
            foreach(var animation in Game1.Inst.Scene.GetComponents<C3DRenderable>()) {
                if (animation.Value.GetType() != typeof(CAnimation))
                    continue;
                var animationComponent = (CAnimation)animation.Value;
                if (animationComponent.CurrentKeyFrame >= animationComponent.KeyFrames.Count)
                    animationComponent.CurrentKeyFrame = 0;
                animationComponent.model = animationComponent.KeyFrames[animationComponent.CurrentKeyFrame++];
            }
        }
    }
}
