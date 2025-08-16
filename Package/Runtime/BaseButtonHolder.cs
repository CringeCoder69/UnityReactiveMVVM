using R3;
using UnityEngine;

namespace UnityReactiveMVVM
{
    public abstract class BaseButtonHolder : MonoBehaviour
    {
        public abstract Observable<Unit> Clicked { get; }
    }
}
