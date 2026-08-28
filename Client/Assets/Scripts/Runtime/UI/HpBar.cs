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

        [SerializeField] private float _hpChangeSpeed = 2f;
        [SerializeField] private float _hpWhiteChangeSpeed = 0.5f;

        private Entity _entity;

        private GAISComponent _gais;
        private GameAttr _curHp;
        private GameAttr _maxHp;
        private float _targetFillAmount;

        public void Init(Entity e)
        {
            _entity = e;
            _gais = _entity.GetComponent<GAISComponent>();
            _curHp = _gais.GetAttr(EGameAttr.CurHp);
            _maxHp = _gais.GetAttr(EGameAttr.MaxHp);

            _curHp.OnPostCurrentValueChange += CurHpOnPostCurrentValueChange;

            CurHpOnPostCurrentValueChange(0, _curHp.Current);
        }

        private void CurHpOnPostCurrentValueChange(float prev, float now)
        {
            _targetFillAmount = _maxHp.Current > 0f
                ? Mathf.Clamp01(now / _maxHp.Current)
                : 0f;
        }

        private void Update()
        {
            hp.fillAmount = Mathf.MoveTowards(
                hp.fillAmount,
                _targetFillAmount,
                _hpChangeSpeed * Time.deltaTime);

            hpWhite.fillAmount = Mathf.MoveTowards(
                hpWhite.fillAmount,
                _targetFillAmount,
                _hpWhiteChangeSpeed * Time.deltaTime);
        }
    }
}