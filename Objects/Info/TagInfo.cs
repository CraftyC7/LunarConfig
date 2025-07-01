using DunGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects.Info
{
    internal class TagInfo
    {
        public string tagID { get; set; }
        public float mapObjectPeakMultiplier { get; set; }
        //public Dictionary<string, float> mapObjectMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> itemPoolMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> interiorEnemyPoolMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> exteriorEnemyPoolMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> daytimeEnemyPoolMultipliers = new Dictionary<string, float>();
        public Dictionary<string, float> dungeonMultipliers = new Dictionary<string, float>();

        public TagInfo(string tagID, float mapObjectPeakMultiplier, /*Dictionary<string, float> mapObjectMultipliers,*/ Dictionary<string, float> itemPoolMultipliers, Dictionary<string, float> interiorEnemyPoolMultipliers, Dictionary<string, float> exteriorEnemyPoolMultipliers, Dictionary<string, float> daytimeEnemyPoolMultipliers, Dictionary<string, float> dungeonMultipliers)
        {
            this.tagID = tagID;
            this.mapObjectPeakMultiplier = mapObjectPeakMultiplier;
            //this.mapObjectMultipliers = mapObjectMultipliers;
            this.itemPoolMultipliers = itemPoolMultipliers;
            this.interiorEnemyPoolMultipliers = interiorEnemyPoolMultipliers;
            this.exteriorEnemyPoolMultipliers = exteriorEnemyPoolMultipliers;
            this.daytimeEnemyPoolMultipliers = daytimeEnemyPoolMultipliers;
            this.dungeonMultipliers = dungeonMultipliers;
        }
    }
}
