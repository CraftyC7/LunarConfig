using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects
{
    internal class DungeonInfo
    {
        public string dungeonID {  get; set; }
        public List<String> tags { get; set; }

        public DungeonInfo(DungeonFlow dungeon) 
        {
            dungeonID = dungeon.name;
            tags = new List<String>();
        }

        public DungeonInfo(string dungeonID, List<String> tags)
        {
            this.dungeonID = dungeonID;
            this.tags = tags;
        }
    }
}
