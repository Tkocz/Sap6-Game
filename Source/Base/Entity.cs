namespace Sap6.Base {

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
public sealed class Entity {
    /*--------------------------------------
     * PRIVATE FIELDS
     *------------------------------------*/

    /// <summary>The attached components.</summary>
    private readonly Dictionary<Type, Component> m_Components =
        new Dictionary<Type, Component>();

    /// <summary>The ID that will be assigned to the next entity
    ///          instance.</summary>
    private static int s_NextID = 1;

    /*--------------------------------------
     * PUBLIC PROPERTIES
     *------------------------------------*/

    /// <summary>Gets the unique entity ID.</summary>
    public int ID { get; } = Interlocked.Increment(ref s_NextID);

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Adds a component to the entity.</summary>
    /// <param name="component">The component to add to the entity.</param>
    public void AddComponent(Component component) {
        m_Components.Add(component.GetType(), component);
    }

    /// <summary>Checks whether the entity has a component of the specified
    ///          type.</summary>
    /// <param name="type">The type of the component to look for.</param>
    /// <returns><see langword="true"/> if the entity has a component of the
    ///          specified type.</returns>
    public bool HasComponent(Type type) {
        return m_Components.ContainsKey(type);
    }

    /// <summary>Checks whether the entity has a component of the specified
    ///          type.</summary>
    /// <typeparam name="T">The type of the component to look for.</typeparam>
    /// <returns><see langword="true"/> if the entity has a component of the
    ///          specified type.</returns>
    public bool HasComponent<T>() where T: Component {
        return HasComponent(typeof (T));
    }

    /// <summary>Removes the component of the specified type.</summary>
    /// <param name="component">The type of the component to remove from the
    ///                         entity.</param>
    /// <returns><see langword="true"/> if a component of the specified type was
    ///          removed from the entity.</returns>
    public bool RemoveComponent(Type type) {
        return m_Components.Remove(type);
    }

    /// <summary>Removes the component of the specified type.</summary>
    /// <typeparam name="T">The type of the component to remove from the
    ///                         entity.</typeparam>
    /// <returns><see langword="true"/> if a component of the specified type was
    ///          removed from the entity.</returns>
    public bool RemoveComponent<T>() where T: Component {
        return RemoveComponent(typeof (T));
    }
}

}
