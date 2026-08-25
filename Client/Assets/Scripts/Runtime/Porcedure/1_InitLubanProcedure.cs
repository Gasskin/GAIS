using Framework;

namespace Runtime
{
    public class InitLubanProcedure : BaseProcedure
    {
        private int _delayExit;
        
        public override void Enter(object data = null)
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
                ChangeProcedure<ChooseBaseProcedure>();
            }
        }

        public override void Exit()
        {
        }
    }
}