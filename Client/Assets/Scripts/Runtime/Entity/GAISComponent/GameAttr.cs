using System.Collections.Generic;
using cfg.battle;
using Framework;
using UnityEngine;
using UnityEngine.Pool;

namespace Runtime
{
    public class GameAttr : IPoolObject
    {
    #region Define
        public delegate void PreBaseValueChange(float baseValue, ref float current);

        public delegate void PostCurrentValueChange(float prev, float now);
    #endregion

        public EGameAttr AttrId { get; private set; }

        public float Base { get; private set; }

        public float Current { get; private set; }

        public bool IsDerived { get; private set; }

        public event PreBaseValueChange OnPreBaseValueChange;

        public event PostCurrentValueChange OnPostCurrentValueChange;

        private List<GameAttrModifierCache> _modifierCache = new();

        private GameAttr _base;
        private GameAttr _mult;
        private GameAttr _add;

        private GameAttr _max;
        private GameAttr _min;
        private float? _fixedMax;
        private float? _fixedMin;

        private float MaxValue
        {
            get
            {
                if (_max != null)
                {
                    return _max.Current;
                }
                return _fixedMax ?? float.MaxValue;
            }
        }

        private float MinValue
        {
            get
            {
                if (_min != null)
                {
                    return _min.Current;
                }
                return _fixedMin ?? float.MinValue;
            }
        }

        public void OnRelease()
        {
            Base = 0;
            Current = 0;
            AttrId = EGameAttr.None;
            IsDerived = false;
            _base = null;
            _mult = null;
            _add = null;
            OnPreBaseValueChange = null;
            OnPostCurrentValueChange = null;
            if (_max != null)
                _max.OnPostCurrentValueChange -= ClampAttrOnPostCurrentValueChange;
            if (_min != null)
                _min.OnPostCurrentValueChange -= ClampAttrOnPostCurrentValueChange;
            _max = null;
            _min = null;
            _fixedMax = null;
            _fixedMin = null;
            for (int i = 0; i < _modifierCache.Count; i++)
            {
                ObjectPool.Release(_modifierCache[i]);
            }
            _modifierCache.Clear();
        }

        public void InitValue(EGameAttr id, float value)
        {
            AttrId = id;
            Base = value;
            Current = value;
            IsDerived = false;
        }

        public void SetRange(GameAttr max = null, float? maxValue = null, GameAttr min = null, float? minValue = null)
        {
            if (_max != null)
                _max.OnPostCurrentValueChange -= ClampAttrOnPostCurrentValueChange;
            if (_min != null)
                _min.OnPostCurrentValueChange -= ClampAttrOnPostCurrentValueChange;

            _max = max;
            _fixedMax = maxValue;
            _min = min;
            _fixedMin = minValue;

            Base = Mathf.Clamp(Base, MinValue, MaxValue);
            Current = Mathf.Clamp(Current, MinValue, MaxValue);

            if (_max != null)
                _max.OnPostCurrentValueChange += ClampAttrOnPostCurrentValueChange;
            if (_min != null)
                _min.OnPostCurrentValueChange += ClampAttrOnPostCurrentValueChange;
        }


        public void SetBaseValue(float newValue)
        {
            if (IsDerived)
            {
                return;
            }
            var prev = Base;
            OnPreBaseValueChange?.Invoke(prev, ref newValue);
            newValue = Mathf.Clamp(newValue, MinValue, MaxValue);
            Base = newValue;
            if (!Mathf.Approximately(prev, newValue))
            {
                CalculateCurrent();
            }
        }

        private void SetCurrentValue(float newValue)
        {
            var prev = Current;
            newValue = Mathf.Clamp(newValue, MinValue, MaxValue);
            Current = newValue;
            if (!Mathf.Approximately(prev, newValue))
            {
                OnPostCurrentValueChange?.Invoke(prev, newValue);
            }
        }

        private void CalculateCurrent()
        {
            var baseValue = Base;
            for (int i = 0; i < _modifierCache.Count; i++)
            {
                var cache = _modifierCache[i];
                baseValue += cache.Modifier.Calculator.Calculate(cache.GameEffectSpec);
            }
            SetCurrentValue(baseValue);
        }

        public void RegisterDerived(GameAttr b, GameAttr mult, GameAttr add)
        {
            Base = 0f;
            IsDerived = true;

            _base = b;
            _mult = mult;
            _add = add;

            _base.OnPostCurrentValueChange += DerivedAttrOnPostCurrentValueChange;
            _mult.OnPostCurrentValueChange += DerivedAttrOnPostCurrentValueChange;
            _add.OnPostCurrentValueChange += DerivedAttrOnPostCurrentValueChange;

            DerivedAttrOnPostCurrentValueChange(0, 0);
        }

        private void DerivedAttrOnPostCurrentValueChange(float prev, float now)
        {
            var final = _base.Current * (1f + _mult.Current) + _add.Current;
            SetCurrentValue(final);
        }


        private void ClampAttrOnPostCurrentValueChange(float prev, float now)
        {
            SetCurrentValue(Current);
        }

        public void UpdateCurrent(List<GameEffectSpec> effects)
        {
            var isDirty = _modifierCache.Count > 0;

            foreach (var m in _modifierCache)
            {
                ObjectPool.Release(m);
            }
            _modifierCache.Clear();

            foreach (var spec in effects)
            {
                if (spec.IsActive)
                {
                    for (int i = 0; i < spec.GameEffect.AttrModifiers.Count; i++)
                    {
                        var modifier = spec.GameEffect.AttrModifiers[i];
                        if (modifier.Attr == AttrId)
                        {
                            var cache = ObjectPool.Get<GameAttrModifierCache>();
                            cache.Modifier = modifier;
                            cache.GameEffectSpec = spec;
                            _modifierCache.Add(cache);
                        }
                    }
                }
            }

            isDirty = isDirty || _modifierCache.Count > 0;

            if (isDirty)
            {
                CalculateCurrent();
            }
        }
    }
}