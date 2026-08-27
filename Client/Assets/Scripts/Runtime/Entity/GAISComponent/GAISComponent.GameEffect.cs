using System.Collections.Generic;
using cfg.battle;
using Framework;

namespace Runtime
{
    partial class GAISComponent
    {
        private readonly List<GameEffectSpec> _gameEffectSpecs = new();
        private readonly Dictionary<int, GameEffectSpec> _gameEffectSpecsDict = new();
        private List<GameEffectSpec> _tickPool = new(128);

        private void TickGameEffect(float dt)
        {
            _tickPool.Clear();
            _tickPool.AddRange(_gameEffectSpecs);
            for (int i = 0; i < _tickPool.Count; i++)
            {
                var spec = _tickPool[i];
                spec.Tick(dt);
            }
        }

        private void ClearGameEffectSpecs()
        {
            for (int i = 0; i < _gameEffectSpecs.Count; i++)
            {
                var spec = _gameEffectSpecs[i];
                spec.OnDeActive();
                spec.OnRemove();
                ObjectPool.Release(spec);
            }
            _gameEffectSpecs.Clear();
            _gameEffectSpecsDict.Clear();
        }

        public void ApplyInstantGameEffect(GameEffectSpec spec)
        {
            // Meta Attr
            if (spec.GameEffect.AttrModifiers is { Count: 1 })
            {
                var modifier = spec.GameEffect.AttrModifiers[0];
                if (modifier.Attr >= EGameAttr.MetaNone)
                {
                    modifier.Calculator.Calculate(spec);
                    return;
                }
            }

            for (int i = 0; i < spec.GameEffect.AttrModifiers.Count; i++)
            {
                var modifier = spec.GameEffect.AttrModifiers[i];
                if (modifier.Attr is >= EGameAttr.Max)
                {
                    continue;
                }

                var attr = _gameAttrs[(int)modifier.Attr];
                if (attr == null || attr.IsDerived)
                {
                    continue;
                }
                attr.SetBaseValue(attr.Base + modifier.Calculator.Calculate(spec));
            }
        }

        public int AddGameEffect(int sourceId, GameEffectRow gameEffect)
        {
            var entity = GameEntry.Instance.EntityManager.GetEntity(sourceId);
            if (entity == null)
            {
                return 0;
            }
            var spec = GameEffectSpec.Get(gameEffect, sourceId, this);
            if (spec == null)
            {
                return 0;
            }
            return AddGameEffectSpec(spec);
        }

        private int AddGameEffectSpec(GameEffectSpec spec)
        {
            if (!spec.CanApply())
            {
                ObjectPool.Release(spec);
                return 0;
            }

            if (spec.IsImmune())
            {
                ObjectPool.Release(spec);
                return 0;
            }

            // 瞬时GE
            if (spec.GameEffect.DurationType == EDurationType.None)
            {
                spec.OnExecute();
                ObjectPool.Release(spec);
                return 0;
            }

            // 不堆叠
            if (spec.GameEffect.StackType == EStackType.None)
            {
                AddNewGameEffectSpec();
                return spec.Uid;
            }

            GameEffectSpec stackSpec = null;
            for (int i = 0; i < _gameEffectSpecs.Count; i++)
            {
                var tSpec = _gameEffectSpecs[i];
                // 按照来源堆叠，要求ge的来源是同一个人
                // 否则按照目标堆叠，只要ge相同就堆起来就行
                if (spec.GameEffect.StackType == EStackType.Source && tSpec.Source != spec.Source)
                {
                    continue;
                }
                if (tSpec.IsValid && tSpec.GameEffect.Id == spec.GameEffect.Id)
                {
                    stackSpec = tSpec;
                    break;
                }
            }
            // 不存在，直接新增
            if (stackSpec == null)
            {
                AddNewGameEffectSpec();
                return spec.Uid;
            }
            if (stackSpec.AddStack(1, true, false))
            {
                OnGameEffectDirty();
            }

            // 堆叠后可以直接释放
            ObjectPool.Release(spec);

            return stackSpec.Uid;

            void AddNewGameEffectSpec()
            {
                _gameEffectSpecs.Add(spec);
                _gameEffectSpecsDict.Add(spec.Uid, spec);
                spec.OnAdd();
                if (spec.CanRunning())
                {
                    spec.OnActive();
                    OnGameEffectDirty();
                }
            }
        }

        public void InternalRemoveGameEffectSpec(GameEffectSpec spec)
        {
            // 必须先删除，DeActive可能触发TagDirty
            _gameEffectSpecs.Remove(spec);
            _gameEffectSpecsDict.Remove(spec.Uid);
            spec.OnDeActive();
            spec.OnRemove();
            OnGameEffectDirty();
            ObjectPool.Release(spec);
        }
    }
}