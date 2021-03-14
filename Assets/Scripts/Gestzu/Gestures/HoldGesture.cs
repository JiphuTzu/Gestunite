using UnityEngine;
using System.Collections;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class HoldGesture : AbstractContinuousGesture<HoldGesture>
    {
        public uint numTouchesRequired = 1;
        /**
		 * The minimum time interval in second fingers must press on the target for the gesture to be recognized.
		 * 
		 * @default 0.5f
		 */
        public float minPressDuration = 0.5f;
        private float _slop = -1;
        public float slop
        {
            get
            {
                if (_slop < 0) _slop = defaultSlop;
                return _slop;
            }
        }
        protected Coroutine _timer;
        protected bool _numTouchesRequiredReached;
        protected bool _holding = false;
        public override void Reset()
        {
            base.Reset();
            _numTouchesRequiredReached = false;
            StopTimer();
            _holding = false;
        }
        protected override void OnTouchBegin(GTouch touch)
        {
            if (touchesCount > numTouchesRequired)
            {
                FailOrIgnoreTouch(touch);
                return;
            }

            if (touchesCount == numTouchesRequired)
            {
                //识别到先赋值
                _touchID = touch.id;
                _numTouchesRequiredReached = true;
                Debug.Log("hold ready....");
                StartTimer(minPressDuration);
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            //Debug.Log("state = "+state+"; length = "+touch.locationOffset.magnitude+" slop "+slop);
            if (state == GestureState.POSSIBLE && slop > 0.0f && touch.positionOffset.magnitude > slop)
            {
                SetState(GestureState.FAILED);
            }
            else if (state == GestureState.BEGAN || state == GestureState.CHANGED)
            {
                if (slop > 0.0f && touch.positionOffset.magnitude > slop)
                {
                    SetState(GestureState.ENDED);
                }
                else
                {
                    UpdateLocation();
                    SetState(GestureState.CHANGED);
                }
            }
        }
        protected override void OnTouchEnd(GTouch touch)
        {
            if (_numTouchesRequiredReached == true)
            {
                if (state == GestureState.BEGAN || state == GestureState.CHANGED)
                {
                    UpdateLocation();
                    SetState(GestureState.ENDED);
                }
                else
                {
                    SetState(GestureState.FAILED);
                }
            }
            else
            {
                SetState(GestureState.FAILED);
            }
        }
        private void StopTimer()
        {
            if (_timer != null)
            {
                StopCoroutine(_timer);
                _timer = null;
            }
        }
        private void StartTimer(float time)
        {
            _timer = StartCoroutine(TimerComplete(time));
        }
        private IEnumerator TimerComplete(float time)
        {
            yield return new WaitForSeconds(time);
            if (state == GestureState.POSSIBLE)
            {
                UpdateLocation();
                SetState(GestureState.BEGAN);
                _holding = true;
            }
        }
        protected void FixedUpdate()
        {
            if (_holding == false) return;
            SetState(GestureState.CHANGED);
        }
    }
}