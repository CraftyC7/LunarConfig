using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace LunarConfig.Objects
{
    internal class MapObjectInfo
    {
        public string objID {  get; set; }
        public bool faceAwayWall {  get; set; }
        public bool faceWall { get; set; }
        public bool disallowNearEntrance { get; set; }
        public bool requireDistanceBetweenSpawns { get; set; }
        public bool spawnFlushAgainstWall { get; set; }
        public bool spawnAgainstWall { get; set; }
        public AnimationCurve baseCurve { get; set; }

        public MapObjectInfo(SpawnableMapObject obj) 
        {
            objID = obj.prefabToSpawn.name;
            faceAwayWall = obj.spawnFacingAwayFromWall;
            faceWall = obj.spawnFacingWall;
            disallowNearEntrance = obj.disallowSpawningNearEntrances;
            requireDistanceBetweenSpawns = obj.requireDistanceBetweenSpawns;
            spawnFlushAgainstWall = obj.spawnWithBackFlushAgainstWall;
            spawnAgainstWall = obj.spawnWithBackToWall;
            baseCurve = new AnimationCurve();
        }

        public MapObjectInfo(string objID, bool faceAwayWall, bool faceWall, bool disallowNearEntrance, bool requireDistanceBetweenSpawns, bool spawnFlushAgainstWall, bool spawnAgainstWall, AnimationCurve baseCurve)
        {
            this.objID = objID;
            this.faceAwayWall = faceAwayWall;
            this.faceWall = faceWall;
            this.disallowNearEntrance = disallowNearEntrance;
            this.requireDistanceBetweenSpawns = requireDistanceBetweenSpawns;
            this.spawnFlushAgainstWall = spawnFlushAgainstWall;
            this.spawnAgainstWall = spawnAgainstWall;
            this.baseCurve = baseCurve;
        }
    }
}
