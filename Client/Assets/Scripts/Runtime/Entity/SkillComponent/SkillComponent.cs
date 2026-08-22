using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class SkillComponent : BaseComponent
    {
        public override int ID => ComponentID.SKILL;
        public override bool IsDefaultUpdate => true;

        private Dictionary<int, SkillInfo> _allSkills = new();

        // 普攻
        private SkillInfo _attack;

        // 身法
        private SkillInfo _move;

        // 心法
        private SkillInfo _heart;

        // 被动技能
        private List<SkillInfo> _passiveSkills = new();

        // 普通技能
        private List<SkillInfo> _normalSkills = new();

        public List<int> InitSkills;

        private Dictionary<int, SkillInfo> _coolDowns = new();
        private List<int> _coolDownHelper = new();

        public bool IsCastSkill => _duration > 0;
        private float _duration;
        private List<TimelineEffect> _timeline = new();

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
            _move = null;
            _heart = null;
            _passiveSkills.Clear();
            _normalSkills.Clear();

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

            if (_duration > 0)
            {
                _duration -= dt;

                for (int i = _timeline.Count - 1; i >= 0; i--)
                {
                    var timeline = _timeline[i];
                    timeline.Delay -= dt;
                    if (timeline.Delay > 0)
                    {
                        continue;
                    }
                    timeline.Target.AddGameEffect(Entity.Uid, timeline.GameEffect);
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
            return skillInfo.GetDynamic(value.DynamicAttr) + value.BaseValue;
        }

        public void AddSkill(int id)
        {
            if (_allSkills.ContainsKey(id))
            {
                return;
            }
            var skill = GameEntry.Instance.LubanManager.Tables.GameSkillTable.DataMap.GetValueOrDefault(id);
            if (skill == null)
            {
                return;
            }
            var skillInfo = ObjectPool.Get<SkillInfo>();
            skillInfo.Skill = skill;
            _allSkills.Add(id, skillInfo);
            switch (skill.Type)
            {
                case EGameSkillType.Attack:
                    if (_attack != null)
                    {
                        _allSkills.Remove(_attack.Skill.Id);
                        ObjectPool.Release(_attack);
                    }
                    _attack = skillInfo;
                    break;
                case EGameSkillType.Move:
                    if (_move != null)
                    {
                        _allSkills.Remove(_move.Skill.Id);
                        ObjectPool.Release(_move);
                    }
                    _move = skillInfo;
                    break;
                case EGameSkillType.Heart:
                    if (_heart != null)
                    {
                        _allSkills.Remove(_heart.Skill.Id);
                        ObjectPool.Release(_heart);
                    }
                    _heart = skillInfo;
                    break;
                case EGameSkillType.Passive:
                    _passiveSkills.Add(skillInfo);
                    break;
                case EGameSkillType.Normal:
                default:
                    _normalSkills.Add(skillInfo);
                    break;
            }
        }

        public int GetCastSkill()
        {
            if (IsCastSkill)
            {
                return 0;
            }
            if (_attack == null || _coolDowns.ContainsKey(_attack.Skill.Id))
            {
                return 0;
            }
            return _attack.Skill.Id;
        }

        public void CastSkill(int id, Entity target)
        {
            if (IsCastSkill)
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

            var unit = Entity.GetComponent<UnitComponent>(ComponentID.UNIT);
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

                timeline.Target = target.GetComponent<GAISComponent>(ComponentID.GAIS);

                _timeline.Add(timeline);
            }
            for (int i = 0; i < skillInfo.Skill.ToSelfEffect_Ref.Count; i++)
            {
                var ge = skillInfo.Skill.ToSelfEffect_Ref[i];

                var timeline = ObjectPool.Get<TimelineEffect>();
                timeline.Delay = skillInfo.Skill.ToSelfEffectTimeline[i];

                timeline.GameEffect = ge;

                timeline.Target = Entity.GetComponent<GAISComponent>(ComponentID.GAIS);

                _timeline.Add(timeline);
            }
        }
    }
}