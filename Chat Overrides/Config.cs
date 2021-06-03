using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chat_Overrides
{
    public class Config
    {
        public Config Read(string path)
        {
            if (!File.Exists(path))
            {
                Config config = new Config();
                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }

        public bool SidebarEnabled = false;

        public string SidebarText = "";

        /*
         * {0} NPC Name
         * {1} Has or have depending on NPC amount
         * {2} NPC's defense
         */
        public string BossSpawnedMessage = "[c/AF4BFF:{0} {1} awoken!]";

        /*
         * {0} NPC Name
         * {1} Has or have depending on NPC amount
         * {2} NPC's defense
         */
        public string BossDefeatedMessage = "[c/AF4BFF:{0} {1} been defeated!]";

        /*
         * {0} Player Name
         * {1} Team Name
         * {2} Team color
         */
        public string PlayerJoinTeamMessage = "[c/{2}:{0} has joined the {1} party.]";

        /*
         * {0} Name of the person who died
         * {1} Name of the killer
         * {2} A random death descrption
         * {3} The name of the item that killed the player
         * {4} The hex code of the items rarity
         * {5} the item's prefix id
         * {6} Hex code of the killed players group
         * {7} Hex code of the killers group
         * {8} The current life of the killer
         * {9} The current mana of the killer
         * {10} The killer's defense
         */
        public string DeathMessageFromPvP = "[c/E11919:{0} {2} {1}'s {3}]";

        /*
         * {0} NPC's name
         * {1} NPC's life
         * {2} NPC's Max Life
         * {3} NPC's net id
         * {4} NPC's defense
         */
        public string DeathMessageFromNPC = "";

        /*
         * {0} NPC Type Name
         * {1} NPC's given name
         * {2} NPC's max life
         */
        public string NPCArriveMessage = "[c/327DFF:{1} has arrived!]";

        public string JungleGrowsRestless = "[c/35ff82:The jungle grows restless...]";
    }
}
