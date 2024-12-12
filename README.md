# Sector Down Docs

## Starting Info

This is Sector Down, an asymmetrical card game meant to simulate cyber-attacks and cyber-security principles.

The current version of this game was based on the foundation of Jessica Bayliss' game accessDenied.
Her repo can be found here: [https://github.com/profjdbayliss/accessDenied](https://github.com/profjdbayliss/accessDenied)

Task List: https://docs.google.com/spreadsheets/d/1qD-LCI60T8OVRBIYg3zjuUYa-XjXpcHyX3CcTnzzYqQ/edit?gid=1234996300#gid=1234996300

Design doc: https://docs.google.com/document/d/1XX7vvzohJw5KxB5seTvaujeh4dh9IuloLZEdLoqlr9k/edit?tab=t.0#heading=h.a264o4m8z6f9

## Current State

As of 12/2024, All current game mechanics have been implemented and are functioning mostly bug free once in game. (No game breaking, crashes, or hard locks)

There have been a few more mechanics that have been suggested, but not implemented. See the current design document for more info on these.

The game has not been tested thoroughly, for design or functionality. This is a result of the nature of a 20 player multiplayer game being built by a small team of students on a deadline.

### Gameplay

The gameplay, has been tweaked, but largely reflects the design created by the original devs. However there have been many issues throughout the semester around gameplay. I will try to outline the major issues here

- Blue players lack agency. 75% of the players are on the blue team, and the educational aspect behind the game should really be more of a lesson from the blue’s point of view then red (We want to teach about cyber defense, not how to hack infrastructure.)
    - We added many cards to attempt to help this issue with marginal success. These cards include the ones that protect your points for a few turns, ones that give temporary resistance points, ones that reduce the turn counter, and honeypot.
    - We adjusted the blue’s goal as to reduce the number of turns/weeks left in the game to 0 simulating a “caught the hacker” scenario.
    - These changes helped, but have not totally alleviated the issues. Fairly often, especially if not directly under assault from a Red player, Blue players still have little to do on their turn.
- As a personal observation, The game feels fairly complex, with many small mechanics that have little impact on the game. This results in a game that is hard to learn, but doesn’t really provide a meaningful gameplay experience. For Example, the player needs to understand many different aspects of the game just to play their first few cards:
    - What are meeples?
    - What are sectors?
    - What are facilities?
    - What are resistance points?
    - What is the turn order?
    - How do I play cards, progress phases, what are phases ect…
- All of this is before you look at your starting cards and realize that they say like “Remove one effect from a facility” or “If a facility has a backdoor, reduce its network points by 2” So now the player needs to understand even more.
    - Compare this to a starting hand in Slay the Spire which is like 3 different cards total most of which being simple deal 6 damage or gain 5 block. Very easy to understand at first, and increasing complexity as the game progresses.
- And this STILL doesn’t include meeple sharing, winning/losing the game, overtime, exhaustion, bluff cards ect..
- All of this leads me to think that something needs to be done about the core design of the game. Possibly turning this into a deck builder might be cool, or something else to reduce complexity and provide blue players with more agency.

### Interface

The interface underwent multiple iterations, ending in something reminiscent of a early 2000s flash game. We are currently waiting on our artist to provide an updated color pallet for the game board. There is room for redesigning or adding on to the current interface. This includes additional animations for card plays, card discard/draw, as well as potentially creating more screen wide effects/animations for the white cards.

### Networking

This game uses a peer to peer based networking system. It extends Mirror’s capabilities to provide a network messaging service used to pass messages between clients in order to duplicate game states across multiple computers. This means that some things become much harder to accomplish over the network, and in general, things are more complicated, harder to understand, and require more work to add to or update. 

- The process of adding new network updates is detailed here: https://docs.google.com/document/d/1pB7Bqu_4KQR7EwNdRgnKluuYGGbpLxcVx6lznGzMc38/edit?tab=t.0

The upside to the p2p system is that we do not need to maintain any dedicated server once the game is shipped.

### File Structure
[https://github.com/profjdbayliss/RIT-Resilience-Game/blob/main/docs/Important%20Files.md](Files)

# First Steps

### Playtest Playtest Playtest

The game is now in a state that it is testable by non developers. Beginning the next dev cycle with in depth playtesting, combined with targeted forms/questions to get feedback on the issues previously discussed, would be a great way to start. 

Small video guide to playtest the game by oneself: https://www.youtube.com/watch?v=h0u-rbMuPx8

### Adjust core issues

From playtest feedback, personal preference, or team decisions, create a plan to fix some of the core issues with the game.

### Add Extras

Add more animations, card effect animations, sounds ect..

# More Details

### Card Editor

- We have provided a functional card editor/creator to ship with the game. This is located in a different scene that should be accessible from the main menu.
- The editor allows you to change any data on a card. It does not, currently, provide any safety rails.
    - You could make cards that cost 999 or type whatever you wanted into the description box and have it not match what the card actually does.
    - You can also create blue cards that have red effects and vice versa.
- Creating/Editing cards can be saved as a new deck file (csv) allowing players to chose between different decks.
    - **Again there are no safety rails for confirming that players are using the same deck once in game**

### Starting up the game

- This issue has been present from the start.
- When staring up a game, the host player needs to get to the team selection screen before other players can connect.
    - If a client attempts to connect to the game while the host is in its initial loading ie. after they press ***Create Game*** but before they see the team select, both game clients will bug out and need to be returned to the main menu.
- I attempted to design a lobby system, allowing users to connect and change teams and view each other/change the deck everyone is using, but I could not get this figured out and finished.

### Networking (cont)

More information on creating new network messages can be found here: https://docs.google.com/document/d/1pB7Bqu_4KQR7EwNdRgnKluuYGGbpLxcVx6lznGzMc38/edit?tab=t.0#heading=h.l3e8une9heyk

The general flow of the game network code is as follows:

1. Client A plays a card, the card play creates a network message that contains information about the card play, sometimes just the card UID, card ID, and player ID, but also additional information when required.
2. Network message gets sent to the server via, it passes through the game manager and RGNetworkManager classes.
3. The server 
    1. forwards the message to all clients
    2. handles the network message
4. Clients
    1. handle network message received.
5. Network message is received by client/server who then do something with it, often passing it to the card players, or to the sectors which create the animations.

This means that adding info or creating new network messages is a tedious and time consuming process. You need to add code in 6 or so places and if you miss somewhere, none of it will work.

Clients and Servers also handle the messages differently, meaning that all network receiving code needs to be written twice.

This also means there is no single “Source of Truth” for stuff like card data or game state.

There are also some current technical limitations due to the network setup:

- Cards cannot effect other cards, cards are not unique
    - There is no system for tracking unique cards across multiple players. Cards are assigned unique ID when DRAWN. When a card is played it passes the unique ID and the player ID around, so other game clients can tell which card was played.
    - This means that cards cannot affect other cards. We originally had a card that was meant to reduce other cards cost, but it was a monumental amount of work, and we ended up cutting it.
    - In theory cards effects affecting other cards in your HAND should be ok, but if the card left your hand for some reason, this effect would be eliminated.

### Networking details: 
[https://github.com/profjdbayliss/RIT-Resilience-Game/blob/main/docs/Detailed%20Networking%20Docs.md](Networking Docs)

### Unique Card Tracking

- Redesigning the system which creates cards would be very beneficial. At the start of the game, all unique cards are instantiated by the card readers from the game manager. These cards are then copied, instantiated, and assigned a unique id (UID) when they are drawn by a player.
- Cards in the deck are just stored as a dictionary of card ids not UIDs. They have no other info assigned to them, so if we wanted to have anything change any of the cards in the deck, this is impossible.
- Its also not possible for every player to just instantiate all of their unique cards at game start, as this would result in multiple thousands of game objects being created on every client on game start.
- Scriptable objects are basically made exactly for stuff like holding data for a game card, but are not currently being used.
- Creating a more robust system for tracking and storing card data would be a good idea if there are plans to add more card types to the game.
    - This is not currently possible due to the way cards are setup. The data is held directly inside of a game object, that also has multiple other classes (CardData, CardFront) holding parts of the data as well.
    - This means its not possible to create a unique card without instantiating it. This should be changed.
    - Consider restructuring the card game objects by creating a single, non-MonoBehaviour class or a ScriptableObject to store all the data for a card. You can then use a list of these data objects as your deck. When needed, instantiate a visual representation of the cards. This approach simplifies the structure and resolves related issues.