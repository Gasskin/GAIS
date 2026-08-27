using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    public class UnitManager : BaseManager
    {
        public int PlayerUid { get; private set; }
        public int EnemyUid { get; private set; }

        public override async UniTask Initialize()
        {
            PlayerUid = 0;
            EnemyUid = 0;
            await UniTask.Yield();
        }

        public override void Destroy()
        {
            GameEntry.Instance.EntityManager.RemoveEntity(PlayerUid);
            GameEntry.Instance.EntityManager.RemoveEntity(EnemyUid);
            PlayerUid = 0;
            EnemyUid = 0;
        }

        public async UniTaskVoid CreatePlayer()
        {
            var entry = GameEntry.Instance;
            var row = entry.LubanManager.Tables.EnityInitTable.Get(1001);
            var e = await EntityFactory.CreateBattleUnit(row.InitAttrs, AssetsRef.Instance.Player, row.InitSkills);
            PlayerUid = e;
            await UniTask.Yield();
        }

        public async UniTaskVoid CreateMonster()
        {
            var entry = GameEntry.Instance;
            var row = entry.LubanManager.Tables.EnityInitTable.Get(entry.LevelManager.Level);
            var e = await EntityFactory.CreateBattleUnit(row.InitAttrs, AssetsRef.Instance.Enemy, row.InitSkills);
            EnemyUid = e;
            await UniTask.Yield();
        }
    }
}