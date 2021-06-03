# Chat-Overrides

This plugin allows for basic overriding of chat messages via a configuration file. It uses format styles like `{0}, {1}, {2}` etc. to provide additional information about the occuring event as well.

## Configuration 🧪
### Chat Formats

This plugin currently supports 7 different message overrides with the following formats:

`BossSpawnedMessage`:
  * {0}: NPC Name
  * {1}: Either `has` or `have` based on whether the npc name is singular or plural
  * {2}: The NPC's defense
  
`BossDefeatedMessage`:
  * {0}: NPC Name
  * {1}: Either `has` or `have` based on whether the npc name is singular or plural
  * {2}: The NPC's defense
  
`PlayerJoinTeamMessage`:
  * {0}: Player Name
  * {1}: Team Name
  * {2}: Team color
  
`DeathMessageFromPvP`:
  * {0}: Killed players name
  * {1}: Killers Name
  * {2}: A randomized death description (e.g, `was cut down the middle by`)
  * {3}: The name of the item that killed the player
  * {4}: The hex code of the items rarity
  * {5}: The items prefix
  * {6}: The hex code of the killed players group
  * {7}: The hex code of the killers group
  * {8}: The current life of the killer
  * {9}: The current mana of the killer
  * {10}: The current killers defense
  
`DeathMessageFromNPC`:
  * {0}: NPCs name
  * {1}: NPCs life
  * {2}: NPCs max life
  * {3}: NPCs net id
  * {4}: NPCs defense
  
  `NPCArriveMessage` and `JungleGrowsRestless` do not support formats
