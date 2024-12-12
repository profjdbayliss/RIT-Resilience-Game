# Detailed Networking Docs

More information on creating new network messages can be found here: https://docs.google.com/document/d/1pB7Bqu_4KQR7EwNdRgnKluuYGGbpLxcVx6lznGzMc38/edit?tab=t.0#heading=h.l3e8une9heyk

The general flow of the game network code is as follows:

1. Client A plays a card, the card play creates a network message that contains information about the card play, sometimes just the card UID, card ID, and player ID, but also additional information when required.
2. Network message gets sent to the server via, it passes through the game manager and RGNetworkPlayerList classes.
3. The server 
    1. forwards the message to all clients
    2. handles the network message
4. Clients
    1. handle network message received.
5. Network message is received by client/server who then do something with it, often passing it to the card players, or to the sectors which create the animations.

## Detailed process

**Action:** *Player A plays Phishing on Player B’s Water Pumping Station*

1. Player A clicks and drags their Phishing card to on top of the Water Pumping Station.
    1. Here, on drop, the card play is checked against several variables to see if it is available to be played or not.
    2. Assuming the player can play the card:
2. The card play calls `EnqueueAndSendCardMessageUpdate()` from one of the card drop functions. 
    1. In this case we only need to provide it with a `CardID` `UniqueCardID` `cardMessageType`
    2. So we pass it, `23, 1, CardMessageType.CardUpdate`
        1. This creates an `Update` enum instance and then immediately sends it over the network.
    3. The network message gets pushed to the player’s update queue
3. In Game Manager, we call `SendUpdatesToOpponent` to send our queued updates
4. This goes back to Card Player and calls `GetNextUpdateInMessageFormat()` to turn the `Update` into a `Message`
5. We queue this created message in `mMessageQueue` which gets checked in the `Update()` loop.
    1. If it finds an update message, it sends it to the RG Network Manager
6. `UpdateObsverver()` is called with the message data
    1. This function parses the message based on the message type
    2. It then creates either a `RGNetworkLongMessage` or `RGNetworkShortMessage` and encodes the message data as an array of bytes inside of the message payload.
    3. We then send the message to all clients (if server) or to the server (if a client)
    4. In our example, we would create a `RGNetworkLongMessage` with an array of bytes representing the `23, 1, 3` (3 being the int value of the `CardUpdate` enum)
7. The network message gets received in `RGNetworkPlayerList` in one of the network receive functions depending on if its the server/client and if its a short or long message
    1. In our example we will receive the message in the `OnServerReceiveLongMessage` as we sent the message from a client to the server.
    2. This function will parse the message based on its type and then, since we know how the message was created, we reverse that same process to create another `Update` type enum from the network message
    
    ```csharp
    1.case CardMessageType.CardUpdate: {
    2.        int element = 0;
    3.        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
    4.        element += 4;
    5.        int uniqueId = GetIntFromByteArray(element, msg.payload);
    6.        element += 4;
    7.        int cardId = GetIntFromByteArray(element, msg.payload);
    8.        element += 4;
    9.        int sectorType = GetIntFromByteArray(element, msg.payload);
    10.        element += 4;
    11.        int facilityType = GetIntFromByteArray(element, msg.payload);
    12.
    13.        element += 4;
    14.
    15.        Update update = new Update {
    16.            Type = CardMessageType.CardUpdate,
    17.            UniqueID = uniqueId,
    18.            CardID = cardId,
    19.            sectorPlayedOn = (SectorType)sectorType,
    20.            FacilityPlayedOnType = (FacilityType)facilityType
    21.        };
    22.        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);
    23.        NetworkServer.SendToAll(msg); //relay to all clients
    24.        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
    25.    }
    26.    break;
    ```
    

This above code snippet is a single switch block inside of the `OnServerReceiveLongMessage` 

- Lines 2-11 manually pull integer data out of the byte array
    - **It is important to note that the order here matters**
- 15-20 create the new card update on from the received information
- 23 relays the message to all clients since its the server, this is how more than 2 player functions
- 24 passes the update back to the Game Manger.

8. Back in the Game manager (but on the other game client) we call `AddUpdateFromPlayer()` 
    1. This function will either pass it to the sector that the card was targeting, or it will pass it to the local card player if its not a card update. Our example will pass it to the sector’s `AddUpdateFromPlayer()` function, passing it the update, phase, and a reference to the card player that played the card held in a dictionary in the game manager.
    
9. `Sector.cs` receives the network message.
    1. It  either enqueues the update (if there are ongoing updates or animations to be called later) or calls `ProcessCardPlay`
    2. `ProcessCardPlay()` will call another function based on the type of card if its a facility target, sector target, hand target (draw card) ect…
    3. Our Phishing card is a facility target card so it will move to `HandleFacilityOpponentPlay()`
10. `HandleFacilityOpponentPlay()` will find the card in the local player’s hand via the card ID
    1. This is done via non unique Card ID, so we pass it `23` as the ID of Phishing, so the network client will attempt to pull a card with ID `23` out of the player’s hand
    2. It updates the history menu locally, and then passes the card over the `CreateCardAnimation()`
11. `CreateCardAnimation()` handles creating a basic card animation via coroutines.
    1. It also handles actually calling the card’s play function, which is done when the card animation completes.
12. The Card Action `CardPlay` is actually called in the case for Phishing, the action is `AddEffect` which adds a `FacilityEffect` instance to facility’s `FacilityEffectManager`
    1. This specific action is Type: `ModifyPoints` Target: `NetworkPhysical` Magnitude: `-1`
    2. So it will reduce the network and physical points on the facility by 1