using Framework;

namespace Runtime
{
    public class InitLubanProcedure : BaseProcedure
    {
        private int _delayExit;
        
        public override void Enter()
        {
            _delayExit = 1;
            var tables = GameEntry.Instance.LubanManager.Tables;
            tables.GameRougeEffectTable.Init();
        }

        public override void Update(float dt)
        {
            _delayExit--;
            if (_delayExit < 0)
            {
                // GameEntry.Instance.ProcedureManager.ChangeProcedure<>();
            }
        }

        public override void Exit()
        {
        }
    }
}