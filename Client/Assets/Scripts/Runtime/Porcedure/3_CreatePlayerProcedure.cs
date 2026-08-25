using cfg.battle;
using Framework;

namespace Runtime
{
    public class CreatePlayerProcedure : BaseProcedure
    {
        private GameRougeEffectRow _base;
        
        public override void Enter(object data = null)
        {
            _base = data as GameRougeEffectRow;
        }

        public override void Update(float dt)
        {
        }

        public override void Exit()
        {
            _base = null;
        }
    }
}