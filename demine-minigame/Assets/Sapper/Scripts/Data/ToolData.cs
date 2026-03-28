using System;
using UnityEngine;

namespace Sapper
{
    public enum ToolType
    {
        Explosive,
        Detector
    }

    [Serializable]
    public class ToolData
    {
        public ToolType ToolType;
        public string DisplayName;
        public int Cost;
        public Sprite Icon;
    }
}
