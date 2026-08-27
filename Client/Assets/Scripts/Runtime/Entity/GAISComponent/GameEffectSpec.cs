using System;
using System.Collections.Generic;
using cfg.battle;
using Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime
{
    public class GameEffectSpec : IPoolObject
    {
    #region static
        private static int _uidFactory = 1;

        public static GameEffectSpec Get(GameEffectRow gameEffect, int sourceId, GAISComponent target)
        {
            if (gameEffect == null)
            {
                return null;
            }
            var spec = ObjectPool.Get<GameEffectSpec>();
            if (_uidFactory >= int.MaxValue)
            {
                _uidFactory = 1;
            }

            spec.Uid = _uidFactory++;

            spec.Source = sourceId;
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
            return spec;
        }
    #endregion

        public bool IsValid => Uid > 0;
        public int Uid { get; private set; }

        public int Source { get; private set; }

        public GAISComponent Target { get; private set; }


        public GameEffectRow GameEffect { get; private set; }

        public float ElapsedTime { get; private set; }

        public int StackCount { get; private set; }

        public float PeriodRemaining { get; private set; }

        public bool IsActive { get; private set; }

        private const float MIN_PERIOD_INTERVAL = 0.1f;

        public void OnRelease()
        {
            Uid = 0;
            Source = 0;
            Target = null;
            ElapsedTime = 0;
            StackCount = 0;
            PeriodRemaining = 0;
            IsActive = false;
            GameEffect = null;
        }

    #region 条件判断
        // 仅添加时判断1次
        public bool CanApply()
        {
            return Target.HasAllTags(GameEffect.ApplyRequiredTags);
        }

        // 有变化时会再次判断
        public bool CanRunning()
        {
            return Target.HasAllTags(GameEffect.OnGoingRequiredTags);
        }

        // 仅添加时判断1次
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

        public void OnActive()
        {
            if (IsActive)
            {
                return;
            }
            IsActive = true;

            for (int i = 0; i < GameEffect.GrantedTags.Count; i++)
            {
                Target.AddTag(GameEffect.GrantedTags[i]);
            }
        }

        public void OnDeActive()
        {
            if (!IsActive)
            {
                return;
            }
            IsActive = false;
            for (int i = 0; i < GameEffect.GrantedTags.Count; i++)
            {
                Target.RemoveTag(GameEffect.GrantedTags[i]);
            }
        }

        public void Tick(float dt)
        {
            if (GameEffect.DurationType == EDurationType.None)
            {
                return;
            }

            var activeDt = dt;
            var shouldExpire = false;

            if (GameEffect.DurationType == EDurationType.Duration)
            {
                var remaining = GameEffect.DurationTime - ElapsedTime;

                if (remaining <= 0f)
                {
                    Expire();
                    return;
                }

                activeDt = Mathf.Min(dt, remaining);
                shouldExpire = dt >= remaining;
            }

            TickPeriod(activeDt);
            ElapsedTime += activeDt;

            if (shouldExpire)
            {
                Expire();
            }
        }
    #endregion

    #region 堆叠
        public bool AddStack(int count, bool refreshDuration, bool refreshPeriod)
        {
            if (count <= 0)
            {
                return false;
            }

            var oldCount = StackCount;
            StackCount += count;
            StackCount = Math.Min(StackCount, GameEffect.StackCountLimit);
            StackCount = Math.Max(StackCount, 1);

            if (refreshDuration && (oldCount < StackCount || GameEffect.OverflowStackRefreshDuration))
            {
                RefreshDuration();
            }
            if (refreshPeriod)
            {
                PeriodRemaining = GameEffect.PeriodDuration;
            }
            return oldCount < StackCount;
        }

        public void RemoveStack(int count, bool refreshDuration)
        {
            if (count <= 0)
            {
                return;
            }
            if (count >= StackCount)
            {
                Target.InternalRemoveGameEffectSpec(this);
            }
            else
            {
                StackCount -= count;

                if (refreshDuration)
                {
                    RefreshDuration();
                }

                Target.OnGameEffectDirty();
            }
        }
    #endregion

    #region 持续时间
        private void TickPeriod(float dt)
        {
            if (GameEffect.PeriodEffect_Ref is not { Count: > 0 } || GameEffect.PeriodDuration < MIN_PERIOD_INTERVAL)
            {
                return;
            }

            PeriodRemaining -= dt;

            // period触发的行为可能导致所属的GE(_spec)失活/移除, 所属GE移除时会将_spec设置为null
            while (PeriodRemaining <= 0f)
            {
                // 不能直接重置为Period, 累计误差
                PeriodRemaining += GameEffect.PeriodDuration;
                if (IsActive)
                {
                    for (int i = 0; i < GameEffect.PeriodEffect_Ref.Count; i++)
                    {
                        var child = GameEffect.PeriodEffect_Ref[i];
                        if (child is not { DurationType: EDurationType.None })
                        {
                            continue;
                        }
                        Target.AddGameEffect(Source, child);
                    }
                }
            }
        }

        void Expire()
        {
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