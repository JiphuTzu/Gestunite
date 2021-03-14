using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Gestzu.Core;

namespace Gestzu.Gestures
{
    public class AbstractDiscreteGesture<T> : Gesture where T:Gesture
    {
        public event UnityAction<T> OnRecognized;
        protected override void ChangedStateCallback(GestureState state)
        {
            if (state == GestureState.RECOGNIZED && OnRecognized != null)
            {
                OnRecognized(this as T);
            }
        }
	}
}