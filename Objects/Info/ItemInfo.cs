﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects.Info
{
    internal class ItemInfo
    {
        public string itemID { get; set; }
        public string displayName { get; set; }
        public int minValue { get; set; }
        public int maxValue { get; set; }
        public float weight { get; set; }
        public bool conductive { get; set; }
        public bool twoHanded { get; set; }
        public bool isScrap { get; set; }
        public List<string> tags { get; set; }

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
            tags = new List<string>();
        }

        public ItemInfo(string itemID, string displayName, int minValue, int maxValue, float weight, bool conductive, bool twoHanded, bool isScrap, List<string> tags)
        {
            this.itemID = itemID;
            this.displayName = displayName;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.weight = weight;
            this.conductive = conductive;
            this.twoHanded = twoHanded;
            this.isScrap = isScrap;
            this.tags = tags;
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
