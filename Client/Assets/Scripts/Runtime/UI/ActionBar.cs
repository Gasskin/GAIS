using cfg.battle;
using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public class ActionBar : MonoBehaviour
    {
        public Image actionBar;
        
        private Entity _entity;
        private GAISComponent _gais;
        private GameAttr _curSpecialEnergy;
        
        public void Init(Entity e)
        {
            _entity = e;
            _gais = _entity.GetComponent<GAISComponent>();
            _curSpecialEnergy = _gais.GetAttr(EGameAttr.CurSpecialEnergy);
            
            _curSpecialEnergy.OnPostCurrentValueChange += CurSpecialEnergyOnPostCurrentValueChange;

            CurSpecialEnergyOnPostCurrentValueChange(0, 0);
        }

        private void CurSpecialEnergyOnPostCurrentValueChange(float prev, float now)
        {
            actionBar.fillAmount = now / 100f;
        }
    }
}