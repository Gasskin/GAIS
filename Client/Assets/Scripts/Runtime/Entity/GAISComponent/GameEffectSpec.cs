using System.Collections.Generic;
using cfg.battle;
using Framework;
using UnityEngine;

namespace Runtime
{
    public class GameEffectSpec : IPoolObject
    {
    #region static
        private static int _uidFactory = 1;

        public static GameEffectSpec Get(GameEffectRow gameEffect, GAISComponent source, GAISComponent target)
        {
            var spec = ObjectPool.Get<GameEffectSpec>();
            if (_uidFactory >= int.MaxValue)
            {
                _uidFactory = 1;
            }

            spec.Uid = _uidFactory++;

            spec.Source = source;
            spec.Target = target;
            spec.GameEffect = gameEffect;

            spec.ElapsedTime = 0;
            spec.StackCount = 1;

            spec.IsActive = false;
            if (spec.GameEffect.PeriodDuration <= 0 || spec.GameEffect.FirstPeriodImmediately)
            {
                spec.PeriodRemaining = 0;
            }
            else
            {
                spec.PeriodRemaining = spec.GameEffect.PeriodDuration;
            }
            var f = ObjectPool.Get<GameEffectSpecRef>();
            f.GameEffectSpec = spec;
            spec.Ref = f;
            return spec;
        }
    #endregion

        public bool IsValid => Uid > 0;
        public int Uid { get; private set; }

        public GAISComponent Source { get; private set; }

        public GAISComponent Target { get; private set; }


        public GameEffectRow GameEffect { get; private set; }

        public float ElapsedTime { get; private set; }

        public int StackCount { get; private set; }

        public float PeriodRemaining { get; private set; }

        public bool IsActive { get; private set; }
        
        public GameEffectSpecRef Ref { get; private set; }


        public void OnRelease()
        {
            Uid = 0;
            Source = null;
            Target = null;
            ElapsedTime = 0;
            StackCount = 0;
            PeriodRemaining = 0;
            IsActive = false;
            ObjectPool.Release(Ref);
            Ref = null;
        }

    #region 条件判断
        public bool CanApply()
        {
            return Target.HasAllTags(GameEffect.ApplyRequiredTags);
        }

        public bool CanRunning()
        {
            return Target.HasAllTags(GameEffect.OnGoingRequiredTags);
        }

        public bool IsImmune()
        {
            return Target.HasAnyTags(GameEffect.ImmuneWhenTags);
        }
    #endregion

    #region 生命周期
        // ▽▽▽ 仅即时效果 ▽▽▽
        public void OnExecute()
        {
            Target.ApplyInstantGameEffect(this);
        }

        // ▽▽▽ 非即时效果 ▽▽▽
        public void OnAdd()
        {
        }

        public void OnRemove()
        {
        }

        public bool OnActive()
        {
            if (IsActive)
            {
                return false;
            }
            IsActive = true;

            return Target.AddTagsWithDirty(GameEffect.GrantedTags);
        }

        public bool OnDeActive()
        {
            if (!IsActive)
            {
                return false;
            }
            IsActive = false;
            return Target.RemoveTagsWithDirty(GameEffect.GrantedTags);
        }

        public void Tick(float dt)
        {
            if (GameEffect.DurationType == EDurationType.None)
            {
                return;
            }

            TickPeriod(dt);

            TickDuration();

            ElapsedTime += dt;
        }
    #endregion

    #region 堆叠
        public void AddStack(int count, bool refreshDuration, bool refreshPeriod)
        {
            if (count <= 0)
            {
                return;
            }

            var oldCount = StackCount;
            StackCount += count;
            StackCount = Mathf.Clamp(StackCount, 1, GameEffect.StackCountLimit);

            if (refreshDuration)
            {
                RefreshDuration();
            }
            if (refreshPeriod)
            {
                PeriodRemaining = GameEffect.PeriodDuration;
            }
        }

        public void RemoveStack(int count, bool refreshDuration)
        {
            if (count >= StackCount)
            {
                Target.InternalRemoveGameEffectSpec(this);
            }
            else
            {
                var oldCount = StackCount;
                StackCount -= count;

                if (refreshDuration)
                {
                    RefreshDuration();
                }
            }
        }
    #endregion

    #region 持续时间
        private void TickPeriod(float dt)
        {
            if (GameEffect.PeriodEffect_Ref is not { Count: > 0 } || GameEffect.PeriodDuration <= 0.1f)
            {
                return;
            }

            PeriodRemaining -= dt;

            // period触发的行为可能导致所属的GE(_spec)失活/移除, 所属GE移除时会将_spec设置为null
            while (PeriodRemaining < 0f && IsActive)
            {
                // 不能直接重置为Period, 累计误差
                PeriodRemaining += GameEffect.PeriodDuration;
                for (int i = 0; i < GameEffect.PeriodEffect_Ref.Count; i++)
                {
                    var child = GameEffect.PeriodEffect_Ref[i];
                    if (child.DurationType != EDurationType.None)
                    {
                        continue;
                    }
                    var spec = Get(child, Source, Target);
                    if (spec != null)
                    {
                        Target.AddGameEffectSpec(spec);
                    }
                }
            }
        }

        private void TickDuration()
        {
            if (GameEffect.DurationType != EDurationType.Duration)
            {
                return;
            }

            var remaining = GameEffect.DurationTime - ElapsedTime;
            if (remaining > 0)
            {
                return;
            }

            if (GameEffect.StackType == EStackType.None)
            {
                Target.InternalRemoveGameEffectSpec(this);
            }
            else
            {
                switch (GameEffect.StackExpireType)
                {
                    case EStackExpireType.RemoveAll:
                    {
                        Target.InternalRemoveGameEffectSpec(this);
                        break;
                    }
                    case EStackExpireType.RemoveOne:
                    {
                        RemoveStack(1, true);
                        break;
                    }
                }
            }
        }

        private void RefreshDuration()
        {
            ElapsedTime = 0.0f;
        }
    #endregion
    }
}