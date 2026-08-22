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
    }
}