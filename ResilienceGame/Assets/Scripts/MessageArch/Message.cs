using System.Collections.Generic;
using System.Text;

/// <summary>
/// The types of messages. 
/// This changes depending on the game.
/// </summary>
public enum CardMessageType
{
    StartTurn,
    EndTurn,
    IncrementTurn,
    ShowCards,
    EndGame,
    None
}

/// <summary>
/// A class for all potential messages that may be sent.
/// </summary>
public class Message
{
    // not all messages have args
    private bool hasArgs = false;
    public bool HasArgs
    {
        get { return hasArgs; }
        set { hasArgs = value; }
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

    /// <summary>
    /// A list of all the arguments including command parameters for a message.
    /// This is the normal way messages are sent.
    /// </summary>
    public List<int> arguments;

    /// <summary>
    /// A constructor that sets message info for short messages without args.
    /// </summary>
    /// <param name="t">The type of the message</param>
    public Message(CardMessageType t)
    {
        type = t;
        hasArgs = false;
    }

    /// <summary>
    /// A constructor that sets message info.
    /// </summary>
    /// <param name="t">The type of the message</param>
    /// <param name="args">A list of the arguments for the message.</param>
    public Message(CardMessageType t, List<int> args)
    {
        type = t;
        hasArgs = true;
        arguments = new List<int>(args.Count);
        foreach (int element in args)
        {
            arguments.Add(element);
        }
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
    public int Count()
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
        StringBuilder str = new StringBuilder(type.ToString() + ":: ");
        if (hasArgs)
        {
            int argcount = Count();
            if (argcount != 0)
            {
                str.Append("::");
                for (int i = 0; i < argcount; i++)
                {
                    if (i < argcount - 1)
                    {
                        str.Append(arguments[i] + ": ");
                    }
                    else
                    {
                        str.Append(arguments[i]);
                    }
                }
                
            }
        }

        return str.ToString();
    }
    
}