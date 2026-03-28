using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sapper
{
    public class SapperRewardFlow : MonoBehaviour
    {
        [SerializeField] private GameObject _rewardModal;
        [SerializeField] private TextMeshProUGUI _rewardAmountText;
        [SerializeField] private TextMeshProUGUI _rewardTypeText;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _failureModal;
        [SerializeField] private Button _retryButton;

        // TODO: Add [SerializeField] private ParticleSystem reference here for confetti celebration effect

        public event Action OnRewardClaimed;
        public event Action OnRetryRequested;

        private void Awake()
        {
            _claimButton.onClick.AddListener(HandleClaim);
            _retryButton.onClick.AddListener(HandleRetry);
            _rewardModal.SetActive(false);
            _failureModal.SetActive(false);
        }

        public void ShowReward(StageData stage)
        {
            _rewardAmountText.text = $"x{stage.RewardAmount}";
            _rewardTypeText.text = stage.RewardType.ToString();
            _rewardModal.SetActive(true);
            // TODO: Trigger confetti particle effect here on stage complete
        }

        public void ShowFailure()
        {
            _failureModal.SetActive(true);
        }

        private void HandleClaim()
        {
            // TODO: Stop confetti particle effect here on claim
            _rewardModal.SetActive(false);
            OnRewardClaimed?.Invoke();
        }

        private void HandleRetry()
        {
            _failureModal.SetActive(false);
            OnRetryRequested?.Invoke();
        }

        // TODO: Add PlayConfetti() method here — activate and play confetti particles on stage complete
        // TODO: Add StopConfetti() method here — stop confetti emission when reward is claimed
    }
}
