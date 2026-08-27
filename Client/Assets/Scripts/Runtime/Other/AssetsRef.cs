using System;
using UnityEngine;

namespace Runtime
{
    public class AssetsRef : MonoBehaviour
    {
        private static AssetsRef _instance;

        public static AssetsRef Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                return null;
            }
        }
        
        public UnitAssetsRef Player;
        public UnitAssetsRef Enemy;
        
        public RougeEffectWindow RougeEffectWindow;
        public ConfirmBattleWindow ConfirmBattleWindow;


        private void Awake()
        {

            _instance = this;
        }
    }
}