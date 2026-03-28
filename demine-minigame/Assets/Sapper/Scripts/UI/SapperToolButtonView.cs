using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sapper
{
    public class SapperToolButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _selectedHighlight;

        public event Action OnClicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void SetData(ToolData tool, bool canAfford)
        {
            _nameText.text = tool.DisplayName;
            _costText.text = tool.Cost.ToString();
            _button.interactable = canAfford;
        }

        public void SetSelected(bool selected)
        {
            if (_selectedHighlight != null)
                _selectedHighlight.gameObject.SetActive(selected);
        }
    }
}
