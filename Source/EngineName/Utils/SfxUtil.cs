namespace EngineName.Utils {

using Microsoft.Xna.Framework.Audio;

public static class SfxUtil {

public static void PlaySound(string name) {
    var sound = Game1.Inst.Content.Load<SoundEffect>(name);

    sound.Play();
}

}

}
