using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class TapGesture : AbstractDiscreteGesture<TapGesture>
    {
        public uint numTapsRequired = 1;
        public uint numTouchesRequired = 1;
        public float maxTapDelay = 0.4f;
        public float maxTapDuration = 1.5f;

        private float _maxTapDistance = -1;
        public float maxTapDistance
        {
            get
            {
                if(_maxTapDistance<0)
                    _maxTapDistance = defaultSlop;
                return _maxTapDistance;
            }
        }
        private float _slop = -1;//iOS has 45px for 132 dpi screen
        public float slop
        {
            get
            {
                if(_slop<0)
                    _slop = defaultSlop;
                return _slop;
            }
        }
        private Coroutine _timer;
        private bool _numTouchesRequiredReached;
        private uint _tapCounter = 0;
        private List<Vector3> _touchBeginLocations = new List<Vector3>();
        /// ================================================
        /// public methods
        /// ================================================
        public override string ToString()
        {
            return "TapGesture(" + numTapsRequired + ")";
        }
        public override void Reset()
        {
            _numTouchesRequiredReached = false;
            _tapCounter = 0;
            StopTimer();
            //_timer.reset();
            _touchBeginLocations.Clear();

            base.Reset();
        }
        internal override bool CanPreventGesture(Gesture preventedGesture)
        {
            //Debug.Log(this+" CanPreventGesture " + preventedGesture);
            if (preventedGesture is TapGesture && (preventedGesture as TapGesture).numTapsRequired > this.numTapsRequired)
            {
                //Debug.Log(this+" CanPreventGesture " + false);
                return false;
            }
            //Debug.Log(this+" CanPreventGesture " + true);
            return true;
        }

        //protected override void Init()
        //{
        //base.Init();
        //Debug.Log("TapGesture.Init()");
        //_timer = new Timer(maxTapDelay, 1);
        //_timer.addEventListener(TimerEvent.TIMER_COMPLETE, timer_timerCompleteHandler);
        //}
        protected override void OnTouchBegin(GTouch touch)
        {
            //Debug.Log("TapGesture.OnTouchBegin() -> " + touchesCount + " | " + numTouchesRequired);
            if (touchesCount > numTouchesRequired)
            {
                FailOrIgnoreTouch(touch);
                return;
            }

            if (touchesCount == 1)
            {
                StopTimer();
                StartTimer(maxTapDuration);
                //_timer.reset();
                //_timer.delay = maxTapDuration;
                //_timer.start();
            }

            if (numTapsRequired > 1)
            {
                if (_tapCounter == 0)
                {
                    // Save touch begin locations to check
                    _touchBeginLocations.Add(touch.position);
                }
                else
                {
                    // Quite a dirty check, but should work in most cases
                    bool found = false;
                    int i = _touchBeginLocations.Count;
                    Vector2 touchLocation = touch.position;
                    while (--i >= 0)
                    {
                        if (Vector2.Distance(touchLocation, _touchBeginLocations[i]) < maxTapDistance)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found == false)
                    {
                        SetState(GestureState.FAILED);
                        return;
                    }
                }
            }

            if (touchesCount == numTouchesRequired)
            {
                _numTouchesRequiredReached = true;
                UpdateLocation();
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            if (slop >= 0 && touch.positionOffset.magnitude > slop)
            {
                SetState(GestureState.FAILED);
            }
        }
        protected override void OnTouchEnd(GTouch touch)
        {
            if (_numTouchesRequiredReached == false)
            {
                //Debug.Log("TapGesture.OnTouchEnd() -> numTouchesRequiredReached = " + _numTouchesRequiredReached);
                SetState(GestureState.FAILED);
            }
            else if (touchesCount == 0)
            {
                // reset flag for the next "full press" cycle
                _numTouchesRequiredReached = false;

                _tapCounter++;
                StopTimer();
                //_timer.reset();
                //Debug.Log("TapGesture.OnTouchEnd() -> tapCounter = "+_tapCounter+" numTapsRequired = "+numTapsRequired );
                if (_tapCounter == numTapsRequired)
                {
                    //识别之前给赋值
                    _touchID = touch.id;
                    SetState(GestureState.RECOGNIZED);
                }
                else
                {
                    StartTimer(maxTapDelay);
                    //_timer.delay = maxTapDelay;
                    //_timer.start();
                }
            }
            else
            {
                Debug.Log("TapGesture.OnTouchEnd() -> touchesCount = " + _touchesCount);
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
                SetState(GestureState.FAILED);
            }
        }
    }
}