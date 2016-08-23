using UnityEngine;
using System.Collections;
namespace Gestzu.Core
{
    public abstract class InputAdapter:MonoBehaviour
    {
        protected TouchesManager _touchesManager;
        public TouchesManager touchesManager
        {
            set
            {
                _touchesManager = value;
                Debug.Log("input adapter.touchManager = "+_touchesManager);
            }
        }
        public abstract void Init();
    }
}

