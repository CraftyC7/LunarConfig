using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SearchService;

namespace LunarConfig.Objects
{
    internal class MoonInfo
    {
        public string moonID {  get; set; }
        public string displayName { get; set; }
        public string risk {  get; set; }
        public string description { get; set; }
        public bool hasTime { get; set; }
        public float timeMultiplier { get; set; }
        public float daytimeProbabilityRange { get; set; }
        public AnimationCurve daytimeCurve { get; set; }
        public int maxDaytimePower { get; set; }
        public float interiorProbabilityRange { get; set; }
        public AnimationCurve interiorCurve { get; set; }
        public int maxInteriorPower { get; set; }
        public AnimationCurve outsideCurve { get; set; }
        public int maxOutsidePower { get; set; }
        public int minScrap { get; set; }
        public int maxScrap { get; set; }
        public float valueMultiplier { get; set; }
        public float amountMultiplier { get; set; }
        public float interiorSizeMultiplier { get; set; }
        public List<String> tags { get; set; }

        public MoonInfo(SelectableLevel level) 
        {
            moonID = level.name;
            displayName = level.PlanetName;
            risk = level.riskLevel;
            description = level.LevelDescription;

            hasTime = level.planetHasTime;
            timeMultiplier = level.DaySpeedMultiplier;

            daytimeProbabilityRange = level.daytimeEnemiesProbabilityRange;
            daytimeCurve = level.daytimeEnemySpawnChanceThroughDay;
            maxDaytimePower = level.maxDaytimeEnemyPowerCount;

            interiorProbabilityRange = level.spawnProbabilityRange;
            interiorCurve = level.enemySpawnChanceThroughoutDay;
            maxInteriorPower = level.maxEnemyPowerCount;

            outsideCurve = level.outsideEnemySpawnChanceThroughDay;
            maxOutsidePower = level.maxOutsideEnemyPowerCount;

            minScrap = level.minScrap;
            maxScrap = level.maxScrap;

            valueMultiplier = 0.4f;
            amountMultiplier = 1f;

            interiorSizeMultiplier = level.factorySizeMultiplier;
            
            tags = new List<String>();
        }

        public MoonInfo(string moonID, string displayName, string risk, string description, bool hasTime, float timeMultiplier, float daytimeProbabilityRange, AnimationCurve daytimeCurve, int maxDaytimePower, float interiorProbabilityRange, AnimationCurve interiorCurve, int maxInteriorPower, AnimationCurve outsideCurve, int maxOutsidePower, int minScrap, int maxScrap, float valueMultiplier, float amountMultiplier, float interiorSizeMultiplier, List<string> tags)
        {
            this.moonID = moonID;
            this.displayName = displayName;
            this.risk = risk;
            this.description = description;
            this.hasTime = hasTime;
            this.timeMultiplier = timeMultiplier;
            this.daytimeProbabilityRange = daytimeProbabilityRange;
            this.daytimeCurve = daytimeCurve;
            this.maxDaytimePower = maxDaytimePower;
            this.interiorProbabilityRange = interiorProbabilityRange;
            this.interiorCurve = interiorCurve;
            this.maxInteriorPower = maxInteriorPower;
            this.outsideCurve = outsideCurve;
            this.maxOutsidePower = maxOutsidePower;
            this.minScrap = minScrap;
            this.maxScrap = maxScrap;
            this.valueMultiplier = valueMultiplier;
            this.amountMultiplier = amountMultiplier;
            this.interiorSizeMultiplier = interiorSizeMultiplier;
            this.tags = tags;
        }

        public string getName()
        {
            if (displayName != moonID)
            {
                return $"{displayName} ({moonID})";
            }
            return displayName;
        }
        
        public float getAverageScrap()
        {
            return (minScrap + maxScrap) / 2;
        }
    }
}
