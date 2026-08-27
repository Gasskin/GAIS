using Framework;

namespace Runtime
{
    public class CreateEnemy: BaseProcedure
    {
        public override void Enter(object data = null)
        {
            GameEntry.Instance.UnitManager.CreateMonster().Forget();
        }

        public override void Update(float dt)
        {
            if (GameEntry.Instance.UnitManager.EnemyUid > 0)
            {
                ChangeProcedure<ConfirmBattle>();
            }
        }

        public override void Exit()
        {
        }
    }
}
