using DI;
using System.Collections;
using UnityEngine;

namespace Game
{
    public class LevelLauncher : IUpdatable
    {
        private readonly DIContainer _container;
        private readonly LevelSystemSO _levelConfig;
        private readonly EventBus _eventBus;
        private readonly CoroutineUsager _coroutineUsager;
        private readonly HealthBarHandlerUI _healthBarHandler;
        private readonly LevelCurencyHandler _levelCurencyHandler;
        private readonly EnemyFactoryHandler _enemyFactoryHandler;

        private readonly Transform _enemyContainer;
        private  Vector3 _enemyDestinationPoint;
        private float _currentTimeToNextWave;

        private bool _isSkipButtonPressed = false;
        private bool _isGameEnded = false;
        private bool _isLastWave = false;

        public LevelLauncher(DIContainer container, LevelSystemSO levelSystem, CoroutineUsager coroutineUsager,
            EventBus eventBus, Transform enemyContainer, Vector3 enemyDestinationPoint,
            HealthBarHandlerUI healthBarHandler, LevelCurencyHandler levelCurencyHandler, EnemyFactoryHandler enemyFactoryHandler)
        {
            _container = container;
            _levelConfig = levelSystem;
            _coroutineUsager = coroutineUsager;
            _eventBus = eventBus;
            _enemyContainer = enemyContainer;
            _enemyDestinationPoint = enemyDestinationPoint;
            _healthBarHandler = healthBarHandler;
            _levelCurencyHandler = levelCurencyHandler;
            _enemyFactoryHandler = enemyFactoryHandler;
        }

        public void LaunchLevel(OnLevelSystemStartedSignal signal) => 
            _coroutineUsager.StartCoroutine(StartLevelSystem(signal));

        private IEnumerator StartLevelSystem(OnLevelSystemStartedSignal signal)
        {
            _eventBus.SubscribeEvent<OnWaveSkippedSignal>(SkipWave);
            _eventBus.SubscribeEvent<OnGameEndedSignal>(StopLevelSystem);

            _currentTimeToNextWave = _levelConfig.MaxTimeToNextWave;

            for (int i = 0; i < _levelConfig.Waves.Count; i++)
            {
                if (_isGameEnded) break;
                var wave = _levelConfig.Waves[i];

                ////

                yield return new WaitForSeconds(wave.TimeBetweenEnemySpawn);

                for (int k = 0; k < wave.EnemyConfiguration.Count; k++)
                {
                    if (_isGameEnded) break;

                    for (int j = 0; j < wave.EnemyConfiguration[k].NumberOfEnemies; j++)
                    {
                        if (_isGameEnded) break;

                        _enemyFactoryHandler.GetEnemyFactoryByType(_container, wave.EnemyConfiguration[k].FactoryType)
                            .SpawnEnemy(_enemyContainer, _enemyDestinationPoint);

                        yield return new WaitForSeconds(wave.TimeBetweenEnemySpawn);
                    }
                }

                ////

                if (i == _levelConfig.Waves.Count - 1) break;

                _eventBus.Invoke(new OnSkipWaveButtonShowedSignal());

                while (_currentTimeToNextWave > 0 && !_isSkipButtonPressed)
                {
                    _currentTimeToNextWave -= Time.deltaTime;
                    yield return null;
                }

                _levelCurencyHandler.AddCurrencyCount(wave.AmountOfSoulsAfterFinish);

                _eventBus.Invoke(new OnSkipWaveButtonHidSignal());
                _eventBus.Invoke(new OnWaveEndedSignal(i + 2)); //to end up with wave 2

                _currentTimeToNextWave = _levelConfig.MaxTimeToNextWave;
                _isSkipButtonPressed = false;
            }

            _isLastWave = true;
        }

        public void Tick()
        {
            if (_isLastWave)
            {
                if (_enemyContainer.childCount == 0 && _healthBarHandler.GetCurrentHealth() > 0)
                {
                    _eventBus.Invoke<OnGameWonSignal>(new OnGameWonSignal());
                    _isLastWave = false;
                }
            }
        }

        public float GetCurrentTimeToNextWave() => _currentTimeToNextWave;
        private void SkipWave(OnWaveSkippedSignal signal) => _isSkipButtonPressed = true;
        private void StopLevelSystem(OnGameEndedSignal signal)
        {
            _isGameEnded = true;

            _eventBus?.UnSubscribeEvent<OnWaveSkippedSignal>(SkipWave);
            _eventBus?.UnSubscribeEvent<OnGameEndedSignal>(StopLevelSystem);

            ClearAllEnemies();
        }

        private void ClearAllEnemies()
        {
            foreach (Transform child in _enemyContainer.transform)
            {
                if (child.TryGetComponent(out Enemy enemy))
                    enemy.DestroySelf();
            }
        }
    }
}
