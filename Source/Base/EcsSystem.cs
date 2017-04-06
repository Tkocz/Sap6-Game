namespace Sap6.Base {

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Represents an entity component system.</summary>
public abstract class EcsSystem {
    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Performs draw logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</summary>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</summary>
    public virtual void Draw(float t, float dt) {
    }

    /// <summary>Performs update logic specific to the system.</summary>
    /// <param name="t">The total game time, in seconds.</summary>
    /// <param name="dt">The time, in seconds, since the last call to this
    ///                  method.</summary>
    public virtual void Update(float t, float dt) {
    }
}

}
