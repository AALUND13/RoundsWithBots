# RoundsWithBots Mod

### Description
Tired of waiting for friends in Rounds or don't have friends who own the game? The **RoundsWithBots** mod solves this by allowing you to play with AI bots. Simply press 'B' to activate the bot feature and 'R' to ready up, and enjoy Rounds anytime!

### Features
- Play Rounds with AI bots.
- Quick and easy activation using hotkeys.

### How to Use
1. Press 'B' to add a bot.
2. Press 'R' to ready up all bots and start playing with AI bots.

### Feedback and Issues
- If you encounter any issues or have feedback, please [submit an issue](https://github.com/AALUND13/RoundsWithBots/issues).

### Credits
- **RoundsWithBots** mod is developed by [AALUND13](https://github.com/AALUND13) and [Tess-y (Root)](https://github.com/Tess-y).

### Changelog
- **Version 3.0.1:**
	- **Fixed Stalemate Config Slider:** Fixed the stalemate config slider to be non-whole numbers.
	
- **Version 3.0.0:**
	- **Added Stalemate Handler:** If the player is dead for a set amount of time, the stalemate handler triggers, gradually killing the bots to end the round.
	- **Prevented Bot Spawning During Ongoing Battles:** Bots are now restricted from spawning while a battle is in progress (minions can still spawn during battle).
	- **Auto-Rematch for Bot-Only Games:** Automatically triggers a rematch at the end of each match when only bots are in the game.
	- **Menu Improvements:** Enhanced the menu for better usability.
	- **Bot AI Enhancements:** Improved bot behavior for more dynamic and challenging interactions.

- **Version 2.3.0:***
	- **Switched to `PlayerAIPhilip` Component:** Updated the bot AI to use the `PlayerAIPhilip` component instead of the `PlayerAI` component for improved functionality and performance.