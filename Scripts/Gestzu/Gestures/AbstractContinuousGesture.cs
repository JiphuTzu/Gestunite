using UnityEngine.Events;
using System.Collections;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class  AbstractContinuousGesture<T> : Gesture where T:Gesture {
        public event UnityAction<T> OnBegan;
        public event UnityAction<T> OnEnded;
        public event UnityAction<T> OnChanged;
        public event UnityAction<T> OnCancelled;
        protected override void ChangedStateCallback(GestureState state)
        {
            if (state == GestureState.BEGAN)
            {
                if (OnBegan != null) OnBegan(this as T);
            }
            else if (state == GestureState.ENDED)
            {
                if (OnEnded != null) OnEnded(this as T);
            }
            else if (state == GestureState.CHANGED)
            {
                if (OnChanged != null) OnChanged(this as T);
            }
            else if (state == GestureState.CANCELLED)
            {
                if (OnCancelled != null) OnCancelled(this as T);
            }
        }
	}
}