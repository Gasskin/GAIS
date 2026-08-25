using Framework;

namespace Runtime
{
    public class ChooseBaseProcedure : BaseProcedure
    {
        public bool WaitForChooseBase { get; set; }
        
        public override void Enter()
        {
            WaitForChooseBase = false;
        }

        public override void Update(float dt)
        {
            if (!WaitForChooseBase)
            {
                return;
            }
            
        }

        public override void Exit()
        {
        }
    }
}