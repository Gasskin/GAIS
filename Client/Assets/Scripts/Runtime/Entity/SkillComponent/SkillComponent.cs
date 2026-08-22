using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{

    public class SkillComponent : BaseComponent
    {
        public override int ID => ComponentID.SKILL;
        public override bool IsDefaultUpdate => false;

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

        private Dictionary<int, float> _coolDowns = new();
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
            _allSkills = null;
            _move = null;
            _heart = null;
            _passiveSkills.Clear();
            _passiveSkills = null;
            _normalSkills.Clear();
            _normalSkills = null;
            
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
            foreach (var (id, value) in _coolDowns)
            {
                var newValue = value - dt;
                _coolDowns[id] = newValue;
                if (value <= 0)
                {
                    _coolDownHelper.Add(id);
                }
            }
            for (int i = 0; i < _coolDownHelper.Count; i++)
            {
                _coolDowns.Remove(_coolDownHelper[i]);
            }

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
                        ObjectPool.Release(_attack);
                    }
                    _attack = skillInfo;
                    break;
                case EGameSkillType.Move:
                    if (_move != null)
                    {
                        ObjectPool.Release(_move);
                    }
                    _move = skillInfo;
                    break;
                case EGameSkillType.Heart:
                    if (_heart != null)
                    {
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
            unit.Skill();

            for (int i = 0; i < _timeline.Count; i++)
            {
                ObjectPool.Release(_timeline[i]);
            }
            _timeline.Clear();
            
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