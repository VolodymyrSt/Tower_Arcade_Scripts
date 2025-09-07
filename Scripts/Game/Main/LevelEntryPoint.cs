using DI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class LevelEntryPoint : MonoBehaviour
    {
        private DIContainer _levelContainer = new();
        private DIContainer _rootContainer;

        [Header("Enemy")]
        [SerializeField] private Transform _enemyTargetDestination;
        [SerializeField] private Transform _enemyStartPosition;
        [Space(10f)]
        [SerializeField] private EnemyDescriptionCardUI _enemyCardHandlerUI;

        [Header("Tower")]
        [SerializeField] private TowerDescriptionCardUI _towerDescriptionCardHandlerUI;

        [Header("TowerPlacement")]
        [SerializeField] private TowerPlacementBlocksHolder _towerPlacementBlocksHolder;
        [SerializeField] private GameInventoryHandler _gameInventoryHandler;

        [Header("LevelConfiguration")]
        [SerializeField] private LevelSystemSO _levelSystemConfig;
        [SerializeField] private LevelConfigurationSO _levelConfiguration;

        [Header("Effects")]
        [SerializeField] private EffectPerformer _effectPerformer;

        [Header("UI")]
        [SerializeField] private HealthBarHandlerUI _healthBarHandler;

        [Header("Setting")]
        [SerializeField] private LevelSettingHandlerUI _levelSettingHandlerUI;
        
        [Header("Camera")]
        [SerializeField] private CameraMoveController _cameraMoveController;
        
        [Header("Massege")]
        [SerializeField] private MassegeHandlerUI _massegeHandler;

        private readonly List<IUpdatable> _updatable = new();
        private EventBus _eventBus;
        private CoroutineUsager _coroutineUsager;
        private SaveData _saveData;
        private GameInput _gameInput;
        private LevelLauncher _levelLauncher;

        private void Awake()
        {
            _rootContainer = FindFirstObjectByType<GameEntryPoint>().GetRootContainer();

            _levelContainer = new DIContainer(_rootContainer);
            _gameInput = new GameInput();
            _eventBus = new EventBus();

            _coroutineUsager = _levelContainer.Resolve<CoroutineUsager>();
            _saveData = _levelContainer.Resolve<SaveData>();

            _levelContainer.RegisterInstance<EventBus>(_eventBus);
            _levelContainer.RegisterInstance<GameInput>(_gameInput);
            _levelContainer.RegisterInstance<LevelSystemSO>(_levelSystemConfig);
            _levelContainer.RegisterInstance<EnemyDescriptionCardUI>(_enemyCardHandlerUI);
            _levelContainer.RegisterInstance<TowerDescriptionCardUI>(_towerDescriptionCardHandlerUI);
            _levelContainer.RegisterInstance<TowerPlacementBlocksHolder>(_towerPlacementBlocksHolder);
            _levelContainer.RegisterInstance<LevelConfigurationSO>(_levelConfiguration);
            _levelContainer.RegisterInstance<EffectPerformer>(_effectPerformer);
            _levelContainer.RegisterInstance<HealthBarHandlerUI>(_healthBarHandler);
            _levelContainer.RegisterInstance<LevelSettingHandlerUI>(_levelSettingHandlerUI);
            _levelContainer.RegisterInstance<CameraMoveController>(_cameraMoveController);
            _levelContainer.RegisterInstance<MassegeHandlerUI>(_massegeHandler);
            _levelContainer.RegisterInstance<GameInventoryHandler>(_gameInventoryHandler);

            LevelDI.Register(_levelContainer);

            _gameInventoryHandler.InitializeInventorySlots(_levelContainer, _saveData.TowerGenerals);

            _levelLauncher = new LevelLauncher(_levelContainer, _levelSystemConfig,
                _coroutineUsager, _eventBus, _enemyStartPosition, _enemyTargetDestination.position,
                _healthBarHandler, _levelContainer.Resolve<LevelCurencyHandler>(),
                _levelContainer.Resolve<EnemyFactoryHandler>());

            _levelContainer.RegisterInstance<LevelLauncher>(_levelLauncher);

            AddUpdatables();
        }

        private void Start() => 
            _eventBus.SubscribeEvent<OnLevelSystemStartedSignal>(_levelLauncher.LaunchLevel);

        private void Update()
        {
            foreach(var updatable in _updatable)
                updatable.Tick();
        }

        private void AddUpdatables()
        {
            _updatable.Clear();

            _updatable.Add(_levelContainer.Resolve<EnemyDescriptionCardHandler>());
            _updatable.Add(_levelContainer.Resolve<TowerDescriptionCardHandler>());
            _updatable.Add(_levelContainer.Resolve<LevelSettingHandler>());
            _updatable.Add(_levelLauncher);
            _updatable.Add(_levelContainer.Resolve<PlacementBlockColorHandler>());
        }

        private void OnDestroy()
        {
            _levelContainer.Dispose();

            _rootContainer.UnRegister(_rootContainer.Resolve<CoinBalanceUI>());
            _rootContainer.UnRegister(_rootContainer.Resolve<LocationHandler>());
            _rootContainer.UnRegister(_rootContainer.Resolve<LevelEntranceController>());

            _eventBus?.UnSubscribeEvent<OnLevelSystemStartedSignal>(_levelLauncher.LaunchLevel);

            _gameInput.Dispose();
        }
    }
}
