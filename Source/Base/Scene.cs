namespace Sap6.Base {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Utils;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Represents a game scene.</summary>
public abstract class Scene {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The systems in use by the scene.</summary>
    private readonly List<EcsSystem> m_Systems = new List<EcsSystem>();

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    public void AddEntity(EcsEntity entity) {
        Trace.Assert(AtomicUtil.CAS(ref entity.m_Scene, entity.m_Scene, this));

        // TODO: Probably use a message to add the entity on the next call to
        //       Update()...
    }

    public void AddSystem(EcsSystem system) {
        m_Systems.Add(system);
    }

    public virtual void Cleanup() {
        foreach (var system in m_Systems) {
            system.Init();
        }

        m_Systems.Clear();
    }

    public virtual void Draw(float t, float dt) {
        foreach (var system in m_Systems) {
            system.Draw(t, dt);
        }
    }

    public IEnumerable<EcsEntity> GetEntities(Type type) {
        throw new System.NotImplementedException();
    }

    public IEnumerable<EcsEntity> GetEntities<T>() where T: EcsComponent {
        return GetEntities(typeof (T));
    }

    public virtual void Init() {
        foreach (var system in m_Systems) {
            system.Init();
        }
    }

    public void RemoveEntity(EcsEntity entity) {
        if (AtomicUtil.CAS(ref entity.m_Scene, this, null)) {
            // The entity is someone else's responsibility.
            return;
        }

        // TODO: Probably use a message to remove the entity on the next call to
        //       Update()...
    }

    public void RemoveSysten(EcsSystem system) {
        m_Systems.Remove(system);
    }

    public virtual void Update(float t, float dt) {
        foreach (var system in m_Systems) {
            system.Update(t, dt);
        }
    }
}

}
