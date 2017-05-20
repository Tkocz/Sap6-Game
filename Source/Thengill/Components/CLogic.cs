namespace Thengill.Components {

using System;

using Core;

public sealed class CLogic: EcsComponent {
    public Action<float, float> Fn;
    public float InvHz = 1.0f/30.0f;

    internal float mTimer;
}

}
