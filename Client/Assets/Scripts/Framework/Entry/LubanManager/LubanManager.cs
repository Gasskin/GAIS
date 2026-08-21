using cfg;
using Cysharp.Threading.Tasks;
using Luban;
using UnityEngine;

namespace Framework
{
    public class LubanManager: BaseManager
    {
        public Tables Tables { get; private set; }
        
        public override async UniTask Initialize()
        {
            Tables = new Tables(LoadByteBuf);
            await UniTask.Yield();
        }

        public override void Destroy()
        {
            Tables = null;
        }
        
        private static ByteBuf LoadByteBuf(string file)
        {
            return new ByteBuf(Resources.Load<TextAsset>($"Luban/{file}").bytes);
        }
    }
}
