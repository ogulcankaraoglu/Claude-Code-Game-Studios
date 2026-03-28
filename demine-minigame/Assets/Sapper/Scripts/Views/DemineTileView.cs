using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sapper
{
    public class DemineTileView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("State Colors")]
        [SerializeField] private Color _hiddenColor = Color.white;
        [SerializeField] private Color _mineFoundColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _missColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color _emptyColor = new Color(0.7f, 0.7f, 0.7f);

        [Header("Icons")]
        [SerializeField] private Sprite _mineSprite;
        [SerializeField] private Sprite _missSprite;

        [Header("Animation")]
        [SerializeField] private Animator _animator;

        // TODO: Add [SerializeField] private ParticleSystem reference here for mine reveal burst effect

        private static readonly int AnimPress   = Animator.StringToHash("Press");
        private static readonly int AnimSuccess = Animator.StringToHash("Success");
        private static readonly int AnimMiss    = Animator.StringToHash("Miss");

        public int TileIndex { get; private set; }
        public TileState CurrentState { get; private set; }
        public event Action<int> OnTapped;

        public void Initialize(int index)
        {
            TileIndex = index;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleButtonClicked);
            SetState(TileState.Hidden);
        }

        private void HandleButtonClicked()
        {
            PlayPress();
            OnTapped?.Invoke(TileIndex);
        }

        public void SetState(TileState state)
        {
            CurrentState = state;
            _button.interactable = state == TileState.Hidden;

            switch (state)
            {
                case TileState.Hidden:
                    _background.color = _hiddenColor;
                    SetIcon(null);
                    SetLabel("");
                    break;

                case TileState.MineFound:
                    _background.color = _mineFoundColor;
                    SetIcon(_mineSprite);
                    SetLabel("");
                    TriggerSuccess();
                    // TODO: Trigger mine reveal burst particle effect here
                    break;

                case TileState.Miss:
                    _background.color = _missColor;
                    SetIcon(_missSprite);
                    SetLabel("X");
                    TriggerMiss();
                    break;

                case TileState.EmptyRevealed:
                    _background.color = _emptyColor;
                    SetIcon(null);
                    SetLabel("");
                    break;
            }
        }

        public void SetInteractable(bool interactable) => _button.interactable = interactable;

        // ── Animation helpers ────────────────────────────────────────────────

        private void PlayPress()
        {
            if (_animator == null) return;
            _animator.SetTrigger(AnimPress);
        }

        private void TriggerSuccess()
        {
            if (_animator == null) return;
            _animator.SetTrigger(AnimSuccess);
        }

        private void TriggerMiss()
        {
            if (_animator == null) return;
            _animator.SetTrigger(AnimMiss);
        }

        // TODO: Add PlayMineBurst() method here — detach particle from canvas, map screen→world position, then play burst

        // ── Icon / label helpers ─────────────────────────────────────────────

        private void SetIcon(Sprite sprite)
        {
            if (_icon == null) return;
            _icon.sprite = sprite;
            _icon.gameObject.SetActive(sprite != null);
        }

        private void SetLabel(string text)
        {
            if (_label == null) return;
            _label.text = text;
        }
    }
}
