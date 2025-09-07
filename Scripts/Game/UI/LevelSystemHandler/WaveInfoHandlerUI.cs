using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class WaveInfoHandlerUI : MonoBehaviour
    {
        private const string WAVE_INFO_ACTIVATE = "WainInfoActivate";

        [Header("UI")]
        [SerializeField] private GameObject _waveInfoRoot;

        [Space(5f)]
        [SerializeField] private TextMeshProUGUI _waveInfoText;
        [SerializeField] private TextMeshProUGUI _timerToNextWaveText;
        [SerializeField] private Button _skipWaveButton;

        //Dependencies
        private LevelLauncher _levelLauncher;
        private EventBus _eventBus;
        private SoundHandler _soundHandler;
        private Animator _animator;

        private void Awake()
        {
            _levelLauncher = LevelDI.Resolve<LevelLauncher>();
            _eventBus = LevelDI.Resolve<EventBus>();
            _soundHandler = LevelDI.Resolve<SoundHandler>();
            _animator = GetComponent<Animator>();
        }
        public void Start()
        {
            _eventBus.SubscribeEvent<OnLevelSystemStartedSignal>(ShowWaveInfo);
            _eventBus.SubscribeEvent<OnWaveEndedSignal>(ShowCurrentWaveInfo);
            _eventBus.SubscribeEvent<OnSkipWaveButtonShowedSignal>(ShowSkipWaveButton);
            _eventBus.SubscribeEvent<OnSkipWaveButtonHidSignal>(HideSkipWaveButton);
            _eventBus.SubscribeEvent<OnGameEndedSignal>(HideWaveInfoRoot);

            _skipWaveButton.onClick.AddListener(() => {
                _soundHandler.PlaySound(ClipName.Click);
                _eventBus.Invoke(new OnWaveSkippedSignal());
            });

            _waveInfoRoot.SetActive(false);
            _skipWaveButton.gameObject.SetActive(false);
        }

        private void Update() => 
            _timerToNextWaveText.text = _levelLauncher.GetCurrentTimeToNextWave().ToString($"0:00");

        private void ShowWaveInfo(OnLevelSystemStartedSignal signal)
        {
            _waveInfoRoot.SetActive(true);

            ChangeWaveInfoConfiguration(1); //level start with wave number 1

            _animator.SetTrigger(WAVE_INFO_ACTIVATE);
        }

        private void ShowCurrentWaveInfo(OnWaveEndedSignal signal)
        {
            ChangeWaveInfoConfiguration(signal.CurrentWaveIndex);
        }

        private void ChangeWaveInfoConfiguration(int currentWaveIndex)
        {
            _waveInfoText.text = $"Wave {currentWaveIndex}";
            _timerToNextWaveText.text = _levelLauncher.GetCurrentTimeToNextWave().ToString($"0:00");
        }

        private void ShowSkipWaveButton(OnSkipWaveButtonShowedSignal signal) => _skipWaveButton.gameObject.SetActive(true);

        private void HideSkipWaveButton(OnSkipWaveButtonHidSignal signal) => _skipWaveButton.gameObject.SetActive(false);

        private void HideWaveInfoRoot(OnGameEndedSignal signal) => _waveInfoRoot.SetActive(false);

        private void OnDestroy()
        {
            _skipWaveButton.onClick.RemoveAllListeners();

            _eventBus?.UnSubscribeEvent<OnLevelSystemStartedSignal>(ShowWaveInfo);
            _eventBus?.UnSubscribeEvent<OnWaveEndedSignal>(ShowCurrentWaveInfo);
            _eventBus?.UnSubscribeEvent<OnSkipWaveButtonShowedSignal>(ShowSkipWaveButton);
            _eventBus?.UnSubscribeEvent<OnSkipWaveButtonHidSignal>(HideSkipWaveButton);
            _eventBus?.UnSubscribeEvent<OnGameEndedSignal>(HideWaveInfoRoot);
        }
    }
}
