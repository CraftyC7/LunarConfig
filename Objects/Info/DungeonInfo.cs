using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunarConfig.Objects.Info
{
    internal class DungeonInfo
    {
        public string dungeonID { get; set; }
        public List<string> tags { get; set; }
        public Dictionary<string, float> mapObjectMultipliers = new Dictionary<string, float>();

        public DungeonInfo(DungeonFlow dungeon, HashSet<string> mapObjects)
        {
            dungeonID = dungeon.name;
            tags = new List<string>();
            mapObjectMultipliers = mapObjects.ToDictionary(k => k, v => 1f);
        }

        public DungeonInfo(string dungeonID, List<string> tags, Dictionary<string, float> mapObjectMultipliers)
        {
            this.dungeonID = dungeonID;
            this.tags = tags;
            this.mapObjectMultipliers = mapObjectMultipliers;
        }
    }
}
