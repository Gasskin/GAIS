using System;
using cfg.battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public class RougeEffectWidget : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private TextMeshProUGUI _text;

        private GameRougeEffectRow _row;
        private Action<GameRougeEffectRow> _onSelect;

        public void Initialize(Action<GameRougeEffectRow> onSelect)
        {
            _onSelect = onSelect;
            _button.onClick.RemoveListener(OnClick);
            _button.onClick.AddListener(OnClick);
        }

        public void Refresh(GameRougeEffectRow row)
        {
            _row = row;
            _text.text = row.Desc;
        }

        private void OnClick()
        {
            if (_row != null)
            {
                _onSelect?.Invoke(_row);
            }
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}
