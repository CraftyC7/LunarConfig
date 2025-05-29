using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LunarConfig.Objects
{
    internal class EnemyInfo
    {
        public string enemyID { get; set; }
        public string displayName { get; set; }
        public bool canSeeThroughFog { get; set; }
        public float doorSpeedMultiplier { get; set; }
        public bool isDaytimeEnemy { get; set; }
        public bool isOutsideEnemy { get; set; }
        public float loudnessMultiplier { get; set; }
        public int maxCount { get; set; }
        public float powerLevel { get; set; }
        public AnimationCurve probabilityCurve { get; set; }
        public int enemyHP { get; set; }

        public EnemyInfo(EnemyType enemy) 
        {
            enemyID = enemy.name;
            displayName = enemy.enemyName;
            canSeeThroughFog = enemy.canSeeThroughFog;
            doorSpeedMultiplier = enemy.doorSpeedMultiplier;
            isDaytimeEnemy = enemy.isDaytimeEnemy;
            isOutsideEnemy = enemy.isOutsideEnemy;
            loudnessMultiplier = enemy.loudnessMultiplier;
            maxCount = enemy.MaxCount;
            powerLevel = enemy.PowerLevel;
            probabilityCurve = enemy.probabilityCurve;
            EnemyAI ai = enemy.enemyPrefab.GetComponent<EnemyAI>();
            enemyHP = ai.enemyHP;
        }

        public EnemyInfo(string enemyID, string displayName, bool canSeeThroughFog, float doorSpeedMultiplier, bool isDaytimeEnemy, bool isOutsideEnemy, float loudnessMultiplier, int maxCount, float powerLevel, AnimationCurve probabilityCurve, EnemyAI ai)
        {
            this.enemyID = enemyID;
            this.displayName = displayName;
            this.canSeeThroughFog = canSeeThroughFog;
            this.doorSpeedMultiplier = doorSpeedMultiplier;
            this.isDaytimeEnemy = isDaytimeEnemy;
            this.isOutsideEnemy = isOutsideEnemy;
            this.loudnessMultiplier = loudnessMultiplier;
            this.maxCount = maxCount;
            this.powerLevel = powerLevel;
            this.probabilityCurve = probabilityCurve;
            enemyHP = ai.enemyHP;
        }

        public string getName()
        {
            if (displayName != enemyID)
            {
                return $"{displayName} ({enemyID})";
            }
            return displayName;
        }
    }
}
