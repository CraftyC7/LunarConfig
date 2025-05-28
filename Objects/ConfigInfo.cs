using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunarConfig.Objects
{
    internal class ConfigInfo
    {
        public List<ItemInfo> allItemInfo { get; } = new List<ItemInfo>();
        public List<EnemyInfo> allEnemyInfo { get; } = new List<EnemyInfo>();

        public void addItem(Item item)
        {
            allItemInfo.Add(new ItemInfo(item));
        }

        public void addEnemy(EnemyType enemy)
        {
            allEnemyInfo.Add(new EnemyInfo(enemy));
        }

        public void writeItems(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var info in allItemInfo)
                {
                    writer.WriteLine($"{info.getName()} - Min: {info.minValue}, Max: {info.maxValue}, Conductive: {info.conductive}, Scrap: {info.isScrap}, Two-Handed: {info.twoHanded}, Weight: {info.weight}");
                    MiniLogger.LogInfo($"Recorded {info.getName()}");
                }
            }
        }

        public ConfigInfo()
        {
            
        }
    }
}
