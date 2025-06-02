using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;
using UnityEngine;

namespace LunarConfig.Objects
{
    internal class OutsideMapObjectInfo
    {
        public string objID {  get; set; }
        public int objWidth { get; set; }
        public bool faceAwayWall {  get; set; }
        public AnimationCurve baseCurve { get; set; }

        public OutsideMapObjectInfo(SpawnableOutsideObjectWithRarity obj) 
        {
            SpawnableOutsideObject _obj = obj.spawnableObject;
            objID = _obj.name;
            objWidth = _obj.objectWidth;
            faceAwayWall = _obj.spawnFacingAwayFromWall;
            baseCurve = new AnimationCurve();
        }

        public OutsideMapObjectInfo(string objID, int objWidth, bool faceAwayWall, AnimationCurve baseCurve)
        {
            this.objID = objID;
            this.objWidth = objWidth;
            this.faceAwayWall = faceAwayWall;
            this.baseCurve = baseCurve;
        }
    }
}
