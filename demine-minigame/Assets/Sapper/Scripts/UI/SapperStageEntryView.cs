using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sapper
{
    public class SapperStageEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageNameText;
        [SerializeField] private Image _statusIcon;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _activeHighlight;

        [Header("Status Colors")]
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _activeColor = Color.yellow;
        [SerializeField] private Color _lockedColor = Color.gray;

        public void SetData(StageData stage, StageSaveData save, bool isActive)
        {
            _stageNameText.text = stage.DisplayName;
            _lockedOverlay.SetActive(!save.IsUnlocked);
            _activeHighlight.SetActive(isActive);

            if (save.IsCompleted)
                _stageNameText.color = _completedColor;
            else if (isActive)
                _stageNameText.color = _activeColor;
            else
                _stageNameText.color = _lockedColor;
        }
    }
}
