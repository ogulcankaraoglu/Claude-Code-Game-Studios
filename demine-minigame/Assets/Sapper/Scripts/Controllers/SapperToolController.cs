using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sapper
{
    public class SapperToolController
    {
        public event Action<int> OnCurrencyChanged;
        public event Action<string> OnToolUseFailed; // message for UI

        private DemineStageController _stage;
        private List<ToolData> _tools;
        private int _currency;

        public int Currency => _currency;

        public void Initialize(DemineStageController stage, List<ToolData> tools, int currency)
        {
            _stage = stage;
            _tools = tools;
            _currency = currency;
        }

        public void SetCurrency(int amount)
        {
            _currency = amount;
            OnCurrencyChanged?.Invoke(_currency);
        }

        // Returns affected tile results on success, null on failure
        public List<TileRevealResult> UseTool(ToolType toolType, int targetTileIndex)
        {
            ToolData tool = _tools.Find(t => t.ToolType == toolType);
            if (tool == null)
            {
                OnToolUseFailed?.Invoke("Tool not available.");
                return null;
            }

            if (_currency < tool.Cost)
            {
                OnToolUseFailed?.Invoke($"Not enough currency. Need {tool.Cost}.");
                return null;
            }

            List<TileRevealResult> results;

            switch (toolType)
            {
                case ToolType.Explosive:
                    results = _stage.RevealArea(targetTileIndex);
                    break;

                case ToolType.Detector:
                    var single = _stage.RevealGuaranteedMine();
                    results = single != null
                        ? new List<TileRevealResult> { single }
                        : new List<TileRevealResult>();
                    break;

                default:
                    return null;
            }

            _currency -= tool.Cost;
            OnCurrencyChanged?.Invoke(_currency);
            return results;
        }

        public bool CanAfford(ToolType toolType)
        {
            ToolData tool = _tools.Find(t => t.ToolType == toolType);
            return tool != null && _currency >= tool.Cost;
        }

        public int GetToolCost(ToolType toolType)
        {
            ToolData tool = _tools.Find(t => t.ToolType == toolType);
            return tool?.Cost ?? 0;
        }
    }
}
