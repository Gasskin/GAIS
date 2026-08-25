using System.Collections.Generic;
using cfg.battle;
using Framework;
using UnityEngine;

namespace Runtime
{
    public class RougeEffectWindow : MonoBehaviour
    {
        [SerializeField]
        private RougeEffectWidget[] _widgets;

        private readonly List<GameRougeEffectRow> _randomResults = new();
        private bool _widgetsInitialized;

        public void RandomBase()
        {
            gameObject.SetActive(true);
            InitializeWidgets();
            GameEntry.Instance.LubanManager.Tables.GameRougeEffectTable.RandomBase(_randomResults);

            for (int i = 0; i < _widgets.Length; i++)
            {
                bool hasData = i < _randomResults.Count;
                _widgets[i].gameObject.SetActive(hasData);
                if (hasData)
                {
                    _widgets[i].Refresh(_randomResults[i]);
                }
            }
        }

        public void Select(GameRougeEffectRow row)
        {
            row.ChooseCount++;
            gameObject.SetActive(false);
        }

        private void InitializeWidgets()
        {
            if (_widgetsInitialized)
            {
                return;
            }

            for (int i = 0; i < _widgets.Length; i++)
            {
                _widgets[i].Initialize(Select);
            }

            _widgetsInitialized = true;
        }
    }
}
