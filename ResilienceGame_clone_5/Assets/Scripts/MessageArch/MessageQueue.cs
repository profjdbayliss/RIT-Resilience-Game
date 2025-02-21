using System;
using System.Collections.Generic;

/// <summary>
/// A specialized list that is also observable. Inherits from IObservable.
/// </summary>
public class MessageQueue
{

    #region variables

    /// <summary>
    /// The default size of a message list. Should be 20.
    /// </summary>
    public const int DEFAULT_SIZE = 40;

    /// <summary>
    /// A queue of messages
    /// </summary>
    private Queue<Message> messages;
    
    #endregion

    #region constructors

    /// <summary>
    /// Create a new list of Messages of DEFAULT_SIZE. Also initializes
    /// all variables to reasonable default values.
    /// </summary>
    public MessageQueue()
    {
        messages = new Queue<Message>(DEFAULT_SIZE);
    }

    #endregion

    #region methods

    /// <summary>
    /// Add an element to the list. 
    /// </summary>
    /// <param name="msg">Message to add to the list.</param>
    public void Enqueue(Message msg)
    {
        messages.Enqueue(msg);
    }

    /// <summary>
    /// Takes an element off the start of the list and returns it. 
    /// </summary>
    /// <returns>The message at the start of the list or null if the list is empty.</returns>
    public Message Dequeue()
    {

        return messages.Dequeue();
    }
    

    /// <summary>
    /// Checks whether or not the list is empty.
    /// </summary>
    /// <returns>True if the list is empty and false otherwise.</returns>
    public bool IsEmpty()
    {
        return messages.Count==0;
    }

    /// <summary>
    /// The number of messages in the list.
    /// </summary>
    /// <returns>The count of messages in the list.</returns>
    public int Count()
    {
        return messages.Count;
    }

    public void Clear()
    {
       messages.Clear();
    }
    #endregion
} // MessageQueue

