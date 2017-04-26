namespace EngineName.Utils {

using Microsoft.Xna.Framework.Audio;

/// <summary>Provides basic functionality for audio playback.</summary>
public static class SfxUtil {

/// <summary>Loads and plays the specified sound.</summary>
/// <param name="name">The name of the sound asset to play.</param>
/// <param name="vol">The volume to use for playback (1.0 = 100%).</param>
/// <param name="pitch">The pitch to use for playback (0.0 = original).</param>
/// <param name="pan">The pan to use (left-right, 0.0 = center).</param>
public static void PlaySound(string name, float vol=1.0f, float pitch=0.0f, float pan=0.0f) {
    // TODO: We probably want to store this in some lookup table even though MonoGame stores a
    //       lookup table internally, see link:
    // https://github.com/labnation/MonoGame/blob/master/MonoGame.Framework/Content/ContentManager.cs#L197
    var sound = Game1.Inst.Content.Load<SoundEffect>(name);

    sound.Play(vol, pitch, pan);
}

}

}
