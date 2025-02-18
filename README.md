Game was planned out by my brother Owais Akhtar, programming done by me.

Note: This is an early prototype. All art is placeholder art, and this was uploaded for playtesting purposes. However, I wanted to make it public since I got the main gameplay to work.

In Last Man Standing (LMS), players challenge their memory and try to outlive their friends in a back-and-forth game of survival and escape.  You can read more about the rules of the game here: LMS - Rulebook Prototype 4

Download [here](https://sunstation.itch.io/last-man-standing)

This is the first Unity project where I implemented Unity's Netcode solution to add multiplayer to the game

Updates

The main gameplay and behavior is finished. Players can create or join a lobby, ready up, read a tutorial (which I still need to add), and play the game from start to finish.  Players can then either return to the main menu, or ready up and start a new game in the same lobby.

Issues

While most of the behavior works as intended, some interactions cause weird things to happen. Examples include:

 Activating the troll card effect but not picking any card lets you use the effect of the troll card on another turn
Some card effects require you to choose another player to use the effect on. However, if the last two cards are these types of cards, strange things happen
I haven't tested this, but I think you will still be able to target dead players with card effects
I will try to fix these issues, and other issues that will come up (I don't even know if this will work properly when tested, I just know it works locally), as well as polish the game by adding better visuals and maybe some sound effects, maybe even small animations.

Overall, this was a cool project that caused many headaches since I wasn't familiar with coding in a way where Server and Clients can talk to each other, so I got bugs related to syncing issues that I'd never dealt with before. Most of the stuff I learned about Unity Netcode was through Code Monkey's Kitchen Nightmare Multiplayer Tutorial. It felt really nice to be able to apply skills I learned and put my own spin on them (the tutorial didn't really go over many player-to-player interaction behaviors).

Updates

Planning on adding a video of gameplay soon
2/23/2024 - Started the player at 4 lives now instead of one
2/24/2024 - Fixed some bugs that happened after trying to start a new game in the same lobby (PlayerData not resetting properly, null errors because of events that weren't unsubscribed from, etc)
Credits

Design and Art - Owais Akhtar
Programming - Danyal Akhtar (myself)


