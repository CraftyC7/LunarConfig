using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LunarConfig.Objects
{
    internal class SpawnableObjectKeeper
    {
        public Dictionary<string, Item> items = new Dictionary<string, Item>();
        public Dictionary<string, EnemyType> enemies = new Dictionary<string, EnemyType>();
        public Dictionary<string, SpawnableMapObject> mapObjects = new Dictionary<string, SpawnableMapObject>();

        public SpawnableObjectKeeper(NetworkManager manager) 
        {
            HashSet<string> registeredItems = new HashSet<string>();
            HashSet<string> registeredEnemies = new HashSet<string>();
            HashSet<string> registeredMapObjects = new HashSet<string>();

            foreach (var obj in Resources.FindObjectsOfTypeAll<Item>())
            {
                if (obj.spawnPrefab == null)
                {
                    //Item is missing prefab!
                }
                else if (!manager.NetworkConfig.Prefabs.Contains(obj.spawnPrefab))
                {
                    //Item not a real item!
                }
                else if (registeredItems.Contains(obj.name))
                {
                    //Item already registered!
                }
                else
                {
                    items.Add(obj.name, obj);
                    registeredItems.Add(obj.name);
                }
            }

            foreach (var obj in Resources.FindObjectsOfTypeAll<EnemyType>())
            {
                if (obj.enemyPrefab == null)
                {
                    //Enemy is missing prefab!
                }
                else if (!manager.NetworkConfig.Prefabs.Contains(obj.enemyPrefab))
                {
                    //Enemy not a real enemy!
                }
                else if (registeredEnemies.Contains(obj.name))
                {
                    //Enemy already registered!
                }
                else
                {
                    enemies.Add(obj.name, obj);
                    registeredEnemies.Add(obj.name);
                }
            }

            foreach (var level in Resources.FindObjectsOfTypeAll<SelectableLevel>())
            {
                foreach (var obj in level.spawnableMapObjects)
                {
                    if (obj.prefabToSpawn == null)
                    {
                        //Map Object is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(obj.prefabToSpawn))
                    {
                        //Map Object not a real map object!
                    }
                    else if (registeredMapObjects.Contains(obj.prefabToSpawn.name))
                    {
                        //Map Object already registered!
                    }
                    else
                    {
                        mapObjects.Add(obj.prefabToSpawn.name, obj);
                        registeredMapObjects.Add(obj.prefabToSpawn.name);
                    }
                }
            }
        }
    }
}
