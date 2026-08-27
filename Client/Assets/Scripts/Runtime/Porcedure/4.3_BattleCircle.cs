using cfg.battle;
using Framework;

namespace Runtime
{
    public class BattleCircle : BaseProcedure
    {
        private Entity _player;
        private Entity _enemy;

        private GAISComponent _pGAIS;
        private GAISComponent _eGAIS;

        private GameAttr _pHp;
        private GameAttr _eHp;
        
        private GameEntry _entry;

        public override void Enter(object data = null)
        {
            _entry = GameEntry.Instance;
            
            _player = _entry.EntityManager.GetEntity(_entry.UnitManager.PlayerUid);
            _pGAIS = _player.GetComponent<GAISComponent>(ComponentID.GAIS);
            _pHp = _pGAIS.GetAttr(EGameAttr.CurHp);
            
            _enemy = _entry.EntityManager.GetEntity(_entry.UnitManager.EnemyUid);
            _eGAIS = _enemy.GetComponent<GAISComponent>(ComponentID.GAIS);
            _eHp = _eGAIS.GetAttr(EGameAttr.CurHp);
        }

        public override void Update(float dt)
        {
            if (CheckFail())
            {
                return;
            }

            if (CheckWin())
            {
                return;
            }
        }

        public override void Exit()
        {
        }


        private bool CheckFail()
        {
            if (_pHp.Current <= 0)
            {
                return true;
            }
            return false;
        }

        private bool CheckWin()
        {
            if (_eHp.Current <= 0)
            {
                ChangeProcedure<BattleEnd>();
                return true;
            }
            return false;
        }
    }
}