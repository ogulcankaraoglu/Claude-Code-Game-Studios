using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sapper
{
    public class SapperEventController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SapperEventConfig _eventConfig;

        [Header("References")]
        [SerializeField] private DemineBoardView _boardView;
        [SerializeField] private SapperRewardFlow _rewardFlow;
        [SerializeField] private SapperHUD _hud;
        [SerializeField] private SapperStagePanel _stagePanel;
        [SerializeField] private SapperToolBar _toolBar;
        [SerializeField] private GameObject _eventCompleteOverlay;
        [SerializeField] private GameObject _tutorialOverlay;

        private DemineStageController _stageController;
        private SapperToolController _toolController;
        private SapperEventSaveData _saveData;
        private StageData _currentStage;
        private int _currentStageIndex;
        private bool _pendingToolUse;
        private ToolType _pendingToolType;

        private const string SAVE_KEY = "SapperEventSave";
        private const string TUTORIAL_SEEN_KEY = "SapperTutorialSeen";

        private void Start()
        {
            LoadOrCreateSave();
            InitializeControllers();
            ShowTutorialIfNeeded();
            LoadCurrentStage();
        }

        private void LoadOrCreateSave()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                _saveData = JsonUtility.FromJson<SapperEventSaveData>(json);
                if (_saveData.EventId != _eventConfig.EventId)
                    _saveData = CreateFreshSave();
            }
            else
            {
                _saveData = CreateFreshSave();
            }
        }

        private SapperEventSaveData CreateFreshSave()
        {
            var save = new SapperEventSaveData
            {
                EventId = _eventConfig.EventId,
                PlayerCurrency = 500, // starting currency
                ActiveStageId = _eventConfig.Stages[0].StageId
            };

            for (int i = 0; i < _eventConfig.Stages.Count; i++)
            {
                save.Stages.Add(new StageSaveData
                {
                    StageId = _eventConfig.Stages[i].StageId,
                    IsUnlocked = i == 0,
                    IsCompleted = false,
                    IsRewardClaimed = false,
                    MovesRemaining = _eventConfig.Stages[i].MoveBudget,
                    FoundMineCount = 0
                });
            }

            return save;
        }

        private void InitializeControllers()
        {
            _stageController = new DemineStageController();
            _toolController = new SapperToolController();

            _stageController.OnMovesChanged += moves =>
            {
                _hud.SetMoves(moves);
                PersistSave();
            };

            _stageController.OnFoundMinesChanged += found =>
            {
                _hud.SetMineProgress(found, _currentStage.RequiredMineCount);
                PersistSave();
            };

            _stageController.OnStageEnded += HandleStageEnded;

            _toolController.OnCurrencyChanged += currency =>
            {
                _hud.SetCurrency(currency);
                _saveData.PlayerCurrency = currency;
                PersistSave();
            };

            _toolController.OnToolUseFailed += msg => _hud.ShowMessage(msg);
        }

        private void LoadCurrentStage()
        {
            _currentStageIndex = FindUncompletedStageIndex();
            if (_currentStageIndex < 0)
            {
                ShowEventComplete();
                return;
            }

            _currentStage = _eventConfig.Stages[_currentStageIndex];
            StageSaveData stageSave = _saveData.Stages[_currentStageIndex];

            _stageController.Initialize(_currentStage, stageSave);
            _toolController.Initialize(_stageController, _eventConfig.Tools, _saveData.PlayerCurrency);

            _boardView.BuildBoard(_currentStage.RowCount, _currentStage.ColumnCount);
            _boardView.OnTileSelected -= HandleTileSelected;
            _boardView.OnTileSelected += HandleTileSelected;

            _hud.SetStageTitle(_currentStage.DisplayName);
            _hud.SetMoves(_stageController.MovesRemaining);
            _hud.SetMineProgress(_stageController.FoundMineCount, _currentStage.RequiredMineCount);
            _hud.SetCurrency(_toolController.Currency);

            _stagePanel.Refresh(_eventConfig.Stages, _saveData.Stages, _currentStageIndex);
            _toolBar.Refresh(_eventConfig.Tools, _toolController);
            _toolBar.OnToolSelected -= HandleToolSelected;
            _toolBar.OnToolSelected += HandleToolSelected;

            _rewardFlow.OnRewardClaimed -= HandleRewardClaimed;
            _rewardFlow.OnRewardClaimed += HandleRewardClaimed;
            _rewardFlow.OnRetryRequested -= HandleRetry;
            _rewardFlow.OnRetryRequested += HandleRetry;
        }

        private void HandleTileSelected(int index)
        {
            if (_pendingToolUse)
            {
                ExecuteToolOnTile(_pendingToolType, index);
                _pendingToolUse = false;
                _toolBar.ClearSelection();
                return;
            }

            var result = _stageController.RevealTile(index);
            if (result != null)
                _boardView.ApplyRevealResult(result);
        }

        private void HandleToolSelected(ToolType toolType)
        {
            if (toolType == ToolType.Detector)
            {
                // Detector doesn't need tile selection
                ExecuteToolOnTile(toolType, -1);
                _toolBar.ClearSelection();
                return;
            }

            // Explosive needs a target tile — enter targeting mode
            _pendingToolUse = true;
            _pendingToolType = toolType;
            _hud.ShowMessage("Select a tile for the Explosive.");
        }

        private void ExecuteToolOnTile(ToolType toolType, int tileIndex)
        {
            var results = _toolController.UseTool(toolType, tileIndex);
            if (results != null)
                _boardView.ApplyRevealResults(results);
        }

        private void HandleStageEnded(StageResult result)
        {
            _boardView.LockBoard();
            _pendingToolUse = false;

            if (result == StageResult.Success)
            {
                _saveData.Stages[_currentStageIndex].IsCompleted = true;
                PersistSave();
                _rewardFlow.ShowReward(_currentStage);
            }
            else
            {
                _rewardFlow.ShowFailure();
            }
        }

        private void HandleRewardClaimed()
        {
            _saveData.Stages[_currentStageIndex].IsRewardClaimed = true;

            // Unlock next stage
            if (_currentStageIndex + 1 < _eventConfig.Stages.Count)
                _saveData.Stages[_currentStageIndex + 1].IsUnlocked = true;

            PersistSave();
            LoadCurrentStage();
        }

        private void HandleRetry()
        {
            // Reset stage save — mines will be re-randomized on first tap
            _saveData.Stages[_currentStageIndex] = new StageSaveData
            {
                StageId = _currentStage.StageId,
                IsUnlocked = true,
                IsCompleted = false,
                IsRewardClaimed = false,
                MovesRemaining = _currentStage.MoveBudget,
                FoundMineCount = 0
            };
            PersistSave();
            LoadCurrentStage();
        }

        private int FindUncompletedStageIndex()
        {
            for (int i = 0; i < _saveData.Stages.Count; i++)
            {
                var s = _saveData.Stages[i];
                if (s.IsUnlocked && !s.IsCompleted)
                    return i;
            }
            return -1;
        }

        private void ShowEventComplete()
        {
            _saveData.IsEventCompleted = true;
            PersistSave();
            _eventCompleteOverlay.SetActive(true);
        }

        private void ShowTutorialIfNeeded()
        {
            if (PlayerPrefs.GetInt(TUTORIAL_SEEN_KEY, 0) == 0)
            {
                _tutorialOverlay.SetActive(true);
                PlayerPrefs.SetInt(TUTORIAL_SEEN_KEY, 1);
                PlayerPrefs.Save();
            }
        }

        private void PersistSave()
        {
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(_saveData));
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) PersistSave();
        }
    }
}
