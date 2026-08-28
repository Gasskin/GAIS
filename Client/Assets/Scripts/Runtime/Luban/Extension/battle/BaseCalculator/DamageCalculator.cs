using Framework;
using Runtime;

namespace cfg.battle
{
    partial class DamageCalculator
    {
        public override float Calculate(GameEffectSpec ge)
        {
            var source = GameEntry.Instance.EntityManager.GetEntity(ge.Source);
            if (source == null)
            {
                return 0;
            }
            if (!source.GetAllComponents(
                    out GAISComponent sGAIS,
                    out SkillComponent sSkill))
            {
                return 0;
            }
            if (!ge.Target.Entity.GetAllComponents(
                    out UnitComponent tUnit,
                    out SkillComponent tSkill))
            {
                return 0;
            }
      

            var tCurHp = ge.Target.GetAttr(EGameAttr.CurHp);

            var damage = BaseArea();

            if (tCurHp.Current <= damage)
            {
            }
            else
            {
                tCurHp.SetBaseValue(tCurHp.Current - damage);
            }

            tUnit.AssetsRef.damageNumBar.ShowDamage(damage, false);

            return 0;


            // ===========================
            // 基础伤害 = 攻击力 * 技能倍率
            // ===========================
            float BaseArea()
            {
                var atk = sGAIS.GetAttr(EGameAttr.Atk).Current;
                var result = atk * sSkill.GetDynamicValue(Value, SkillId);

                return result;
            }
        }
    }
}