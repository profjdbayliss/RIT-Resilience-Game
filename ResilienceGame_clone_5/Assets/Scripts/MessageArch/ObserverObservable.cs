using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An observer class implements this interface. Observers are those
/// that want to watch another class closely.
/// </summary>
public interface IRGObserver
{
    /// <summary>
    /// Observable classes call the update method of all registered observers.
    /// </summary>
    /// <param name="data">Any data needed by the observer from the observable object.</param>
    void UpdateObserver(Message data);
}

/// <summary>
/// Observable classes keep a list of observers and then update them when
/// changes occur.
/// </summary>
public interface IRGObservable
{
    /// <summary>
    /// A method to register an observer of this class.
    /// </summary>
    /// <param name="o">The observer object.</param>
    void RegisterObserver(IRGObserver o);

    /// <summary>
    /// A method to remove a specific observer.
    /// </summary>
    /// <param name="o">The observer object to remove.</param>
    void RemoveObserver(IRGObserver o);

    /// <summary>
    /// A method to call the update method of all observers with appropriate data.
    /// </summary>
    void NotifyObservers();
}
