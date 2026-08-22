using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class UnitManager : BaseManager
    {
        public Entity Player;
        public Entity Enemy;

        public override async UniTask Initialize()
        {
            await UniTask.Yield();
        }

        public override void Destroy()
        {
            GameEntry.Instance.EntityManager.RemoveEntity(Player);
            GameEntry.Instance.EntityManager.RemoveEntity(Enemy);
        }

        public async UniTaskVoid CreatePlayer()
        {
            var entry = GameEntry.Instance;
            var row = entry.LubanManager.Tables.GameAttrInitTable.Get(1001);
            var e = await EntityFactory.CreateBattleUnit(row.InitValues, entry.assetsRef.player);
            Player = e;
            await UniTask.Yield();
        }

        public async UniTaskVoid CreateMonster()
        {
            var entry = GameEntry.Instance;
            var init = entry.LevelManager.LevelAttr;
            var e = await EntityFactory.CreateBattleUnit(init, entry.assetsRef.enemy);
            Enemy = e;
            await UniTask.Yield();
        }
    }
}