using cfg.battle;
using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public class HpBar : MonoBehaviour
    {
        public Image hp;
        public Image hpWhite;

        private Entity _entity;

        private GAISComponent _gais;
        private GameAttr _curHp;
        private GameAttr _maxHp;

        public void Init(Entity e)
        {
            _entity = e;
            _gais = _entity.GetComponent<GAISComponent>(ComponentID.GAIS);
            _curHp = _gais.GetAttr(EGameAttr.CurHp);
            _maxHp = _gais.GetAttr(EGameAttr.MaxHp);

            _curHp.OnPostCurrentValueChange += CurHpOnPostCurrentValueChange;

            CurHpOnPostCurrentValueChange(0, _curHp.Current);
        }

        private void CurHpOnPostCurrentValueChange(float prev, float now)
        {
            hp.fillAmount = now / _maxHp.Current;
        }
    }
}