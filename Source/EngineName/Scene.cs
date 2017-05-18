namespace EngineName {

    /*--------------------------------------
     * USINGS
     *------------------------------------*/

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Lidgren.Network;
    using Core;
    using Utils;

    using EngineName.Components;
    using EngineName.Components.Renderable;
    using Microsoft.Xna.Framework;
    /*--------------------------------------
     * CLASSES
     *------------------------------------*/

    /// <summary>Represents a game scene.</summary>
    public abstract class Scene {
        public int NumEntities {
            get { return m_Entities.Count; }
        }
        /// <summary>Represents a game scenes directed lightsource.</summary>
        public Vector3 DiffuseColor;
        public Vector3 Direction;
        public Vector3 SpecularColor;
        public Vector3 AmbientColor = new Vector3(0.2f);

        /*--------------------------------------
         * NESTED TYPES
         *------------------------------------*/

        /// <summary>Represents a pending entity add-, remove- or update
        ///          operation.</summary>
        private struct EntityOp {
            /*--------------------------------------
             * PUBLIC CONSTANTS
             *------------------------------------*/

            /// <summary>Add entity.</summary>
            public const int OP_ADD = 1;

            /// <summary>Remove entity.</summary>
            public const int OP_REMOVE = 2;

            /// <summary>Update entity components.</summary>
            public const int OP_UPDATE = 3;

            /*--------------------------------------
             * PUBLIC FIELDS
             *------------------------------------*/

            /// <summary>The operation to perform.</summary>
            public int Op;
        }

        /*--------------------------------------
         * NON-PUBLIC FIELDS
         *------------------------------------*/

        private readonly List<int> mEntsToRemove = new List<int>();

        /// <summary>The event callbacks registered in the scene.</summary>
        private readonly Dictionary<string, List<Action<object>>> mEventCBs =
            new Dictionary<string, List<Action<object>>>();

        private List<int> m_Entities = new List<int>();

        /// <summary>The systems in use by the scene.</summary>
        private readonly List<EcsSystem> m_Systems = new List<EcsSystem>();


        protected Dictionary<Type, Dictionary<int, EcsComponent>> Components = new Dictionary<Type, Dictionary<int, EcsComponent>>();
        private int EntityCounter = -1;
        //wanted a more unique id... couldnt use standard random class in C# ...when started two instances of the game... same tick equals same id.. ;) probaly change!
        private CryptoRandom rnd = new CryptoRandom();

        /*--------------------------------------
* PUBLIC METHODS
*------------------------------------*/

        /// <summary>Raises the specified event in the scene.</summary>
        /// <param name="name">The name of the event to raise.</param>
        /// <param name="data">The event data.</param>
        public void Raise(string name, object data) {
            // Event name is case insensitive.
            name = name.ToLower();

            List<Action<object>> cbs;
            if (!mEventCBs.TryGetValue(name, out cbs)) {
                // No events registered for this event name.
                return;
            }

            foreach (var cb in cbs) {
                cb(data);
            }
        }

        /// <summary>Registers a callback for a specific event.</summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="cb">The callback to invoke after the event is raised.</param>
        public void OnEvent(string name, Action<object> cb) {
            // Event name is case insensitive.
            name = name.ToLower();

            List<Action<object>> cbs;
            if (!mEventCBs.TryGetValue(name, out cbs)) {
                mEventCBs[name] = cbs = new List<Action<object>>();
            }

            cbs.Add(cb);
        }

        /// <summary>Adds the specified entity to the scene.</summary>
        /// <param name="entity">The entity to add to the scene.</param>
        public int AddEntity() {
            /*
        DebugUtil.Assert(AtomicUtil.CAS(ref entity.m_Scene, this, null),
                         "entity.m_Scene is not null!");*/

            EntityCounter = rnd.Next(0, int.MaxValue);
            m_Entities.Add(EntityCounter);
            return EntityCounter;
        }
        /// <summary>Removes the specified entity from the scene.</summary>
        /// <param name="entity">The entity to remove from the scene.</param>
        /// 
        public void RemoveEntity(int eid) {
            mEntsToRemove.Add(eid);
        }

        public void AddComponent(int id, EcsComponent component, Type type) {
            Components[type].Add(id, component);
        }

        public void AddComponent<T>(int id, T component) where T : EcsComponent {
            AddComponent(id, component, typeof (T));
        }
        public Dictionary<int, EcsComponent> GetComponents<T>() where T : EcsComponent {
            Dictionary<int, EcsComponent> r;
            if (!Components.TryGetValue(typeof (T), out r)) {
                return new Dictionary<int, EcsComponent>{};
            }

            return r;
        }

        public EcsComponent GetComponentFromEntity<T>(int id) where T : EcsComponent {
            return Components[typeof(T)][id];
        }

        public bool EntityHasComponent<T>(int id) where T : EcsComponent {
            return Components[typeof(T)].ContainsKey(id);
        }
    /// <summary>Add the specified system to the scene.</summary>
    /// <param name="system">The system to add.</param>
    public void AddSystem(EcsSystem system) {
        m_Systems.Add(system);
        system.Scene = this;
    }

    /// <summary>Add the specified systems to the scene.</summary>
    /// <param name="systems">The systems to add.</param>
    public void AddSystems(params EcsSystem[] systems) {
        foreach (var system in systems) {
            AddSystem(system);
        }
    }

    /// <summary>Performs cleanup logic for the scene.</summary>
    public virtual void Cleanup() {
        foreach (var system in m_Systems) {
            system.Cleanup();
        }

        m_Systems.Clear();
    }

    /// <summary>Draws the scene by invoking the <see cref="EcsSystem.Draw"/>
    ///          method on all systems in the scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this method.</param>
    public virtual void Draw(float t, float dt) {
        foreach (var system in m_Systems) {
            system.Draw(t, dt);
        }

        foreach (var eid in mEntsToRemove) {
            foreach (var e in Components) {
                e.Value.Remove(eid);
            }

            m_Entities.Remove(eid);
        }

        mEntsToRemove.Clear();
    }

    /// <summary>Retrieves all entities containing a component of the specified
    ///          type.</summary>
    /// <param name="type">The component type to look for.</param>
    /// <returns>All entities containing a component of the specified
    ///          type.</returns>
    public List<int> GetEntities() {
        return m_Entities;
    }

    /// <summary>Performs initialization logic for the scene.</summary>
    public virtual void Init() {

        // init all component dictionaries
        Components.Add(typeof(CTransform), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CInput), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(C3DRenderable), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(C2DRenderable), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CCamera), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CBody), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CParticle), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CLogic), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CBox), new Dictionary<int, EcsComponent>());
		Components.Add(typeof(CAI), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CSyncObject),new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CFlock),new Dictionary<int, EcsComponent>());
		Components.Add(typeof(CInventory), new Dictionary<int, EcsComponent>());
        Components.Add(typeof(CPickUp), new Dictionary<int, EcsComponent>());

