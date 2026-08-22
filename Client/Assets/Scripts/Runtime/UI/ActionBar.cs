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
        private GameAttr _curActionEnergy;
        
        public void Init(Entity e)
        {
            _entity = e;
            _gais = _entity.GetComponent<GAISComponent>(ComponentID.GAIS);
            _curActionEnergy = _gais.GetAttr(EGameAttr.CurActionEnergy);
            
            _curActionEnergy.OnPostCurrentValueChange += CurActionEnergyOnPostCurrentValueChange;

            CurActionEnergyOnPostCurrentValueChange(0, 0);
        }

        private void CurActionEnergyOnPostCurrentValueChange(float prev, float now)
        {
            actionBar.fillAmount = now / 100f;
        }
    }
}