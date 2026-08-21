using System.Collections.Generic;
using cfg.battle;
using Framework;

namespace Runtime
{
    partial class GAISComponent
    {
        private readonly List<GameEffectSpec> _gameEffectSpecs = new();
        private List<GameEffectSpec> _tickPool = new(128);

        private void TickGameEffect(float dt)
        {
            _tickPool.Clear();
            _tickPool.AddRange(_gameEffectSpecs);
            for (int i = 0; i < _tickPool.Count; i++)
            {
                var spec = _tickPool[i];
                if (spec.IsActive)
                {
                    spec.Tick(dt);
                }
            }
        }
        
        public void ApplyInstantGameEffect(GameEffectSpec spec)
        {
            foreach (var modifier in spec.GameEffect.AttrModifiers)
            {
                var attr = _gameAttrs[(int)modifier.Attr];
                if (attr == null)
                {
                    return;
                }
                attr.SetBaseValue(attr.Base + modifier.Calculator.Calculate(spec));
            }
        }

        public int AddGameEffectSpec(GameEffectSpec spec)
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
                if (AddNewGameEffectSpec(spec.GameEffect.StackCountLimit))
                {
                    return spec.Uid;
                }
                ObjectPool.Release(spec);
                return 0;
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
                AddNewGameEffectSpec(0);
                return spec.Uid;
            }
            stackSpec.AddStack(1, true, false);
            // 堆叠后可以直接释放
            ObjectPool.Release(spec);

            return stackSpec.Uid;

            bool AddNewGameEffectSpec(int countLimit)
            {
                // 总数检查
                if (countLimit > 0)
                {
                    var has = 0;
                    for (int i = 0; i < _gameEffectSpecs.Count; i++)
                    {
                        if (_gameEffectSpecs[i].GameEffect.Id == spec.GameEffect.Id)
                        {
                            has++;
                            if (has >= countLimit)
                            {
                                return false;
                            }
                        }
                    }
                }
                _gameEffectSpecs.Add(spec);
                spec.OnAdd();
                if (spec.CanRunning())
                {
                    spec.OnActive();
                    OnGameEffectDirty();
                }
                return true;
            }
        }
        
        public void InternalRemoveGameEffectSpec(GameEffectSpec spec)
        {
            // 必须先删除，DeActive可能触发TagDirty
            _gameEffectSpecs.Remove(spec);
            spec.OnDeActive();
            spec.OnRemove();
            ObjectPool.Release(spec);
        }
        
        public void UpdateEffectState()
        {
            var hasDirty = false;
            foreach (var spec in _gameEffectSpecs)
            {
                if (spec.IsActive)
                {
                    if (!spec.CanRunning())
                    {
                        var dirty = spec.OnDeActive();
                        hasDirty = hasDirty || dirty;
                    }
                }
                else
                {
                    if (spec.CanRunning())
                    {
                        var dirty =  spec.OnActive();
                        hasDirty = hasDirty || dirty;
                    }
                }
            }
            if (hasDirty)
            {
                OnGameEffectDirty();
            }
        }
    }
}
