using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Facility;

/// <summary>
/// The types of messages. 
/// This changes depending on the game.
/// </summary>
public enum CardMessageType
{
    StartGame,
    StartNextPhase,
    EndPhase,
    IncrementTurn,
    SharePlayerType,
    ShareDiscardNumber,
    ReturnCardToDeck,
    CardUpdate,
    CardUpdateWithExtraFacilityInfo,
    ReduceCost,
    RemoveEffect,
    DiscardCard,
    //ForceDiscard,
    MeepleShare,
    ChangeCardID,
    EndGame,
    DrawCard,
    LogAction,
    SectorAssignment,
    SendSectorData,
    None
}

/// <summary>
/// A class for all potential messages that may be sent.
/// </summary>
public class Message
{
    public uint senderID;
    private bool isBytes = false;
    public bool IsBytes
    {
        get { return isBytes; }
        set { isBytes = value; }
    }

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
    public List<byte> byteArguments;

    /// <summary>
    /// A constructor that sets message info for short messages without args.
    /// </summary>
    /// <param name="t">The type of the message</param>
    public Message(CardMessageType t, uint senderID)
    {
        this.senderID = senderID;
        type = t;
        hasArgs = false;
        isBytes = false;
    }

    /// <summary>
    /// A constructor that sets message info.
    /// </summary>
    /// <param name="t">The type of the message</param>
    /// <param name="args">A list of the arguments for the message.</param>
    public Message(CardMessageType t, uint senderID, List<int> args)
    {
        this.senderID = senderID;
        type = t;
        hasArgs = true;
        isBytes = false;
        arguments = new List<int>(args);
    }

    /// <summary>
    /// A constructor that sets message info.
    /// </summary>
    /// <param name="t">The type of the message</param>
    /// <param name="args">A list of the arguments for the message.</param>
    public Message(CardMessageType t, uint senderID, List<byte> args)
    {
        this.senderID = senderID;
        type = t;
        hasArgs = true;
        isBytes = true;
        byteArguments = new List<byte>(args);
       
    }

    /// <summary>
    /// A constructor that sets message info specifically for messages such as
    /// sending new facility id info
    /// </summary>
    /// <param name="t">The type of the message</param>
    /// <param name="arg">single argument</param>
    public Message(CardMessageType t, uint senderID, int singleArg)
    {
        this.senderID = senderID;
        type = t;
        hasArgs = true;
        isBytes = false;
        arguments = new List<int>(1);
        arguments.Add(singleArg);
    }

    /// <summary>
    /// A constructor that sets message info specifically for messages such as
    /// sending new facility id info
    /// </summary>
    /// <param name="t">The type of the message</param>
    /// <param name="uniqueID">card's unique id</param>
    /// <param name="cardID">A unique card id.</param>
    public Message(CardMessageType t, uint senderID, int uniqueID, int cardID)
    {
        this.senderID = senderID;
        type = t;
        hasArgs = true;
        isBytes = false;
        arguments = new List<int>(2);
        arguments.Add(uniqueID);
        arguments.Add(cardID);
    }

    public Message(CardMessageType t, uint senderID, string messageString) {
        this.senderID = senderID;
        type = t;
        hasArgs = true;
        isBytes = true;
        byteArguments = Encoding.UTF8.GetBytes(messageString).ToList();
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
    /// <returns>A list of the type, sender id, and arguments in this message separated by colons.</returns>
    public override string ToString() {
        StringBuilder str = new StringBuilder($"MessageType: {type}");

        if (hasArgs) {
            str.Append(", Args: ");

            if (isBytes) {
                if (byteArguments != null && byteArguments.Count > 0) {
                    for (int i = 0; i < byteArguments.Count; i++) {
                        str.Append($"[{i}]={byteArguments[i]}");
                        if (i < byteArguments.Count - 1) str.Append(", ");
                    }
                }
                else {
                    str.Append("(empty)");
                }
            }
            else {
                if (arguments != null && arguments.Count > 0) {
                    for (int i = 0; i < arguments.Count; i++) {
                        str.Append($"[{GetUpdateNameFromInt(i)}]={arguments[i]}");

                        // Additional details for specific arguments
                        if (type == CardMessageType.CardUpdate && i == 3) {
                            str.Append($" (FacilityType: {(FacilityType)arguments[i]})");
                        }

                        // Additional specific types and their arguments
                        if (type == CardMessageType.ReduceCost && i == 4) {
                            str.Append($" (Amount: {arguments[i]})");
                        }
                        else if (type == CardMessageType.RemoveEffect && i == 3) {
                            str.Append($" (FacilityPlayedOnType: {(FacilityType)arguments[i]})");
                        }
                        else if (type == CardMessageType.MeepleShare && i == 4) {
                            str.Append($" (Amount: {arguments[i]})");
                        }
                        else if (type == CardMessageType.CardUpdateWithExtraFacilityInfo) {
                            if (i == 4) {
                                str.Append($" (AdditionalFacility1: {(FacilityType)arguments[i]})");
                            }
                            else if (i == 5) {
                                str.Append($" (AdditionalFacility2: {(FacilityType)arguments[i]})");
                            }
                            else if (i == 6) {
                                str.Append($" (AdditionalFacility3: {(FacilityType)arguments[i]})");
                            }
                        }

                        if (i < arguments.Count - 1) str.Append(", ");
                    }
                }
                else {
                    str.Append("(empty)");
                }
            }
        }
        else {
            str.Append(", No Args");
        }

        return str.ToString();
    }

    private string GetUpdateNameFromInt(int i) {
        return i switch {
            0 => "Player ID",
            1 => "Unique Card ID",
            2 => "Card ID",
            3 => "Facility Type",
            4 => "Additional Information", // Depending on type, it could be Amount or Facility Effect/Facility Type
            5 => "Additional Facility 1",
            6 => "Additional Facility 2",
            7 => "Additional Facility 3",
            _ => ""
        };
    }



}