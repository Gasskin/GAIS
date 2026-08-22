using System.Collections.Generic;
using cfg.battle;
using Framework;

namespace Runtime
{
    public class SkillInfo : IPoolObject
    {
        public GameSkillRow Skill;

        public Dictionary<EGameAttr, float> SkillAttrs = new();
        
        public float Cooldown;
        
        public void OnRelease()
        {
            Skill = null;
            SkillAttrs.Clear();
            Cooldown = 0f;
        }

        public float GetDynamic(EGameAttr attr)
        {
            return SkillAttrs.GetValueOrDefault(attr, 0f);
        }

        public void AddDynamic(EGameAttr attr, float value)
        {
            SkillAttrs.TryAdd(attr, 0);
            SkillAttrs[attr] += value;
        }

        public void SetCoolDown()
        {
            Cooldown = Skill.Cd;
        }
    }
}