namespace EngineName {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Collections.Concurrent;
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
     * NESTED TYPES
     *------------------------------------*/

    /// <summary>Represents a pending entity add- or remove operation.</summary>
    private class PendingEntity {
        /// <summary>The entity to add or remove.</summary>
        public EcsEntity Entity;

        /// <summary>Indicates whether the entity should be removed instead of
        ///          added.</summary>
        public bool IsRemove;
    }

    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The entities added to the scene.</summary>
    private readonly HashSet<EcsEntity> m_Entities = new HashSet<EcsEntity>();

    /// <summary>The pending entities waiting to be added or removed.</summary>
    private readonly List<PendingEntity> m_EntitiesPending =
        new List<PendingEntity>();

    /// <summary>Used as a cache for entity retrieval.</summary>
    private readonly Dictionary<Type, List<EcsEntity>> m_EntityComponents =
        new Dictionary<Type, List<EcsEntity>>();

    /// <summary>The systems in use by the scene.</summary>
    private readonly List<EcsSystem> m_Systems = new List<EcsSystem>();

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Adds the specified entity to the scene.</summary>
    /// <param name="entity">The entity to add to the scene.</param>
    public void AddEntity(EcsEntity entity) {
        Trace.Assert(AtomicUtil.CAS(ref entity.m_Scene, entity.m_Scene, this));

        lock (m_EntitiesPending) {
            m_EntitiesPending.Add(new PendingEntity { Entity = entity });
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

    /// <summary>Retrieves all entities containing a component of the specified
    ///          type.</summary>
    /// <param name="type">The component type to look for.</param>
    /// <returns>All entities containing a component of the specified
    ///          type.</returns>
    public IEnumerable<EcsEntity> GetEntities(Type type) {
        List<EcsEntity> entities;
        if (m_EntityComponents.TryGetValue(type, out entities)) {
            return entities;
        }

        return new EcsEntity[0];
    }

    /// <summary>Retrieves all entities containing a component of the specified
    ///          type.</summary>
    /// <typeparam name="T">The component type to look for.</typeparam>
    /// <returns>All entities containing a component of the specified
    ///          type.</returns>
    public IEnumerable<EcsEntity> GetEntities<T>() where T: EcsComponent {
        return GetEntities(typeof (T));
    }

    /// <summary>Performs initialization logic for the scene.</summary>
    public virtual void Init() {
        foreach (var system in m_Systems) {
            system.Init();
        }
    }

    /// <summary>Removes the specified entity from the scene.</summary>
    /// <param name="entity">The entity to remove from the scene.</param>
    /// <returns><see langword="true"/> if the entity existed in the scene and
    ///          was removed,</returns>
    public bool RemoveEntity(EcsEntity entity) {
        if (AtomicUtil.CAS(ref entity.m_Scene, this, null)) {
            // The entity is someone else's responsibility.
            return false;
        }

        lock (m_EntitiesPending) {
            m_EntitiesPending.Add(new PendingEntity {
                                      Entity   = entity,
                                      IsRemove = true
                                  });
        }

        return true;
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
        HandlePendingEntities();

        foreach (var system in m_Systems) {
            system.Update(t, dt);
        }
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    private void AddEntityInternal(EcsEntity entity) {
        Debug.Assert(m_Entities.Add(entity));

        foreach (var component in entity.m_Components) {
            List<EcsEntity> entities;
            if (!m_EntityComponents.TryGetValue(component.GetType(), out entities)) {
                entities = new List<EcsEntity>();
                m_EntityComponents[component.GetType()] = entities;
            }

            entities.Add(entity);
        }
    }

    /// <summary>Adds the specified entity to the scene.</summary>
    /// <param name="entity">The entity to add to the scene.</param>
    private void HandlePendingEntities() {
        // NOTE: We don't have to lock here because we never touch
        //       m_Entitiespending between updates, only during.
        foreach (var pending in m_EntitiesPending) {
            if (pending.IsRemove) {
                RemoveEntityInternal(pending.Entity);
            }
            else {
                AddEntityInternal(pending.Entity);
            }
        }

        m_EntitiesPending.Clear();
    }

    /// <summary>Removes the specified entity from the scene.</summary>
    /// <param name="entity">The entity to remove from the scene.</param>
    private void RemoveEntityInternal(EcsEntity entity) {
        Debug.Assert(m_Entities.Remove(entity));

        foreach (var component in entity.m_Components) {
            m_EntityComponents[component.GetType()].Remove(entity);
        }
    }
}

}
