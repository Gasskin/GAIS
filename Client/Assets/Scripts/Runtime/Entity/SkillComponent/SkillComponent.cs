using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    [ComponentID(ComponentID.SKILL)]
    public class SkillComponent : BaseComponent
    {
        public override bool IsDefaultUpdate => true;

        private Dictionary<int, SkillInfo> _allSkills = new();

        // 普攻
        public SkillInfo Attack { get; private set; }

        // 技能
        public SkillInfo Normal { get; private set; }

        // 大招
        public SkillInfo Ultimate { get; private set; }

        // 心法
        public SkillInfo Special { get; private set; }

        public List<int> InitSkills;

        private Dictionary<int, SkillInfo> _coolDowns = new();
        private List<int> _coolDownHelper = new();

        private float _duration;
        private List<TimelineEffect> _timeline = new();

        public bool IsCast => _duration > 0;

        public override async UniTask Initialize()
        {
            if (InitSkills != null)
            {
                foreach (var id in InitSkills)
                {
                    AddSkill(id);
                }
            }
            await UniTask.Yield();
        }

        public override void OnRelease()
        {
            foreach (var skill in _allSkills.Values)
            {
                ObjectPool.Release(skill);
            }
            _allSkills.Clear();
            Ultimate = null;
            Attack = null;
            Normal = null;
            Special = null;

            _coolDowns.Clear();
            _coolDownHelper.Clear();

            _duration = 0;
            foreach (var t in _timeline)
            {
                ObjectPool.Release(t);
            }
            _timeline.Clear();
        }

        public override void Update(float dt)
        {
            if (dt <= 0)
            {
                return;
            }

            // 检查冷却
            if (_coolDowns.Count > 0)
            {
                _coolDownHelper.Clear();
                foreach (var (id, info) in _coolDowns)
                {
                    info.Cooldown -= dt;
                    if (info.Cooldown <= 0)
                    {
                        _coolDownHelper.Add(id);
                    }
                }
                for (int i = 0; i < _coolDownHelper.Count; i++)
                {
                    _coolDowns.Remove(_coolDownHelper[i]);
                }
            }

            if (_duration > 0)
            {
                if (dt > _duration)
                {
                    dt = _duration;
                }
                _duration -= dt;

                for (int i = _timeline.Count - 1; i >= 0; i--)
                {
                    var timeline = _timeline[i];
                    timeline.Delay -= dt;
                    if (timeline.Delay > 0)
                    {
                        continue;
                    }
                    var target = GameEntry.Instance.EntityManager.GetEntity(timeline.TargetUid);
                    if (target != null)
                    {
                        var tGAIS = target.GetComponent<GAISComponent>();
                        tGAIS?.AddGameEffect(Entity.Uid, timeline.GameEffect);
                    }
                    _timeline.RemoveAt(i);
                    ObjectPool.Release(timeline);
                }

                if (_duration <= 0)
                {
                    for (int i = _timeline.Count - 1; i >= 0; i--)
                    {
                        var timeline = _timeline[i];
                        ObjectPool.Release(timeline);
                    }
                    _timeline.Clear();
                }
            }
        }


        public float GetDynamicValue(DynamicValue value, int skillId)
        {
            if (!_allSkills.TryGetValue(skillId, out var skillInfo))
            {
                return value.BaseValue;
            }
            return skillInfo.Get(value.DynamicAttr) + value.BaseValue;
        }

        public void AddSkill(int id)
        {
            if (_allSkills.ContainsKey(id))
            {
                return;
            }
            var skillRow = GameEntry.Instance.LubanManager.Tables.GameSkillTable.DataMap.GetValueOrDefault(id);
            if (skillRow == null)
            {
                return;
            }
            var skillInfo = ObjectPool.Get<SkillInfo>();
            skillInfo.Skill = skillRow;
            _allSkills.Add(id, skillInfo);

            switch (skillRow.Type)
            {
                case EGameSkillType.Attack:
                    if (Attack != null)
                    {
                        _allSkills.Remove(Attack.Skill.Id);
                        ObjectPool.Release(Attack);
                    }
                    Attack = skillInfo;
                    break;
                case EGameSkillType.Normal:
                    if (Normal != null)
                    {
                        _allSkills.Remove(Normal.Skill.Id);
                        ObjectPool.Release(Normal);
                    }
                    Normal = skillInfo;
                    break;
                case EGameSkillType.Ultimate:
                    if (Ultimate != null)
                    {
                        _allSkills.Remove(Ultimate.Skill.Id);
                        ObjectPool.Release(Ultimate);
                    }
                    Ultimate = skillInfo;
                    break;
                case EGameSkillType.Speical:
                    if (Special != null)
                    {
                        _allSkills.Remove(Special.Skill.Id);
                        ObjectPool.Release(Special);
                    }
                    Special = skillInfo;
                    break;
            }
        }


        public bool CanAttack()
        {
            return Attack != null && !_coolDowns.ContainsKey(Attack.Skill.Id);
        }

        public bool CanNormal()
        {
            return Normal != null && !_coolDowns.ContainsKey(Normal.Skill.Id);
        }

        public bool CanUltimate()
        {
            return Ultimate != null && !_coolDowns.ContainsKey(Ultimate.Skill.Id);
        }

        public bool CanSpecial()
        {
            return Special != null && !_coolDowns.ContainsKey(Special.Skill.Id);
        }

        public void CastSkill(int id, Entity target)
        {
            if (IsCast)
            {
                return;
            }

            if (_coolDowns.ContainsKey(id))
            {
                return;
            }
            if (!_allSkills.TryGetValue(id, out var skillInfo))
            {
                return;
            }

            var unit = Entity.GetComponent<UnitComponent>();
            unit.Skill(skillInfo.Skill.Type);

            for (int i = 0; i < _timeline.Count; i++)
            {
                ObjectPool.Release(_timeline[i]);
            }
            _timeline.Clear();

            skillInfo.SetCoolDown();
            _coolDowns.Add(id, skillInfo);

            _duration = skillInfo.Skill.Duration;
            for (int i = 0; i < skillInfo.Skill.ToTargetEffect_Ref.Count; i++)
            {
                var ge = skillInfo.Skill.ToTargetEffect_Ref[i];

                var timeline = ObjectPool.Get<TimelineEffect>();
                timeline.Delay = skillInfo.Skill.ToTargetEffectTimeline[i];

                timeline.GameEffect = ge;

                timeline.TargetUid = target.Uid;

                _timeline.Add(timeline);
            }
            for (int i = 0; i < skillInfo.Skill.ToSelfEffect_Ref.Count; i++)
            {
                var ge = skillInfo.Skill.ToSelfEffect_Ref[i];

                var timeline = ObjectPool.Get<TimelineEffect>();
                timeline.Delay = skillInfo.Skill.ToSelfEffectTimeline[i];

                timeline.GameEffect = ge;

                timeline.TargetUid = Entity.Uid;

                _timeline.Add(timeline);
            }
        }
    }
}