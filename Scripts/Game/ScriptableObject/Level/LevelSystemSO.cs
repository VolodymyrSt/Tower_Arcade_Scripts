using DI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "LevelSystem", menuName = "Scriptable Object/LevelSystemConfig")]
    public class LevelSystemSO : ScriptableObject
    {
        public List<WaveSO> Waves = new();
        public float MaxTimeToNextWave;

        public int GetWavesCount() => Waves.Count;
    }
}
