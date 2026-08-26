# Changes Logs v4.0.0
Note: The main feature of this update is the new **Custom** ai system, that uses a **Pathfinding** system to get to the players more reliably.

## Added
- Added a new **Pathfinding** AI, so expect better **AI** behaviour.
## Changes
- Rework the "Bot Menu" to be more user friendly.
- Refactor and rework the "Picker AI" system.
- Bots no longer block **LITERALLY EVERYTHING**.
## Fixes
- Fixed leaving while in a bot pick phase break the picker system.
- Fixed the bots picker system trigger multiple times.
- Fixed bots only matches auto rematch sometime break.

# Changes Logs v3.1.0
## Added
- Added the `Bot Menu`, which appears when clicking the `TO EDIT` text on bots.
## Changes
- Converted the project into a Unity project.

# Changes Logs v3.0.1
## Fixes
- Fixed the **Stalemate** config slider to support non-whole numbers.

# Changes Logs v3.0.0
## Added
- Added the **Stalemate Handler**. If a player remains dead for a set amount of time, the stalemate handler gradually kills the bots to end the round.
- Added **Auto-Rematch** for bot-only games. A rematch is automatically triggered at the end of each match when only bots are in the game.
## Changes
- Prevented bots from spawning while a battle is in progress. Minions can still spawn during battles.
- Improved the **Menu** for better usability.
- Improved **Bot AI** behavior for more dynamic and challenging interactions.

# Changes Logs v2.3.0
## Changes
- Switched the bot AI from the `PlayerAI` component to the `PlayerAIPhilip` component for improved functionality and performance.