using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects
{
    internal class DungeonInfo
    {
        public string dungeonID {  get; set; }
        public Dictionary<String, int> tags { get; set; }

        public DungeonInfo(DungeonFlow dungeon) 
        {
            dungeonID = dungeon.name;
            tags = new Dictionary<String, int>();
        }

        public DungeonInfo(string dungeonID, Dictionary<String, int> tags)
        {
            this.dungeonID = dungeonID;
            this.tags = tags;
        }
    }
}
