using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Runtime;

namespace Framework
{
    public class ProcedureManager : BaseManager, IUpdateManager
    {
        private Dictionary<Type, BaseProcedure> _procedures = new();

        private BaseProcedure _nowProcedure;

        public override async UniTask Initialize()
        {
            RegisterProcedure(new InitLubanProcedure());
            RegisterProcedure(new ChooseBaseProcedure());
            await UniTask.Yield();
        }

        public override void Destroy()
        {
            _nowProcedure?.Exit();
            _nowProcedure = null;
        }

        public void Update(float dt)
        {
            _nowProcedure?.Update(dt);
        }

        public void ChangeProcedure<T>() where T : BaseProcedure, new()
        {
            var type = typeof(T);
            if (_procedures.TryGetValue(type, out var procedure) && procedure != _nowProcedure)
            {
                _nowProcedure?.Exit();
                _nowProcedure = procedure;
                _nowProcedure.Enter();
            }
        }

        public T GetNow<T>() where T : BaseProcedure, new()
        {
            return  (T)_nowProcedure;
        }

        private void RegisterProcedure(BaseProcedure procedure)
        {
            _procedures.TryAdd(procedure.GetType(), procedure);
        }
    }
}