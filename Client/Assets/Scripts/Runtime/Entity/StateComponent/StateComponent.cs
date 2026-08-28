using Cysharp.Threading.Tasks;
using Framework;

namespace Runtime
{
    [ComponentID(ComponentID.STATE)]
    public class StateComponent: BaseComponent
    {
        public EUnitState State { get; private set; }
        
        public override bool IsDefaultUpdate => false;
        public override async UniTask Initialize()
        {
            State = EUnitState.None;
            await UniTask.Yield();
        }

        public override void Update(float dt)
        {
        }

        public override void OnRelease()
        {
            State = EUnitState.None;
        }
    }
}
