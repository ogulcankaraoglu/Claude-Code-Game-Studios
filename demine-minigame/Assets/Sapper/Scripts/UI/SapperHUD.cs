using UnityEngine;
using TMPro;
using System.Collections;

namespace Sapper
{
    public class SapperHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageTitleText;
        [SerializeField] private TextMeshProUGUI _movesText;
        [SerializeField] private TextMeshProUGUI _mineProgressText;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private float _messageDuration = 2f;

        private Coroutine _messageRoutine;

        public void SetStageTitle(string title) => _stageTitleText.text = title;
        public void SetMoves(int moves)
        {
            _movesText.text = $"Moves: {moves}";
            _movesText.color = moves <= 3
                ? new Color(1f, 0.361f, 0.102f)
                : new Color(0.941f, 0.929f, 0.910f);
        }
        public void SetMineProgress(int found, int required) => _mineProgressText.text = $"Mines: {found}/{required}";
        public void SetCurrency(int currency) => _currencyText.text = currency.ToString();

        public void ShowMessage(string message)
        {
            if (_messageRoutine != null)
                StopCoroutine(_messageRoutine);
            _messageRoutine = StartCoroutine(ShowMessageRoutine(message));
        }

        private IEnumerator ShowMessageRoutine(string message)
        {
            _messageText.text = message;
            _messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(_messageDuration);
            _messageText.gameObject.SetActive(false);
        }
    }
}
