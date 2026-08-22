using Framework;

namespace Runtime
{
    public class InitProcedure : BaseProcedure
    {
        public override void Enter()
        {
            GameEntry.Instance.UnitManager.CreatePlayer().Forget();
            GameEntry.Instance.UnitManager.CreateMonster().Forget();
        }

        public override void Update(float dt)
        {
        }

        public override void Exit()
        {
        }
    }
}