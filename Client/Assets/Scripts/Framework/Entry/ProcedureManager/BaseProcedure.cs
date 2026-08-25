namespace Framework
{
    public abstract class BaseProcedure
    {
        public abstract void Enter(object data = null);
        public abstract void Update(float dt);
        public abstract void Exit();

        public ProcedureManager Manager { get; set; }

        protected void ChangeProcedure<T>(object data = null) where T : BaseProcedure, new()
        {
            Manager?.ChangeProcedure<T>(data);
        }
    }
}