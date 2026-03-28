using System;
using UnityEngine;

namespace Sapper
{
    [Serializable]
    public class StageData
    {
        public string StageId;
        public string DisplayName;
        public int RowCount;
        public int ColumnCount;
        public int RequiredMineCount;
        public int MoveBudget;
        public RewardType RewardType;
        public string RewardId;
        public int RewardAmount;
        public string BackgroundTheme;
        public string UnlockCondition;
    }
}
