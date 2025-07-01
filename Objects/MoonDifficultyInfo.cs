using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace LunarConfig.Objects
{
    internal class MoonDifficultyInfo
    {
        public float heat = -1;
        public List<(MoonEvent, float)> events = new List<(MoonEvent, float)> ();

        public MoonDifficultyInfo() 
        {
            
        }

        public void IncrementHeat()
        {
            heat += 1f;
        }

        public void DecrementHeat()
        {
            if (heat >= 0.5f)
            {
                heat -= 0.5f;
            }
        }

        public void RandomizeEvents()
        {

        }
    }
}
