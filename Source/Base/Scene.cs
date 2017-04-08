namespace Sap6.Base {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System.Diagnostics;

using Utils;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

public abstract class Scene {
    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    public Scene Parent { get; internal set; }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    public void AddEntity(EcsEntity entity) {
        Trace.Assert(AtomicUtil.CAS(ref entity.m_Scene, entity.m_Scene, this));

        // TODO: Probably use a message to add the entity on the next call to
        //       Update()...
    }

    public virtual void Cleanup() {
    }

    public virtual void Draw(float t, float dt) {
    }

    public virtual void Init() {
    }

    public void RemoveEntity(EcsEntity entity) {
        Trace.Assert(AtomicUtil.CAS(ref entity.m_Scene, this, null));

        // TODO: Probably use a message to remove the entity on the next call to
        //       Update()...
    }

    public virtual void Update(float t, float dt) {
    }
}

}
