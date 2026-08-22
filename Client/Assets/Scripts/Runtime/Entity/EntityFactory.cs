using System.Collections.Generic;
using cfg.battle;
using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public static class EntityFactory
    {
        public static async UniTask<Entity> CreateBattleUnit(Dictionary<EGameAttr, float> initValues, UnitAssetsRef assetsRef)
        {
            var e = ObjectPool.Get<Entity>();
            
            var gais = ObjectPool.Get<GAISComponent>();
            gais.InitValues = initValues;
            e.AddComponent(gais);

            var unit = ObjectPool.Get<UnitComponent>();
            unit.AssetsRef = assetsRef;
            e.AddComponent(unit);
            
            await e.Initialize();
            GameEntry.Instance.EntityManager.AddEntity(e);
            return e;
        }
    }
}

