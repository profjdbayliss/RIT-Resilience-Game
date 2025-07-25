# Sector Down

## Starting Info

This is Sector Down, an asymmetrical card game meant to simulate cyber-attacks and cyber-security principles.

The current version of this game was based on the foundation of Jessica Bayliss' game Access Denied.
Her repo can be found here: [Access Denied](https://github.com/profjdbayliss/accessDenied)

If you are having issues joining another player's game, check to see if your system has any firewall blocking port 14623 (The port used for the server and joining clients), if you don't have access to your firewall permissions, talk to your computer's IT manager: [Troubleshooting Tips](https://www.wikihow.com/Check-if-Your-Firewall-Is-Blocking-Something#Check-for-Blocked-Ports-.28Windows.29)

## Gameplay

### Set up:
Start with 1 Red Player and up to 4 Blue Players. Another Red Player should join the game for every fifth Blue Player added.

Every Blue Player will control a Sector with 3 Facilities each. Each Facility will have different Sector Products and Resistance Points. All players will have 2 Workers of each color with Red Players having an additional Colorless Worker. 

All players begin play by drawing 5 cards from their decks


### Turn Order:
1. Every third turn a White Card is drawn.
2. Red Player(s) take their turn.
3. Blue player(s) take their turn.


### Turn Composition:
1. **DRAW PHASE:** Players must draw until they have 5 cards in their hand. If a player cannot draw any more cards the game ends. Players can discard and redraw up to 2 cards. 
    - Any Workers used in the previous turn are replenished. Workers can work Overtime for 2 turns, adding 2 Workers per color to a Sector. After 2 turns, Workers enter exhaustion for 4 turns, limiting a Sector to 1 Worker per color.

2. **ACTION PHASE:** Players are able to activate their cards by using the required number of Workers as indicated by each card.

3. **END TURN PHASE:** Players must discard their cards until they have 5 cards left in their hand. During this phase, players can donate any remaining Workers to their allies.


### White Cards:
These cards are split into two decks, a Positive Deck and a Negative Deck. Every third turn a card is drawn starting from the Negative Deck. Once 2 cards are drawn from the Negative Deck a card is drawn from the Positive Deck, this pattern continues until there are no more cards in the Positive Deck.


### Red Players:
#### Colorless Worker:
Red Players have 1 Colorless Worker which can be used in place of any other regular Worker. 


#### Backdoor: 
An effect placed on Blue Facilities that is required for Red Players to play stronger cards. This effect remains for 3 turns or until it is removed.


### Blue Players:
#### Fortified:
An effect placed on Blue Facilities that prevents any Backdoors from being placed on a Facility while active and removes any Backdoors already on a Facility. This effect remains for 3 turns or until it is removed.


## Ending the Game: 
**The game ends when one of the following conditions are met** 

#### Running Out of Cards: 
When there are no more cards in a player’s deck then the game is over and the opposing team wins. 


#### Running Out of Turns:
When the game has reached the end of turn 30, the game ends and Blue Players win.


#### Facilities Down: 
If all Facilities in every Sector are down the Red Players win. 


#### Doom Clock:
If half of all Sectors are down or a Core Sector (Energy, Communication, Water, Information Technology) has 2 or more of their Facilities down at the same time the Blue Players have only 3 turns to have more than half of all Sectors online or to restore the Core Sector. If Blue Players manages this then the Doom Clock resets to 3 turns. If 3 turns have passed the Red Players win.

## Credits

A large team created this work and related products: 
- Students (Liam Andres, Diego Barilla, Sam Beckman, Lizhao Cao, Jye Crocker, Michael Eaton, Elad Flaison, Ben Garvey, Emmett McEvoy, Kevin LaPorte, Mukund Suresh, Emily Nack, Henry Orsagh, Dariel Ravelo-Ramos, Lee Smith, Heena Thadani, Huadong Zhang, James Zilberman)
- Faculty (Jessica Bayliss, Chao Peng, David I. Schwartz, Brian Tomaszewski)
- Research scientist (Chris Schwartz)
- DoD representatives (David Abitbol, Karen Guttieri, Mark McElwain, Steve Whitham, Chris Wilkinson).

**Rochester Institute of Technology produced this game and its data under United States Military Academy (USMA) Award Number W911NF-23-2-0036. USMA, as the Federal awarding agency, reserves a royalty-free, nonexclusive, and irrevocable right to reproduce, publish, or otherwise use this data for Federal purposes, and to authorize others to do so in accordance with 2 CFR 200.315(b).**
