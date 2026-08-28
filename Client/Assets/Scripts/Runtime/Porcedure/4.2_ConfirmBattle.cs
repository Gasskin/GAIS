using Framework;

namespace Runtime
{
    public class ConfirmBattle: BaseProcedure
    {
        public bool StartBattle { get; set; }

        public override void Enter(object data = null)
        {
            StartBattle = false;
            
            AssetsRef.Instance.ConfirmBattleWindow.Show();
        }

        public override void Update(float dt)
        {
            if (StartBattle)
            {
                ChangeProcedure<BattleCircle>();
            }
        }

        public override void Exit()
        {
        }
    }
}
