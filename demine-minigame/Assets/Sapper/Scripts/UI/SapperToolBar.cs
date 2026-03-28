using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sapper
{
    public class SapperToolBar : MonoBehaviour
    {
        [SerializeField] private SapperToolButtonView _explosiveButton;
        [SerializeField] private SapperToolButtonView _detectorButton;

        public event Action<ToolType> OnToolSelected;

        private SapperToolButtonView _selectedButton;

        private void Awake()
        {
            _explosiveButton.OnClicked += () => HandleToolClicked(ToolType.Explosive, _explosiveButton);
            _detectorButton.OnClicked += () => HandleToolClicked(ToolType.Detector, _detectorButton);
        }

        public void Refresh(List<ToolData> tools, SapperToolController toolController)
        {
            foreach (var tool in tools)
            {
                switch (tool.ToolType)
                {
                    case ToolType.Explosive:
                        _explosiveButton.SetData(tool, toolController.CanAfford(ToolType.Explosive));
                        break;
                    case ToolType.Detector:
                        _detectorButton.SetData(tool, toolController.CanAfford(ToolType.Detector));
                        break;
                }
            }
        }

        public void ClearSelection()
        {
            _selectedButton?.SetSelected(false);
            _selectedButton = null;
        }

        private void HandleToolClicked(ToolType toolType, SapperToolButtonView button)
        {
            ClearSelection();
            button.SetSelected(true);
            _selectedButton = button;
            OnToolSelected?.Invoke(toolType);
        }
    }
}
