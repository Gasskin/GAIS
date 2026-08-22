using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public static class EntityFactory
    {
        public static async UniTask<Entity> CreateBattleUnit(Dictionary<EGameAttr, float> initValues, UnitAssetsRef assetsRef, List<int> initSkills)
        {
            var e = ObjectPool.Get<Entity>();

            var gais = ObjectPool.Get<GAISComponent>();
            gais.InitValues = initValues;
            e.AddComponent(gais);

            var unit = ObjectPool.Get<UnitComponent>();
            unit.AssetsRef = assetsRef;
            e.AddComponent(unit);

            var skill = ObjectPool.Get<SkillComponent>();
            skill.InitSkills = initSkills;
            e.AddComponent(skill);

            await e.Initialize();
            GameEntry.Instance.EntityManager.AddEntity(e);
            return e;
        }
    }
}