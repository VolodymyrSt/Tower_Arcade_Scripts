using DG.Tweening;
using DI;
using Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Values")]
        [SerializeField] private float _scaleValue = 1.1f;
        [SerializeField] private float _normalValue = 1f;
        [SerializeField] private float _duration = 0.3f;

        [Header("Ease")]
        [SerializeField] private Ease _ease;

        private SoundHandler _soundHandler;

        private void Awake()
        {
            try
            {
                _soundHandler = MenuDI.Resolve<SoundHandler>();

                if (_soundHandler == null)
                    _soundHandler = LevelDI.Resolve<SoundHandler>();
            }
            catch
            {

            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(_scaleValue, _duration)
                .SetEase(_ease)
                .Play()
                .OnComplete(() =>{
                    _soundHandler.PlaySound(ClipName.Selected);
                });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(_normalValue, _duration)
                .SetEase(_ease)
                .Play();
        }
    }
}
