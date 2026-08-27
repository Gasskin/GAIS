using cfg.battle;
using Framework;

namespace Runtime
{
    public class ChooseBaseProcedure : BaseProcedure
    {
        public GameRougeEffectRow WaitForChooseBase { get; set; }

        private bool _isCreatePlayer;

        public override void Enter(object data = null)
        {
            WaitForChooseBase = null;
            _isCreatePlayer = false;
            GameEntry.Instance.AssetsRef.RougeEffectWindow.RandomBase();
        }

        public override void Update(float dt)
        {
            if (WaitForChooseBase == null)
            {
                return;
            }
            if (!_isCreatePlayer)
            {
                GameEntry.Instance.UnitManager.CreatePlayer().Forget();
                _isCreatePlayer = true;
            }
            if (GameEntry.Instance.UnitManager.PlayerUid > 0) 
            {
                ChangeProcedure<CreateEnemy>();
            }
        }

        public override void Exit()
        {
            WaitForChooseBase = null;
        }
    }
}