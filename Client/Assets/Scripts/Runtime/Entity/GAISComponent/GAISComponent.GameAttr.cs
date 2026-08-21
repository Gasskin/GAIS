using System.Collections.Generic;
using cfg.battle;
using Framework;

namespace Runtime
{
    partial class GAISComponent
    {
        private List<GameAttr> _gameAttrs = new();

        private void InitGameAttr(Dictionary<EGameAttr, float> initValues)
        {
            // 0号位是None
            _gameAttrs.Add(null);
            for (int i = 1; i < (int)EGameAttr.Max; i++)
            {
                var id = (EGameAttr)i;
                var gameAttr = ObjectPool.Get<GameAttr>();
                if (initValues != null && initValues.TryGetValue(id, out var value))
                {
                    gameAttr.InitValue(id, value);
                }
                else
                {
                    gameAttr.InitValue(id, 0);
                }
                _gameAttrs.Add(gameAttr);
            }

            // 初始化衍生属性
            RegisterDerivedAttrs();

            // 初始化血量
            _gameAttrs[(int)EGameAttr.CurHp].InitValue(EGameAttr.CurHp, _gameAttrs[(int)EGameAttr.MaxHp].Current);
        }

        private void ClearGameAttr()
        {
            for (int i = 0; i < _gameAttrs.Count; i++)
            {
                ObjectPool.Release(_gameAttrs[i]);
            }
            _gameAttrs.Clear();
        }

        private void RegisterDerivedAttrs()
        {
            RegisterDerivedAttr(EGameAttr.MaxHp);
            RegisterDerivedAttr(EGameAttr.Atk);
            RegisterDerivedAttr(EGameAttr.Defence);
            RegisterDerivedAttr(EGameAttr.DefenceFixedIgnore);
            RegisterDerivedAttr(EGameAttr.DefencePctIgnore);
            RegisterDerivedAttr(EGameAttr.CriticalRate);
            RegisterDerivedAttr(EGameAttr.DeCriticalRate);
            RegisterDerivedAttr(EGameAttr.CriticalRatio);
            RegisterDerivedAttr(EGameAttr.DeCriticalRatio);
            RegisterDerivedAttr(EGameAttr.DamageMore);
            RegisterDerivedAttr(EGameAttr.DamageLess);
            RegisterDerivedAttr(EGameAttr.SkillHaste);
            RegisterDerivedAttr(EGameAttr.ActionCharge);

            return;

            void RegisterDerivedAttr(EGameAttr attr)
            {
                var b = attr + 1;
                var mult = attr + 2;
                var add = attr + 3;
                _gameAttrs[(int)attr].RegisterDerived(_gameAttrs[(int)b], _gameAttrs[(int)mult], _gameAttrs[(int)add]);
            }
        }
        
        public GameAttr GetAttr(EGameAttr attr)
        {
            return _gameAttrs[(int)attr];
        }

        public void OnGameEffectDirty()
        {
            for (int i = 0; i < _gameAttrs.Count; i++)
            {
                _gameAttrs[i].UpdateAttr(_gameEffectSpecs);
            }
        }
    }
}