using Framework;

namespace Runtime
{
    public class BattleCircle : BaseProcedure
    {
        private Entity _player;
        private Entity _enemy;
        private GameEntry _entry;

        public override void Enter(object data = null)
        {
            _entry = GameEntry.Instance;
            _player = _entry.EntityManager.GetEntity(_entry.UnitManager.PlayerUid);
            _enemy = _entry.EntityManager.GetEntity(_entry.UnitManager.EnemyUid);
        }

        public override void Update(float dt)
        {
            if (_player == null || _enemy == null)
            {
                return;
            }
            
            
        }

        public override void Exit()
        {
        }
    }
}