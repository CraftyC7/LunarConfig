using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LunarConfig.Objects.Info
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
        public bool useFalloff { get; set; }
        public AnimationCurve falloffCurve { get; set; }
        public int enemyHP { get; set; }
        public bool canDie { get; set; }
        public bool destroyOnDeath { get; set; }
        public bool canDestroy { get; set; }
        public bool canStun { get; set; }
        public float stunDifficulty { get; set; }
        public float stunTime { get; set; }
        public List<string> tags { get; set; }
        public List<string> blacklistTags { get; set; }

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
            useFalloff = enemy.useNumberSpawnedFalloff;
            falloffCurve = enemy.numberSpawnedFalloff;
            EnemyAI ai = enemy.enemyPrefab.GetComponent<EnemyAI>();
            enemyHP = ai.enemyHP;
            canDie = enemy.canDie;
            destroyOnDeath = enemy.destroyOnDeath;
            canDestroy = enemy.canBeDestroyed;
            canStun = enemy.canBeStunned;
            stunDifficulty = enemy.stunGameDifficultyMultiplier;
            stunTime = enemy.stunTimeMultiplier;
            tags = new List<string>();
            blacklistTags = new List<string>();
        }

        public EnemyInfo(string enemyID, string displayName, bool canSeeThroughFog, float doorSpeedMultiplier, bool isDaytimeEnemy, bool isOutsideEnemy, float loudnessMultiplier, int maxCount, float powerLevel, AnimationCurve probabilityCurve, bool useFalloff, AnimationCurve falloffCurve, int enemyHP, bool canDie, bool destroyOnDeath, bool canDestroy, bool canStun, float stunDifficulty, float stunTime, List<string> tags, List<string> blacklistTags)
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
            this.useFalloff = useFalloff;
            this.falloffCurve = falloffCurve;
            this.enemyHP = enemyHP;
            this.canDie = canDie;
            this.destroyOnDeath = destroyOnDeath;
            this.canDestroy = canDestroy;
            this.canStun = canStun;
            this.stunDifficulty = stunDifficulty;
            this.stunTime = stunTime;
            this.tags = tags;
            this.blacklistTags = blacklistTags;
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
