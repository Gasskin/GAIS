using cfg.battle;
using Framework;

namespace Runtime
{
    public class BattleCircle : BaseProcedure
    {
        private Entity _player;
        private Entity _enemy;

        private GAISComponent _pGAIS;
        private SkillComponent _pSkill;
        private GameAttr _pHp;
        private GameAttr _pEnergy;
        private int _pRound = 0;


        private GAISComponent _eGAIS;
        private SkillComponent _eSkill;
        private GameAttr _eHp;
        private GameAttr _energy;
        private int _eRound = 0;

        private GameEntry _entry;

        public override void Enter(object data = null)
        {
            _entry = GameEntry.Instance;

            _player = _entry.EntityManager.GetEntity(_entry.UnitManager.PlayerUid);
            _pGAIS = _player.GetComponent<GAISComponent>();
            _pSkill = _player.GetComponent<SkillComponent>();
            _pHp = _pGAIS.GetAttr(EGameAttr.CurHp);
            _pEnergy = _pGAIS.GetAttr(EGameAttr.CurSpecialEnergy);
            _pRound = 1;

            _enemy = _entry.EntityManager.GetEntity(_entry.UnitManager.EnemyUid);
            _eGAIS = _enemy.GetComponent<GAISComponent>();
            _eSkill = _enemy.GetComponent<SkillComponent>();
            _eHp = _eGAIS.GetAttr(EGameAttr.CurHp);
            _energy = _eGAIS.GetAttr(EGameAttr.CurSpecialEnergy);
            _eRound = 1;
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

            if (_pSkill.IsCast || _eSkill.IsCast)
            {
                return;
            }

            if (_pRound > 0)
            {
                if (_pSkill.CanSpecial())
                {
                    _pSkill.CastSkill(_pSkill.Special.Skill.Id, _enemy);
                    return;
                }
                if (_pSkill.CanUltimate())
                {
                    _pRound--;
                    _pSkill.CastSkill(_pSkill.Ultimate.Skill.Id, _enemy);
                    return;
                }
                if (_pSkill.CanNormal())
                {
                    _pRound--;
                    _pSkill.CastSkill(_pSkill.Normal.Skill.Id, _enemy);
                    return;
                }
                if (_pSkill.CanAttack())
                {
                    _pRound--;
                    _pSkill.CastSkill(_pSkill.Attack.Skill.Id, _enemy);
                    return;
                }
            }

            if (_eRound > 0)
            {
                if (_eSkill.CanSpecial())
                {
                    _eSkill.CastSkill(_eSkill.Special.Skill.Id, _player);
                    return;
                }
                if (_eSkill.CanUltimate())
                {
                    _eRound--;
                    _eSkill.CastSkill(_eSkill.Ultimate.Skill.Id, _player);
                    return;
                }
                if (_eSkill.CanNormal())
                {
                    _eRound--;
                    _eSkill.CastSkill(_eSkill.Normal.Skill.Id, _player);
                    return;
                }
                if (_eSkill.CanAttack())
                {
                    _eRound--;
                    _eSkill.CastSkill(_eSkill.Attack.Skill.Id, _player);
                    return;
                }
            }

            _pRound++;
            _eRound++;
        }

        public override void Exit()
        {
        }


        private bool CheckFail()
        {
            if (_pHp.Current <= 0)
            {
                ChangeProcedure<BattleEnd>();
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