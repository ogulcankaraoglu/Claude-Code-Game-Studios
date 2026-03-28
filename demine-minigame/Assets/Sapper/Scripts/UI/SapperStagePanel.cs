using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sapper
{
    public class SapperStagePanel : MonoBehaviour
    {
        [SerializeField] private Transform _stageListParent;
        [SerializeField] private SapperStageEntryView _stageEntryPrefab;

        private List<SapperStageEntryView> _entries = new List<SapperStageEntryView>();

        public void Refresh(List<StageData> stages, List<StageSaveData> saves, int activeIndex)
        {
            foreach (var e in _entries) Destroy(e.gameObject);
            _entries.Clear();

            for (int i = 0; i < stages.Count; i++)
            {
                var entry = Instantiate(_stageEntryPrefab, _stageListParent);
                entry.SetData(stages[i], saves[i], i == activeIndex);
                _entries.Add(entry);
            }
        }
    }
}
