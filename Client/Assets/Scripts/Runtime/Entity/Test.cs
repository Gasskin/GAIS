using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime
{
    public class Test: MonoBehaviour
    {
        private void Start()
        {
            EntityFactory.CreateTestEntity().Forget();
        }
    }
}
