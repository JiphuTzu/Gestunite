using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class Gesture : MonoBehaviour
    {
        //public delegate void GestureCallback<T>(T gesture);
        //
        //protected delegate void GestureToFailCallback(GestureState newState);
        /**
		 * Threshold for screen distance they must move to count as valid input 
		 * (not an accidental offset on touch), 
		 * based on 20 pixels on a 252ppi device.
         * Mathf.Round(20 / 252 * Capabilities.screenDPI);
		 */
        private float _defaultSlop = -1;
        public float defaultSlop
        {
            get
            {
                if (_defaultSlop < 0)
                    _defaultSlop = Mathf.Round(20 / 252 * Screen.dpi);
                return _defaultSlop;
            }
        }
        protected GesturesManager _gesturesManager;
        /**
		 * Map (generic object) of tracking touch points, where keys are touch points IDs.
		 */
        protected Dictionary<int, GTouch> _touchesMap = new Dictionary<int, GTouch>();
        protected Vector3 _centralPosition = new Vector3();
        protected Vector3 _centralLocation = new Vector3();
        /**
		 * List of gesture we require to fail.
		 * @see requireGestureToFail()
		 */
        protected Dictionary<Gesture, bool> _gesturesToFail = new Dictionary<Gesture, bool>();
        protected GestureState _pendingRecognizedState;
        protected UnityAction<GestureState> _GestureToFail;

        private GameObject _target;
        /**
		 * FIXME
		 * InteractiveObject (DisplayObject) which this gesture is tracking the actual gesture motion on.
		 * 
		 * <p>Could be some image, component (like map) or the larger view like Stage.</p>
		 * 
		 * <p>You can change the target in the runtime, e.g. you have a gallery
		 * where only one item is visible at the moment, so use one gesture instance
		 * and change the target to the currently visible item.</p>
		 */
        public GameObject target
        {
            get
            {
                return _target;
            }
            set
            {
                GameObject last = this.target;
                if (last == value)
                    return;

                UninstallTarget(last);
                _target = value;
                InstallTarget(_target);
            }
        }
        private GameObject _currentTarget;
        public GameObject currentTarget
        {
            get
            {
                return _currentTarget;
            }
        }

        protected bool _disabled = false;

        /** 
		 * @default true
		 */
        public bool disabled
        {
            get
            {
                return _disabled;

            }
            set
            {
                if (_disabled == value) return;

                _disabled = value;

                if (_disabled == true)
                {
                    if (state == GestureState.POSSIBLE)
                    {
                        SetState(GestureState.FAILED);
                    }
                    else if (state == GestureState.BEGAN || state == GestureState.CHANGED)
                    {
                        SetState(GestureState.CANCELLED);
                    }
                }
            }
        }
        protected GestureState _state = GestureState.POSSIBLE;
        public GestureState state { get { return _state; } }

        protected bool _idle = true;
        internal bool idle { get { return _idle; } }
        protected int _touchID = -1;
        public virtual int touchID { get { return _touchID; } }

        /**
		 * Amount of currently tracked touch points.
		 * 
		 * @see #_touches
		 */
        public uint touchesCount { get { return _touchesCount; } }
        protected uint _touchesCount = 0;

        /// <summary>
        /// 碰撞点的3D位置
        /// </summary>
        public Vector3 location { get { return new Vector3(_location.x, _location.y, _location.z); } }
        protected Vector3 _location = new Vector3();

        /// <summary>
        /// Touch的屏幕值的中心点
        /// </summary>
        public Vector3 position { get { return new Vector3(_position.x, _position.y, _position.z); } }
        protected Vector3 _position = new Vector3();

        private bool _inited;
        public T GetComponentInCurrent<T>()
        {
            if (_currentTarget != null)
            {
                return _currentTarget.GetComponent<T>();
            }
            return default(T);
        }
        public T GetComponentInCurrentChildren<T>()
        {
            if (_currentTarget != null)
            {
                return _currentTarget.GetComponentInChildren<T>();
            }
            return default(T);
        }
        public bool IsTrackingTouch(int touchID)
        {
            //Debug.Log("Gesture.IsTrackingTouch(" + touchID + ") -> ");
            //foreach (KeyValuePair<uint, GTouch> kv in _touchesMap)
            //{
            //Debug.Log("Gesture.touchesMap.key = " + kv.Key);
            //}
            return _touchesMap.ContainsKey(touchID);//(_touchesMap[touchID] != undefined);
        }
        /**
		 * Cancels current tracking (interaction) cycle.
		 * 
		 * <p>Could be useful to "stop" gesture for the current interaction cycle.</p>
		 */
        public virtual void Reset()
        {
            if (idle == true) return;// Do nothing as we are idle and there is nothing to reset

            //Debug.LogError("Gesture.Reset()");
            _position.Set(0.0f, 0.0f, 0.0f);
            _location.Set(0.0f, 0.0f, 0.0f);

            _touchesMap.Clear();
            _touchesCount = 0;
            _idle = true;


            foreach (KeyValuePair<Gesture, bool> kv in _gesturesToFail)
            {
                Gesture gestureToFail = kv.Key;
                gestureToFail._GestureToFail -= GestureToFailStateChangeHandler;
            }
            _pendingRecognizedState = null;
            GestureState state = this.state;//caching getter

            if (state == GestureState.POSSIBLE)
            {
                // manual reset() call. Set to FAILED to keep our State Machine clean and stable
                SetState(GestureState.FAILED);
            }
            else if (state == GestureState.BEGAN || state == GestureState.CHANGED)
            {
                // manual reset() call. Set to CANCELLED to keep our State Machine clean and stable
                SetState(GestureState.CANCELLED);
            }
            else
            {
                // reset from GesturesManager after reaching one of the 4 final states:
                // (state == GestureState.RECOGNIZED ||
                // state == GestureState.ENDED ||
                // state == GestureState.FAILED ||
                // state == GestureState.CANCELLED)
                SetState(GestureState.POSSIBLE);
            }
        }
        /**
		 * Remove gesture and prepare it for GC.
		 * 
		 * <p>The gesture is not able to use after calling this method.</p>
		 */
        public void Dispose()
        {
            //TODO
            Reset();
            target = null;
            //gestureShouldReceiveTouchCallback = null;
            //gestureShouldBeginCallback = null;
            //gesturesShouldRecognizeSimultaneouslyCallback = null;
            _gesturesToFail = null;
        }
        internal virtual bool CanBePreventedByGesture(Gesture preventingGesture)
        {
            return true;
        }
        internal virtual bool CanPreventGesture(Gesture preventedGesture)
        {
            return true;
        }
        /**
		 * <b>NB! Current implementation is highly experimental!</b> See examples for more info. 
		 */
        public void RequireGestureToFail(Gesture gesture)
        {
            //TODO
            if (gesture == null)
            {
                Debug.LogError("can not be null");
            }

            _gesturesToFail[gesture] = true;
        }
        protected virtual void Init()
        {

        }
        protected void InstallTarget(GameObject target)
        {
            if (target != null && _gesturesManager != null)
            {
                //Debug.Log("Gesture.InstallTarget " + _gesturesManager);
                _gesturesManager.AddGesture(this);
            }
        }
        protected void UninstallTarget(GameObject target)
        {
            if (target != null && _gesturesManager != null)
            {
                _gesturesManager.RemoveGesture(this);
            }
        }
        /**
		 * TODO: clarify usage. For now it's supported to call this method in onTouchBegin with return.
		 */
        protected void IgnoreTouch(GTouch touch)
        {
            if (_touchesMap.ContainsKey(touch.id) == true)
            {
                _touchesMap.Remove(touch.id);
                _touchesCount--;
            }
            Debug.Log("Gesture.IgnoreTouch " + _touchesMap.Count);
        }
        protected void FailOrIgnoreTouch(GTouch touch)
        {
            if (state == GestureState.POSSIBLE)
            {
                SetState(GestureState.FAILED);
            }
            else
            {
                IgnoreTouch(touch);
            }
        }
        /**
		 * <p><b>NB!</b> This is abstract method and must be overridden.</p>
		 */
        protected virtual void OnTouchBegin(GTouch touch)
        {
        }

        /**
		 * <p><b>NB!</b> This is abstract method and must be overridden.</p>
		 */
        protected virtual void OnTouchMove(GTouch touch)
        {
        }

        /**
		 * <p><b>NB!</b> This is abstract method and must be overridden.</p>
		 */
        protected virtual void OnTouchEnd(GTouch touch)
        {
        }


        /**
		 * 
		 */
        protected virtual void OnTouchCancel(GTouch touch)
        {
        }
        public bool SetState(GestureState newState)
        {
            if (_state == newState && _state == GestureState.CHANGED)
            {
                // shortcut for better performance
                if (_GestureToFail != null) _GestureToFail(_state);

                ChangedStateCallback(GestureState.CHANGED);

                ResetNotificationProperties();

                return true;
            }

            if (_state.CanTransitionTo(newState) == false)
            {
                //Debug.LogError("You cannot change from state " +
                //_state + " to state " + newState  + ".");
                return false;
            }

            if (newState != GestureState.POSSIBLE)
            {
                // in case instantly switch state in touchBeganHandler()
                _idle = false;
            }


            if (newState == GestureState.BEGAN || newState == GestureState.RECOGNIZED)
            {
                //var gestureToFail:Gesture;
                //var key:*;
                // first we check if other required-to-fail gestures recognized
                // TODO: is this really necessary? using "requireGestureToFail" API assume that
                // required-to-fail gesture always recognizes AFTER this one.
                foreach (KeyValuePair<Gesture, bool> kv in _gesturesToFail)
                {
                    Gesture gestureToFail = kv.Key;
                    if (gestureToFail.idle == false && gestureToFail.state != GestureState.POSSIBLE && gestureToFail.state != GestureState.FAILED)
                    {
                        // Looks like other gesture won't fail,
                        // which means the required condition will not happen, so we must fail
                        SetState(GestureState.FAILED);
                        return false;
                    }
                }
                // then we check if other required-to-fail gestures are actually tracked (not IDLE)
                // and not still not recognized (e.g. POSSIBLE state)
                foreach (KeyValuePair<Gesture, bool> kv in _gesturesToFail)
                {
                    Gesture gestureToFail = kv.Key;
                    if (gestureToFail.state == GestureState.POSSIBLE)
                    {
                        // Other gesture might fail soon, so we postpone state change
                        _pendingRecognizedState = newState;

                        foreach (KeyValuePair<Gesture, bool> kvp in _gesturesToFail)
                        {
                            gestureToFail = kvp.Key;
                            gestureToFail._GestureToFail += GestureToFailStateChangeHandler;
                        }

                        return false;
                    }
                    // else if gesture is in IDLE state it means it doesn't track anything,
                    // so we simply ignore it as it doesn't seem like conflict from this perspective
                    // (perspective of using "requireGestureToFail" API)
                }


                //if (gestureShouldBeginCallback != null && !gestureShouldBeginCallback(this))
                //{
                //SetState(GestureState.FAILED);
                //return false;
                //}
            }

            //GestureState oldState = _state;	
            _state = newState;
            //Debug.LogError("Gesture.SetState(" + _state + ")" + _state.IsEndState);

            if (_state.IsEndState && _gesturesManager != null)
            {
                _gesturesManager.ScheduleGestureStateReset(this);
            }

            //TODO: what if RTE happens in event handlers?
            if (_GestureToFail != null) _GestureToFail(_state);

            ChangedStateCallback(_state);

            ResetNotificationProperties();

            if (_state == GestureState.BEGAN || _state == GestureState.RECOGNIZED)
            {
                if (_gesturesManager != null)
                {
                    _gesturesManager.OnGestureRecognized(this);
                }
            }
            return true;
        }
        protected virtual void ChangedStateCallback(GestureState state)
        {
        }
        protected void UpdateCentralPoint()
        {
            _centralPosition = Vector3.zero;
            _centralLocation = Vector3.zero;

            foreach (KeyValuePair<int, GTouch> kv in _touchesMap)
            {
                _centralPosition += kv.Value.position;
                _centralLocation += kv.Value.location;
            }
            _centralPosition /= _touchesCount;
            _centralLocation /= _touchesCount;
        }
        protected void UpdateLocation()
        {
            UpdateCentralPoint();

            _position.Set(_centralPosition.x, _centralPosition.y, _centralPosition.z);

            _location.Set(_centralLocation.x, _centralLocation.y, _centralLocation.z);
        }
        protected virtual void ResetNotificationProperties()
        {

        }
        internal void TouchBeginHandler(GTouch touch)
        {
            if (_touchesMap.ContainsKey(touch.id) == false)
            {
                _touchesMap.Add(touch.id, touch);
                _currentTarget = touch.target;
                _touchesCount++;
            }
            //Debug.Log("Gesture.TouchBeginHandler() -> " + _touchesMap.Count);

            OnTouchBegin(touch);

            if (_touchesCount == 1 && state == GestureState.POSSIBLE)
            {
                _idle = false;
            }
        }
        internal void TouchMoveHandler(GTouch touch)
        {
            if (_touchesMap.ContainsKey(touch.id) == false)
            {
                _touchesMap.Add(touch.id, touch);
                _touchesCount++;
            }
            //Debug.Log("Gesture.TouchMoveHandler() -> " + _touchesMap.Count);
            OnTouchMove(touch);
        }
        internal void TouchEndHandler(GTouch touch)
        {
            if (_touchesMap.ContainsKey(touch.id) == true)
            {
                _touchesMap.Remove(touch.id);
                _touchesCount--;
            }
            //Debug.Log("Gesture.TouchEndHandler() -> " + _touchesMap.Count);
            OnTouchEnd(touch);
        }
        internal void TouchCancelHandler(GTouch touch)
        {
            if (_touchesMap.ContainsKey(touch.id) == true)
            {
                _touchesMap.Remove(touch.id);
                _touchesCount--;
            }
            //Debug.Log("Gesture.TouchCancelHandler() -> " + _touchesMap.Count);
            OnTouchCancel(touch);

            if (state.IsEndState == false)
            {
                if (state == GestureState.BEGAN || state == GestureState.CHANGED)
                {
                    SetState(GestureState.CANCELLED);
                }
                else
                {
                    SetState(GestureState.FAILED);
                }
            }
        }
        protected void GestureToFailStateChangeHandler(GestureState newState)
        {
            if (_pendingRecognizedState == null || state != GestureState.POSSIBLE)
                return;

            if (newState == GestureState.FAILED)
            {
                foreach (KeyValuePair<Gesture, bool> kv in _gesturesToFail)
                {
                    Gesture gestureToFail = kv.Key;
                    if (gestureToFail.state == GestureState.POSSIBLE)
                    {
                        // we're still waiting for some gesture to fail
                        return;
                    }
                }

                // at this point all gestures-to-fail are either in IDLE or in FAILED states
                SetState(_pendingRecognizedState);
            }
            else if (newState != GestureState.POSSIBLE)
            {
                //TODO: need to re-think this over

                SetState(GestureState.FAILED);
            }
        }
        private void CheckInit()
        {
            if (_inited == false)
            {
                //Debug.Log("Gesture.CheckInit() set target " + gameObject);
                _gesturesManager = Gestunite.GetInstance().gesturesManager;
                this.target = gameObject;
                Init();
                _inited = true;
            }
        }
        private void Awake()
        {
            // Debug.Log("Gesture.Awake()");
            CheckInit();
        }
        private void OnEnable()
        {
            //Debug.Log("Gesture.OnEnable");
            CheckInit();
        }
    }
}