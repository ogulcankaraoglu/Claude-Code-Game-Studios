using System;
using System.Collections.Generic;

namespace Sapper
{
    [Serializable]
    public class StageSaveData
    {
        public string StageId;
        public bool IsUnlocked;
        public bool IsCompleted;
        public bool IsRewardClaimed;
        public int MovesRemaining;
        public int FoundMineCount;
        public List<int> RevealedTileIndices = new List<int>();
    }

    [Serializable]
    public class SapperEventSaveData
    {
        public string EventId;
        public bool IsEventCompleted;
        public string ActiveStageId;
        public List<StageSaveData> Stages = new List<StageSaveData>();
        public int PlayerCurrency;
    }
}
