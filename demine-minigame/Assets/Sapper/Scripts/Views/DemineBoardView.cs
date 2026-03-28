using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sapper
{
    public class DemineBoardView : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private DemineTileView _tilePrefab;

        public event Action<int> OnTileSelected;

        private List<DemineTileView> _tiles = new List<DemineTileView>();

        public void BuildBoard(int rows, int columns)
        {
            ClearBoard();

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;

            int total = rows * columns;
            for (int i = 0; i < total; i++)
            {
                var tile = Instantiate(_tilePrefab, _grid.transform);
                tile.Initialize(i);
                tile.OnTapped += index => OnTileSelected?.Invoke(index);
                _tiles.Add(tile);
            }
        }

        public void ApplyRevealResult(TileRevealResult result)
        {
            if (result == null) return;
            _tiles[result.TileIndex].SetState(result.State);
        }

        public void ApplyRevealResults(List<TileRevealResult> results)
        {
            foreach (var r in results)
                ApplyRevealResult(r);
        }

        public void SetBoardInteractable(bool interactable)
        {
            foreach (var tile in _tiles)
                tile.SetInteractable(interactable && tile.CurrentState == TileState.Hidden);
        }

        public void LockBoard() => SetBoardInteractable(false);

        private void ClearBoard()
        {
            foreach (var tile in _tiles)
                Destroy(tile.gameObject);
            _tiles.Clear();
        }
    }
}
