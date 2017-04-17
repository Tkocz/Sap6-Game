namespace EngineName.Core {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Represents an entity.</summary>
public sealed class EcsEntity {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The attached components.</summary>
    internal readonly Dictionary<Type, EcsComponent> mComponents =
        new Dictionary<Type, EcsComponent>();

    /// <summary>The scene that the entity is currently in.</summary>
    internal Scene mScene;

    /// <summary>The ID that will be assigned to the next entity
    ///          instance.</summary>
    private static int sNextID = 1;

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    /// <summary>Gets the unique entity ID.</summary>
    public int ID { get; } = Interlocked.Increment(ref sNextID);

    /// <summary>Gets the scene that the entity is in.</summary>
    public Scene Scene {
        get { return mScene; }
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Adds a component to the entity.</summary>
    /// <param name="component">The component to add to the entity.</param>
    public void AddComponent(EcsComponent component) {
        mComponents.Add(component.GetType(), component);

        var scene = mScene;
        if (scene != null) {
            scene.NotifyComponentsChanged(this);
        }
    }

    /// <summary>Retrieves the entity component of the specified type.</summary>
    /// <param name="type">The type of the component to retrieve.</param>
    /// <returns>The attached entity component of the specified type.</returns>
    public EcsComponent GetComponent(Type type) {
        return mComponents[type];
    }

    /// <summary>Retrieves the entity component of the specified type.</summary>
    /// <typeparam name="T">The type of the component to retrieve.</typeparam>
    /// <returns>The attached entity component of the specified type.</returns>
    public T GetComponent<T>() where T: EcsComponent {
        return (T)GetComponent(typeof (T));
    }

    /// <summary>Checks whether the entity has a component of the specified
    ///          type.</summary>
    /// <param name="type">The type of the component to look for.</param>
    /// <returns><see langword="true"/> if the entity has a component of the
    ///          specified type.</returns>
    public bool HasComponent(Type type) {
        return mComponents.ContainsKey(type);
    }

    /// <summary>Checks whether the entity has a component of the specified
    ///          type.</summary>
    /// <typeparam name="T">The type of the component to look for.</typeparam>
    /// <returns><see langword="true"/> if the entity has a component of the
    ///          specified type.</returns>
    public bool HasComponent<T>() where T: EcsComponent {
        return HasComponent(typeof (T));
    }

    /// <summary>Removes the component of the specified type.</summary>
    /// <param name="type">The type of the component to remove from the
    ///                    entity.</param>
    /// <returns><see langword="true"/> if a component of the specified type was
    ///          removed from the entity.</returns>
    public bool RemoveComponent(Type type) {
        bool removed = mComponents.Remove(type);

        if (removed) {
            var scene = mScene;
            if (scene != null) {
                scene.NotifyComponentsChanged(this);
            }
        }

        return removed;
    }

    /// <summary>Removes the component of the specified type.</summary>
    /// <typeparam name="T">The type of the component to remove from the
    ///                         entity.</typeparam>
    /// <returns><see langword="true"/> if a component of the specified type was
    ///          removed from the entity.</returns>
    public bool RemoveComponent<T>() where T: EcsComponent {
        return RemoveComponent(typeof (T));
    }
}

}
