using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects
{
    internal class ItemInfo
    {
        public string itemID {  get; set; }
        public string displayName { get; set; }
        public int minValue { get; set; }
        public int maxValue { get; set; }
        public float weight { get; set; }
        public bool conductive { get; set; }
        public bool twoHanded { get; set; }
        public bool isScrap { get; set; }

        public ItemInfo(Item item) 
        {
            itemID = item.name;
            displayName = item.itemName;
            minValue = item.minValue;
            maxValue = item.maxValue;
            weight = item.weight;
            conductive = item.isConductiveMetal;
            twoHanded = item.twoHanded;
            isScrap = item.isScrap;
        }

        public string getName()
        {
            if (displayName != itemID)
            {
                return $"{displayName} ({itemID})";
            }
            return displayName;
        }
        
        public float getAverageValue()
        {
            return (minValue + maxValue) / 2;
        }
    }
}
