namespace EngineName.Systems {

using Components;
using Core;

public class LogicSystem: EcsSystem {
    public override void Update(float t, float dt) {
        base.Update(t, dt);

        foreach (var e in Scene.GetComponents<CLogic>()) {
            var logic = (CLogic)e.Value;

            logic.mTimer += dt;
            if (logic.mTimer <= logic.InvHz) {
                // Not time to do logic just yet.
                continue;
            }

            logic.Fn(t, logic.InvHz);
            logic.mTimer = 0.0f;
        }
    }
}

}
