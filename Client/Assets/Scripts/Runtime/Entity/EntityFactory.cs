using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public static class EntityFactory
    {
        public static async UniTask CreateTestEntity()
        {
            var e = ObjectPool.Get<Entity>();
            var gais = ObjectPool.Get<GAISComponent>();
            gais.InitValues = GameEntry.Instance.LubanManager.Tables.GameAttrInitTable.DataMap[1001].InitValues;
            e.AddComponent(gais);
            await e.Initialize();
            GameEntry.Instance.EntityManager.AddEntity(e);
        }
    }
}

