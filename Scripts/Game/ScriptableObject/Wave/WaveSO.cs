using DI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Scriptable Object/WaveConfigs")]
    public class WaveSO : ScriptableObject
    {
        public List<EnemyData> EnemyConfiguration = new();

        public float TimeBetweenEnemySpawn;
        public int AmountOfSoulsAfterFinish;

    }

    [Serializable]
    public struct EnemyData { 
        public int NumberOfEnemies;
        public EnemyFactoryType FactoryType;
    }
}
