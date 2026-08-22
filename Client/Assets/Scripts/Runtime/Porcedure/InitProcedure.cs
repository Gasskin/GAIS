using Framework;

namespace Runtime
{
    public class InitProcedure : BaseProcedure
    {
        private float _delay = 1f;

        public override void Enter()
        {
            GameEntry.Instance.UnitManager.CreatePlayer().Forget();
            GameEntry.Instance.UnitManager.CreateMonster().Forget();
        }

        public override void Update(float dt)
        {
            var player = GameEntry.Instance.UnitManager.Player;
            var enemy = GameEntry.Instance.UnitManager.Enemy;
            if (player == null || enemy == null)
            {
                return;
            }

            _delay -= dt;
            if (_delay > 0f)
            {
                return;
            }

            var sSkill = player.GetComponent<SkillComponent>(ComponentID.SKILL);
            var want = sSkill.GetCastSkill();
            if (want > 0)
            {
                sSkill.CastSkill(want, enemy);
            }

            var eSkill = enemy.GetComponent<SkillComponent>(ComponentID.SKILL);
            want = eSkill.GetCastSkill();
            if (want > 0)
            {
                eSkill.CastSkill(want, player);
            }
        }

        public override void Exit()
        {
        }
    }
}