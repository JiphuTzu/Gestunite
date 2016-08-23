using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gestzu.Gestures;

namespace Gestzu.Core
{
    public class GesturesManager
    {
        protected GameObject _frameTickerShape;
        protected Dictionary<Gesture, bool> _gesturesMap = new Dictionary<Gesture, bool>();
        protected Dictionary<GTouch, List<Gesture>> _gesturesForTouchMap = new Dictionary<GTouch, List<Gesture>>();
        protected Dictionary<GameObject, List<Gesture>> _gesturesForTargetMap = new Dictionary<GameObject, List<Gesture>>();
        protected Dictionary<Gesture, bool> _dirtyGesturesMap = new Dictionary<Gesture, bool>();
        protected uint _dirtyGesturesCount;
        private Coroutine _delayResetDirtyGestures;
        //protected var _stage:Stage;
        public List<GameObject> roots;
        public GesturesManager()
        {
        }
        protected void ResetDirtyGestures()
        {
            foreach (Gesture gesture in _dirtyGesturesMap.Keys)
            {
                gesture.Reset();
            }
            _dirtyGesturesMap.Clear();
            _dirtyGesturesCount = 0;
            if (_delayResetDirtyGestures != null)
            {
                Gestunite.GetInstance().StopCoroutine(_delayResetDirtyGestures);
                _delayResetDirtyGestures = null;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="gesture">Gesture</param>
        internal void AddGesture(Gesture gesture)
        {
            if (gesture == null)
            {
                Debug.LogError("Argument 'gesture' must be not null.");
            }

            GameObject target = gesture.target;
            if (target == null)
            {
                Debug.LogError("Gesture must have target.");
            }

            List<Gesture> targetGestures;
            if (_gesturesForTargetMap.ContainsKey(target) == true)
            {
                targetGestures = _gesturesForTargetMap[target];
                if (targetGestures.IndexOf(gesture) < 0)
                {
                    targetGestures.Add(gesture);
                }
            }
            else
            {
                targetGestures = new List<Gesture>();
                targetGestures.Add(gesture);
                _gesturesForTargetMap[target] = targetGestures;
            }

            if (_gesturesMap.ContainsKey(gesture) == false)
            {
                _gesturesMap.Add(gesture, true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gesture">Gesture</param>
        internal void RemoveGesture(Gesture gesture)
        {
            if (gesture == null)
            {
                Debug.LogError("Argument 'gesture' must be not null.");
            }


            GameObject target = gesture.target;
            // check for target because it could be already GC-ed (since target reference is weak)
            if (target != null)
            {
                if (_gesturesForTargetMap.ContainsKey(target) == true)
                {
                    List<Gesture> targetGestures = _gesturesForTargetMap[target];
                    if (targetGestures.Count > 1)
                    {
                        targetGestures.Remove(gesture);
                    }
                    else
                    {
                        _gesturesForTargetMap.Remove(target);
                    }
                }
            }

            if (_gesturesMap.ContainsKey(gesture) == true)
            {
                _gesturesMap.Remove(gesture);
            }

            gesture.Reset();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gesture">Gesture</param>
        internal void ScheduleGestureStateReset(Gesture gesture)
        {
            //Debug.LogWarning("ScheduleGestureStateReset");
            if (_dirtyGesturesMap.ContainsKey(gesture) == false)
            {
                _dirtyGesturesMap.Add(gesture, true);
                _dirtyGesturesCount++;
                if (_delayResetDirtyGestures == null)
                {
                    _delayResetDirtyGestures = Gestunite.GetInstance().StartCoroutine(DelayResetDirtyGestures());
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gesture">Gesture</param>
        internal void OnGestureRecognized(Gesture gesture)
        {
            GameObject target = gesture.target;

            foreach (KeyValuePair<Gesture, bool> kv in _gesturesMap)
            {
                Gesture otherGesture = kv.Key;
                GameObject otherTarget = otherGesture.target;
                //Debug.Log("OnGestureRecognized " + (otherGesture != gesture) + " " + otherGesture.enabled + " " + otherGesture.state);
                //Debug.Log("--->" + target + " " + otherTarget);
                // conditions for otherGesture "own properties"
                if (otherGesture != gesture &&
                    target != null && otherTarget != null &&//in case GC worked half way through
                    otherGesture.disabled == false &&
                    otherGesture.state == GestureState.POSSIBLE)
                {
                    if (otherTarget == target)
                    {
                        // conditions for gestures relations
                        if (gesture.CanPreventGesture(otherGesture) &&
                            otherGesture.CanBePreventedByGesture(gesture)
                            //&&
                            //(gesture.gesturesShouldRecognizeSimultaneouslyCallback == null ||
                            // !gesture.gesturesShouldRecognizeSimultaneouslyCallback(gesture, otherGesture)) &&
                            //(otherGesture.gesturesShouldRecognizeSimultaneouslyCallback == null ||
                            //!otherGesture.gesturesShouldRecognizeSimultaneouslyCallback(otherGesture, gesture))
                            )
                        {
                            otherGesture.SetState(GestureState.FAILED);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="touch">Touch</param>
        internal void OnTouchBegin(GTouch touch)
        {
            Gesture gesture;
            int i;

            //// This vector will contain active gestures for specific touch during all touch session.
            List<Gesture> gesturesForTouch;
            if (_gesturesForTouchMap.ContainsKey(touch) == false)
            {
                gesturesForTouch = new List<Gesture>();
                _gesturesForTouchMap.Add(touch, gesturesForTouch);
            }
            else
            {
                gesturesForTouch = _gesturesForTouchMap[touch];
                // touch object may be pooled in the future
                gesturesForTouch.Clear();
            }

            //Debug.Log("gesture -- " + target);

            //// Create a sorted(!) list of gestures which are interested in this touch.
            //// Sorting priority: deeper target has higher priority, recently added gesture has higher priority.
            List<Gesture> gesturesForTarget;
            BubbleToRoot(touch.target, out gesturesForTarget);
            Debug.Log(">>>>>>>>" + gesturesForTarget.Count);
            //if (target != null)
            //{
            //gesturesForTarget = _gesturesForTargetMap[target];
            i = gesturesForTarget.Count;
            while (--i >= 0)
            {
                gesture = gesturesForTarget[i];
                //Debug.Log("gestures For target " + i + " --> " + gesture.target);
                if (gesture.disabled == false
                //&& (gesture.gestureShouldReceiveTouchCallback == null ||
                //gesture.gestureShouldReceiveTouchCallback(gesture, touch))
                )
                {
                    //TODO: optimize performance! decide between unshift() vs [i++] = gesture + reverse()
                    gesturesForTouch.Insert(0, gesture);
                }
            }
            //}

            //// Then we populate them with this touch and event.
            //// They might start tracking this touch or ignore it (via Gesture#ignoreTouch())
            i = gesturesForTouch.Count;
            while (--i >= 0)
            {
                gesture = gesturesForTouch[i];
                // Check for state because previous (i+1) gesture may already abort current (i) one
                if (_dirtyGesturesMap.ContainsKey(gesture) == false)
                {
                    gesture.TouchBeginHandler(touch);
                }
                else
                {
                    gesturesForTouch.RemoveAt(i);
                }
            }
            //Debug.Log("GesturesManager.OnTouchBegin() -> " + gesturesForTouch.Count);
        }
        private void BubbleToRoot(GameObject target, out List<Gesture> gesturesFroTouch)
        {
            gesturesFroTouch = new List<Gesture>();
            while (target != null)
            {
                if (_gesturesForTargetMap.ContainsKey(target) == true)
                {
                    gesturesFroTouch.AddRange(_gesturesForTargetMap[target]);
                    //return target;
                }
                if (roots.Contains(target) == true)
                {
                    break;
                }
                if (target.transform.parent != null)
                {
                    target = target.transform.parent.gameObject;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="touch">Touch</param>
        internal void OnTouchMove(GTouch touch)
        {
            List<Gesture> gesturesForTouch = _gesturesForTouchMap[touch];
            Gesture gesture;
            int i = gesturesForTouch.Count;
            while (--i >= 0)
            {
                gesture = gesturesForTouch[i];
                //Debug.Log("dirtyGesturesMap.ContainesKey(" + gesture + ") -> " + _dirtyGesturesMap.ContainsKey(gesture) + " gesture.IsTrackingTouch(" + touch.id + ") -> " + gesture.IsTrackingTouch(touch.id));
                if (_dirtyGesturesMap.ContainsKey(gesture) == false && gesture.IsTrackingTouch(touch.id))
                {
                    gesture.TouchMoveHandler(touch);
                }
                else
                {
                    // gesture is no more interested in this touch (e.g. ignoreTouch was called)
                    gesturesForTouch.RemoveAt(i);
                }
            }
            //Debug.Log("GesturesManager.OnTouchMove() -> " + gesturesForTouch.Count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="touch">Touch</param>
        internal void OnTouchEnd(GTouch touch)
        {
            //Debug.Log("GestureManager.OnTouchEnd(" + touch.id + ")");
            List<Gesture> gesturesForTouch = _gesturesForTouchMap[touch];
            Gesture gesture;
            int i = gesturesForTouch.Count;
            while (--i >= 0)
            {
                gesture = gesturesForTouch[i];
                //Debug.Log("dirtyGesturesMap.ContainesKey(" + gesture + ") -> " + _dirtyGesturesMap.ContainsKey(gesture) + "gesture.IsTrackingTouch(" + touch.id + ") -> " + gesture.IsTrackingTouch(touch.id));
                if (_dirtyGesturesMap.ContainsKey(gesture) == false && gesture.IsTrackingTouch(touch.id))
                {
                    gesture.TouchEndHandler(touch);
                }
            }
            //Debug.Log("GesturesManager.OnTouchEnd() -> " + gesturesForTouch.Count);

            gesturesForTouch.Clear();// release for GC

            _gesturesForTouchMap.Remove(touch);//TODO: remove this once Touch objects are pooled
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="touch">Touch</param>
        internal void OnTouchCancel(GTouch touch)
        {
            List<Gesture> gesturesForTouch = _gesturesForTouchMap[touch];
            Gesture gesture;
            int i = gesturesForTouch.Count;
            while (--i > 0)
            {
                gesture = gesturesForTouch[i];

                if (_dirtyGesturesMap.ContainsKey(gesture) == false && gesture.IsTrackingTouch(touch.id))
                {
                    gesture.TouchCancelHandler(touch);
                }
            }

            gesturesForTouch.Clear();// release for GC

            _gesturesForTouchMap.Remove(touch);//TODO: remove this once Touch objects are pooled
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator DelayResetDirtyGestures()
        {
            yield return new WaitForFixedUpdate();
            _delayResetDirtyGestures = null;
            ResetDirtyGestures();
        }
    }
}
