using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Runtime;
using UnityEngine;

namespace Framework
{
    public class GameEntry : MonoBehaviour
    {
        public static GameEntry Instance { get; private set; }
        public LubanManager LubanManager { get; private set; } = new();
        public EntityManager EntityManager { get; private set; } = new();
        public ProcedureManager ProcedureManager { get; private set; } = new();
        
        // 业务
        public UnitManager UnitManager { get; private set; } = new();
        public LevelManager LevelManager { get; private set; } = new();

        
        private List<BaseManager> _baseManagers = new();
        private List<IUpdateManager> _updateManagers = new();

        
        private bool _isInitEnd = false;


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            _isInitEnd = false;
        }

        private void Start()
        {
            // 框架层
            RegisterManager(LubanManager);
            RegisterManager(EntityManager);
            RegisterManager(ProcedureManager);
            // 业务层
            RegisterManager(UnitManager);
            RegisterManager(LevelManager);

            Initialize().Forget();
        }

        private void OnDestroy()
        {
            for (int i = _baseManagers.Count - 1; i >= 0; i--)
            {
                _baseManagers[i].Destroy();
            }
            _baseManagers.Clear();
            _updateManagers.Clear();
            // _lateUpdateManagers.Clear();
        }

        private void Update()
        {
            if (!_isInitEnd)
            {
                return;
            }
            try
            {
                var dt = Time.deltaTime;
                for (int i = 0; i < _updateManagers.Count; i++)
                {
                    _updateManagers[i].Update(dt);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        // private void LateUpdate()
        // {
        //     if (!_isInitEnd)
        //     {
        //         return;
        //     }
        //     try
        //     {
        //         var dt = Time.deltaTime;
        //         for (int i = 0; i < _lateUpdateManagers.Count; i++)
        //         {
        //             _lateUpdateManagers[i].LateUpdate(dt);
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError(e);
        //         throw;
        //     }
        // }

        private void RegisterManager(BaseManager manager)
        {
            _baseManagers.Add(manager);
            if (manager is IUpdateManager updateManager)
            {
                _updateManagers.Add(updateManager);
            }
            // if (manager is ILateUpdateManager lateUpdateManager)
            // {
            //     _lateUpdateManagers.Add(lateUpdateManager);
            // }
        }

        private async UniTaskVoid Initialize()
        {
            for (int i = 0; i < _baseManagers.Count; i++)
            {
                await _baseManagers[i].Initialize();
            }
            _isInitEnd = true;
            
            ProcedureManager.ChangeProcedure<InitLubanProcedure>(null);
        }
    }
}