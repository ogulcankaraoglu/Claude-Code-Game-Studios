using System.Collections.Generic;
using UnityEngine;

namespace Sapper
{
    [CreateAssetMenu(fileName = "SapperEventConfig", menuName = "Sapper/Event Config")]
    public class SapperEventConfig : ScriptableObject
    {
        public string EventId;
        public string EventTitle;
        public double EventDurationSeconds;
        public List<StageData> Stages = new List<StageData>();
        public List<ToolData> Tools = new List<ToolData>();
    }
}
