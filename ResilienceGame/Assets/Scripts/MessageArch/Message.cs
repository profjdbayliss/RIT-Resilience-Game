using System.Collections.Generic;
using System.Text;

/// <summary>
/// The types of messages. 
/// This changes depending on the game.
/// </summary>
public enum CardMessageType
{
    GameTurnReady,
    JoinGame,
    Discard,
    PlayCard,
    DrawCards,
    None
}

/// <summary>
/// A class for all potential messages that may be sent.
/// </summary>
public class Message
{

    // messages are either a single string or a list of int's
    private bool isString = false;
    public bool IsString
    {
        get { return isString; }
        set { isString = value; }
    }
    
   

    /// <summary>
    /// The type of a specific message.
    /// </summary>
    private CardMessageType type = CardMessageType.None;

    public CardMessageType Type
    {
        get { return type; }
        set { type = value; }
    }

    private int id = -1;

    public int SenderID
    {
        get { return id; }
        set { id = value; }
    }

    /// <summary>
    /// A list of all the arguments including command parameters for a message.
    /// This is the normal way messages are sent.
    /// </summary>
    private List<int> arguments;
    /// <summary>
    /// A single string message to be sent - for when strings are necessary (like with
    /// sending a player name.
    /// </summary>
    private string stringMessage = "";

    /// <summary>
    /// Sets the type to a default of Type.None, the sender id to -1, and the arguments to a new string list.
    /// </summary>
    public Message()
    {
        type = CardMessageType.None;
        id = -1;
        isString = false;
        arguments = new List<int>(10);
    }

    /// <summary>
    /// A constructor that sets message info.
    /// </summary>
    /// <param name="id">The sender id.</param>
    /// <param name="t">The type of the message</param>
    /// <param name="args">A list of the arguments for the message.</param>
    public Message(int id, CardMessageType t, List<int> args)
    {
        type = t;
        this.id = id;
        isString = false;
        arguments = new List<int>(args.Count);
        foreach (int element in args)
        {
            arguments.Add(element);
        }
    }

    /// <summary>
    /// A special message for sending a message with a string argument
    /// </summary>
    /// <param name="int">The sender's id.</param>
    /// <param name="t">The type of the message.</param>
    /// <param name="arg">A single string argument.</param>
    public Message(int id, CardMessageType t, string arg)
    {
        type = t;
        this.id = id;
        stringMessage = arg;
        isString = true;
    }

    /// <summary>
    /// Gets a particular argument.
    /// </summary>
    /// <param name="index">The index of an argument. Zero-based.</param>
    /// <returns>An int argument or -1 if something is incorrect.</returns>
    public int getArg(int index)
    {
        if (arguments.Count > index)
        {
            return arguments[index];
        }

        return -1;
    }

    /// <summary>
    /// A count of the arguments in this message.
    /// </summary>
    /// <returns>The count of arguments for this message.</returns>
    public int argCount()
    {
        return arguments.Count;
    }

    /// <summary>
    /// The ToString of this message.
    /// </summary>
    /// <returns>A list of the type, sender id, and arguments in this message separated by colons.
    ///</returns>
    public override string ToString()
    {
        StringBuilder str = new StringBuilder(type.ToString() + "::" + id);
        if (!isString)
        {
            int argcount = argCount();
            if (argcount != 0)
            {
                str.Append("::");
                for (int i = 0; i < argcount; i++)
                {
                    if (i < argcount - 1)
                    {
                        str.Append(arguments[i] + "::");
                    }
                    else
                    {
                        str.Append(arguments[i]);
                    }
                }
                
            }
        }
        else
        {
            str.Append("::" + stringMessage);
        }

        return str.ToString();
    }
    
}