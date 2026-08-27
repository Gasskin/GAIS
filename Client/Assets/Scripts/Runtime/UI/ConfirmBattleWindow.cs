using System;
using Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public sealed class ConfirmBattleWindow : MonoBehaviour
    {
        [SerializeField]
        private Button _confirmButton;

        private void Start()
        {
            _confirmButton.onClick.AddListener((() =>
            {
                if (GameEntry.Instance.ProcedureManager.IsNow<ConfirmBattle>(out var now))
                {
                    now.StartBattle = true;
                    gameObject.SetActive(false);
                }
            }));
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
