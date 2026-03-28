using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sapper
{
    public enum TileState
    {
        Hidden,
        MineFound,
        Miss,
        EmptyRevealed
    }

    public enum StageResult
    {
        None,
        Success,
        Failure
    }

    public class TileRevealResult
    {
        public int TileIndex;
        public TileState State;
        public bool IsMine;
    }

    public class DemineStageController
    {
        public event Action<int> OnMovesChanged;
        public event Action<int> OnFoundMinesChanged;
        public event Action<StageResult> OnStageEnded;

        private StageData _stageData;
        private TileState[] _tileStates;
        private HashSet<int> _mineIndices;
        private int _movesRemaining;
        private int _foundMineCount;
        private bool _firstTap;
        private bool _isActive;

        public int RowCount => _stageData.RowCount;
        public int ColumnCount => _stageData.ColumnCount;
        public int TileCount => RowCount * ColumnCount;
        public int MovesRemaining => _movesRemaining;
        public int FoundMineCount => _foundMineCount;
        public int RequiredMineCount => _stageData.RequiredMineCount;
        public TileState GetTileState(int index) => _tileStates[index];
        public bool IsMine(int index) => _mineIndices.Contains(index);

        public void Initialize(StageData stageData, StageSaveData saveData = null)
        {
            _stageData = stageData;
            _tileStates = new TileState[stageData.RowCount * stageData.ColumnCount];
            _mineIndices = new HashSet<int>();
            _firstTap = true;
            _isActive = true;

            if (saveData != null && saveData.RevealedTileIndices.Count > 0)
            {
                RestoreFromSave(saveData);
            }
            else
            {
                _movesRemaining = stageData.MoveBudget;
                _foundMineCount = 0;
                for (int i = 0; i < _tileStates.Length; i++)
                    _tileStates[i] = TileState.Hidden;
            }
        }

        // Called on first tap — places mines avoiding the tapped tile and its neighbours
        private void PlaceMines(int safeIndex)
        {
            HashSet<int> safeZone = GetNeighbours(safeIndex);
            safeZone.Add(safeIndex);

            List<int> candidates = new List<int>();
            for (int i = 0; i < TileCount; i++)
            {
                if (!safeZone.Contains(i))
                    candidates.Add(i);
            }

            int minesNeeded = Mathf.Min(_stageData.RequiredMineCount, candidates.Count);
            // Fisher-Yates shuffle to pick mines
            for (int i = candidates.Count - 1; i > candidates.Count - 1 - minesNeeded; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int tmp = candidates[i];
                candidates[i] = candidates[j];
                candidates[j] = tmp;
            }

            for (int i = candidates.Count - minesNeeded; i < candidates.Count; i++)
                _mineIndices.Add(candidates[i]);
        }

        private HashSet<int> GetNeighbours(int index)
        {
            HashSet<int> result = new HashSet<int>();
            int row = index / ColumnCount;
            int col = index % ColumnCount;

            for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = row + dr;
                int nc = col + dc;
                if (nr >= 0 && nr < RowCount && nc >= 0 && nc < ColumnCount)
                    result.Add(nr * ColumnCount + nc);
            }
            return result;
        }

        public TileRevealResult RevealTile(int index)
        {
            if (!_isActive) return null;
            if (_tileStates[index] != TileState.Hidden) return null;

            if (_firstTap)
            {
                PlaceMines(index);
                _firstTap = false;
            }

            _movesRemaining--;
            OnMovesChanged?.Invoke(_movesRemaining);

            bool isMine = _mineIndices.Contains(index);
            TileState newState;

            if (isMine)
            {
                newState = TileState.MineFound;
                _tileStates[index] = newState;
                _foundMineCount++;
                OnFoundMinesChanged?.Invoke(_foundMineCount);
            }
            else
            {
                newState = TileState.Miss;
                _tileStates[index] = newState;
            }

            var result = new TileRevealResult
            {
                TileIndex = index,
                State = newState,
                IsMine = isMine
            };

            CheckEndConditions();
            return result;
        }

        // Used by Explosive tool: reveal all tiles in area, return results
        public List<TileRevealResult> RevealArea(int centerIndex, int radius = 1)
        {
            var results = new List<TileRevealResult>();
            if (!_isActive) return results;

            HashSet<int> targets = GetNeighbours(centerIndex);
            targets.Add(centerIndex);

            // Area reveal costs one move total
            _movesRemaining--;
            OnMovesChanged?.Invoke(_movesRemaining);

            foreach (int idx in targets)
            {
                if (_tileStates[idx] != TileState.Hidden) continue;

                bool isMine = _mineIndices.Contains(idx);
                TileState newState = isMine ? TileState.MineFound : TileState.EmptyRevealed;
                _tileStates[idx] = newState;

                if (isMine)
                {
                    _foundMineCount++;
                    OnFoundMinesChanged?.Invoke(_foundMineCount);
                }

                results.Add(new TileRevealResult { TileIndex = idx, State = newState, IsMine = isMine });
            }

            CheckEndConditions();
            return results;
        }

        // Used by Detector tool: reveal one guaranteed mine tile
        public TileRevealResult RevealGuaranteedMine()
        {
            if (!_isActive) return null;

            foreach (int idx in _mineIndices)
            {
                if (_tileStates[idx] == TileState.Hidden)
                {
                    _movesRemaining--;
                    OnMovesChanged?.Invoke(_movesRemaining);

                    _tileStates[idx] = TileState.MineFound;
                    _foundMineCount++;
                    OnFoundMinesChanged?.Invoke(_foundMineCount);

                    var result = new TileRevealResult { TileIndex = idx, State = TileState.MineFound, IsMine = true };
                    CheckEndConditions();
                    return result;
                }
            }
            return null;
        }

        private void CheckEndConditions()
        {
            if (_foundMineCount >= _stageData.RequiredMineCount)
            {
                _isActive = false;
                OnStageEnded?.Invoke(StageResult.Success);
                return;
            }

            if (_movesRemaining <= 0)
            {
                _isActive = false;
                OnStageEnded?.Invoke(StageResult.Failure);
            }
        }

        public StageSaveData BuildSaveData()
        {
            var save = new StageSaveData
            {
                StageId = _stageData.StageId,
                MovesRemaining = _movesRemaining,
                FoundMineCount = _foundMineCount,
            };

            for (int i = 0; i < _tileStates.Length; i++)
            {
                if (_tileStates[i] != TileState.Hidden)
                    save.RevealedTileIndices.Add(i);
            }

            return save;
        }

        private void RestoreFromSave(StageSaveData save)
        {
            _movesRemaining = save.MovesRemaining;
            _foundMineCount = save.FoundMineCount;
            _firstTap = save.RevealedTileIndices.Count == 0;
        }
    }
}
