namespace EngineName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Core;
using Utils;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Represents a game scene.</summary>
public abstract class Scene {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The entities added to the scene.</summary>
    private readonly List<EcsEntity> m_Entities = new List<EcsEntity>();

    /// <summary>The systems in use by the scene.</summary>
    private readonly List<EcsSystem> m_Systems = new List<EcsSystem>();

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    public void AddEntity(EcsEntity entity) {
        Trace.Assert(AtomicUtil.CAS(ref entity.m_Scene, entity.m_Scene, this));

        // TODO: Probably use a message to add the entity on the next call to
        //       Update()...
        lock (m_Entities) {
            m_Entities.Add(entity);
        }
    }

    /// <summary>Add the specified system to the scene.</summary>
    /// <param name="system">The system to add.</param>
    public void AddSystem(EcsSystem system) {
        m_Systems.Add(system);
    }

    /// <summary>Performs cleanup logic for the scene.</summary>
    public virtual void Cleanup() {
        foreach (var system in m_Systems) {
            system.Init();
        }

        m_Systems.Clear();
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this
    //                   method.</param>
    public virtual void Draw(float t, float dt) {
        foreach (var system in m_Systems) {
            system.Draw(t, dt);
        }
    }

    public IEnumerable<EcsEntity> GetEntities(Type type) {
        // TODO: This is a super bad idea, but it's ok for now. Come up with
        //       a better implementation lol.
        var entities = new List<EcsEntity>();

        foreach (var entity in m_Entities) {
            if (entity.HasComponent(type)) {
                entities.Add(entity);
            }
        }

        return entities;
    }

    public IEnumerable<EcsEntity> GetEntities<T>() where T: EcsComponent {
        return GetEntities(typeof (T));
    }

    /// <summary>Performs initialization logic for the scene.</summary>
    public virtual void Init() {
        foreach (var system in m_Systems) {
            system.Init();
        }
    }

    public bool RemoveEntity(EcsEntity entity) {
        if (AtomicUtil.CAS(ref entity.m_Scene, this, null)) {
            // The entity is someone else's responsibility.
            return false;
        }

        // TODO: Probably use a message to remove the entity on the next call to
        //       Update()...
        lock (m_Entities) {
            return m_Entities.Remove(entity);
        }
    }

    /// <summary>Removes the specified system from the scene.</summary>
    /// <param name="system">The system to remove.</param>
    public void RemoveSysten(EcsSystem system) {
        m_Systems.Remove(system);
    }

    /// <summary>Updates the scene by invoking the
    ///          <see cref="EcsSystem.Update"/> method on all systems in the
    ///          scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this
    //                   method.</param>
    public virtual void Update(float t, float dt) {
        foreach (var system in m_Systems) {
            system.Update(t, dt);
        }
    }
}

}