#if DEBUG
            AddSystem(new Systems.FpsCounterSystem(updatesPerSec: 10));
#endif

        foreach (var system in m_Systems) {
            system.Init();
        }

        Logging.Log.GetLog().Info($"Initialized scene: {GetType().Name}");
    }

    /// <summary>Updates the scene by invoking the
    ///          <see cref="EcsSystem.Update"/> method on all systems in the
    ///          scene.</summary>
    /// <param name="t">The total game time, in seconds.</param>
    /// <param name="dt">The game time, in seconds, since the last call to this
    ///                  method.</param>
    public virtual void Update(float t, float dt) {
        HandlePendingEntities();

         var systems = m_Systems.ToArray();
        foreach (var system in systems) {
            system.Update(t, dt);
        }
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Updates the component cache for the entity.</summary>
    /// <param name="entity">The entitiy to update the component cache for.</param>
    internal void NotifyComponentsChanged(int entity) {
            /*
        lock (m_EntitiesPending) {
            m_EntitiesPending.Enqueue(new EntityOp {
                                          Entity = entity,
                                          Op     = EntityOp.OP_UPDATE
                                      });
        }*/
    }

    /// <summary>Adds the specified entity to the scene.</summary>
    /// <param name="entity">The entity to add to the scene.</param>
    private void AddEntityInternal(int entity) {
            /*
            Debug.Assert(m_Entities.Add(entity));

        var entityComponents = m_EntityComponents;
        foreach (var component in entity.m_Components) {
            var type = component.GetType();
            HashSet<EcsEntity> entities;
            if (!entityComponents.TryGetValue(type, out entities)) {
                entityComponents[type] = entities = new HashSet<EcsEntity>();
            }

            entities.Add(entity);
        }
        */
    }

    /// <summary>Handles pending entity operations (add-, remove- or
    ///          update).</summary>
    private void HandlePendingEntities() {
            /*
        // NOTE: We don't have to lock here because we never touch
        //       m_EntitiesPending between updates, only during.
        foreach (var pending in m_EntitiesPending) {
            Debug.Assert((pending.Op == EntityOp.OP_ADD)
                      || (pending.Op == EntityOp.OP_REMOVE)
                      || (pending.Op == EntityOp.OP_UPDATE));

            var e = pending.Entity;

                 if (pending.Op == EntityOp.OP_ADD   )    AddEntityInternal(e);
            else if (pending.Op == EntityOp.OP_REMOVE) RemoveEntityInternal(e);
            else if (pending.Op == EntityOp.OP_UPDATE) UpdateEntityInternal(e);
        }

        m_EntitiesPending.Clear();
        */
    }

    /// <summary>Removes the specified entity from the scene.</summary>
    /// <param name="entity">The entity to remove from the scene.</param>
    private void RemoveEntityInternal(int entity) {
            /*
        Debug.Assert(m_Entities.Remove(entity));

        foreach (var component in entity.m_Components) {
            m_EntityComponents[component.GetType()].Remove(entity);
        }
        */
    }

    /// <summary>Updates the component cache for the specified entity.</summary>
    /// <param name="entity">The entity to update.</param>
    private void UpdateEntityInternal(int entity) {
            /*
        if (entity.Scene != this) {
            // Entity is no longer in this scene.
            return;
        }

        var entityComponents = m_EntityComponents;
        foreach (var component in entity.m_Components) {
            var type = component.GetType();
            HashSet<EcsEntity> entities;
            if (!entityComponents.TryGetValue(type, out entities)) {
                entityComponents[type] = entities = new HashSet<EcsEntity>();
            }

            entities.Add(entity);
        }
        */
    }
}

}
