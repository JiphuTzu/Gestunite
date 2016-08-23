using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace Gestzu.Core
{
    public class TouchesManager
    {
        protected GesturesManager _gesturesManager;
        protected Dictionary<int, GTouch> _touchesMap = new Dictionary<int, GTouch>();
        protected HitTester _hitTester;
        protected uint _activeTouchesCount;
        protected List<GameObject> _roots;
        uint activeTouchesCount { get { return _activeTouchesCount; } }
        public TouchesManager(GesturesManager manager)
        {
            _gesturesManager = manager;
        }
        public List<GTouch> GetTouches()
        {
            List<GTouch> touches = new List<GTouch>();
            foreach (KeyValuePair<int, GTouch> kv in _touchesMap)
            {
                touches.Add(kv.Value);
            }
            return touches;
        }
        internal void SetHitTester(HitTester tester)
        {
            if (tester == null)
            {
                Debug.LogError("Argument must be non null.");
            }

            _hitTester = tester;
        }
        internal void SetRoots(List<GameObject> roots)
        {
            _roots = roots;
            _gesturesManager.roots = roots;
            //Debug.Log("TouchedManager.SetRoot(" + _root + ")");
        }
        private GameObject GetActiveRoot()
        {
            for(int i = 0; i < _roots.Count; i++)
            {
                GameObject root = _roots[i];
                if (root.activeSelf == true)
                {
                    return root;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchID"></param>
        /// <param name="location"></param>
        /// <param name="possibleTarget"></param>
        /// <returns></returns>
        internal bool OnTouchBegin(int touchID, Vector3 position)
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject() == true)
            {
                //在uGUI上
                return false;
            }
            //Debug.Log("TouchesManagers.OnTouchBegin");
            if (_touchesMap.ContainsKey(touchID) == true)
            {
                Debug.Log("touch with specified ID is already registered and being tracked");
                return false;
            }

            foreach (KeyValuePair<int, GTouch> registeredTouch in _touchesMap)
            {
                // Check if touch at the same location exists.
                // In case we listen to both TouchEvents and MouseEvents, one of them will come first
                // (right now looks like MouseEvent dispatched first, but who know what Adobe will
                // do tomorrow). This check helps to filter out the one comes after.

                // NB! According to the tests with some IR multitouch frame and Windows computer
                // TouchEvent comes first, but the following MouseEvent has slightly offset location
                // (1px both axis). That is why Point#distance() used instead of Point#equals()

                if (Vector3.Distance(registeredTouch.Value.position, position) < 2)
                {
                    Debug.Log("touch at the same location exists");
                    return false;
                }
            }

            GTouch touch = CreateTouch();
            touch.id = touchID;

            RaycastHit result = _hitTester.HitTest();
            //Debug.Log("hit result " + result.point + " " + location);
            if (result.collider == null)
            {
                touch.target = GetActiveRoot();
                touch.SetLocation(position, Time.realtimeSinceStartup, position);
            }
            else
            {
                touch.target = result.collider.gameObject;
                //touch.SetLocation(result.point, Time.realtimeSinceStartup);
                touch.SetLocation(position, Time.realtimeSinceStartup, result.point);
            }

            _touchesMap[touchID] = touch;
            _activeTouchesCount++;

            _gesturesManager.OnTouchBegin(touch);

            return true;
        }
        internal void OnTouchMove(int touchID, Vector3 position)
        {
            //Debug.Log("TouchesManagers.OnTouchMove");
            if (_touchesMap.ContainsKey(touchID) == false)
                return;// touch with specified ID isn't registered
            GTouch touch = _touchesMap[touchID];
            RaycastHit result = _hitTester.HitTest(position,false);
            Vector3 location;
            if (result.collider != null)
            {
                location = result.point;
            }
            else
            {
                location = position;
            }
            if (touch.UpdateLocation(position, Time.realtimeSinceStartup, location))
            {
                // NB! It appeared that native TOUCH_MOVE event is dispatched also when
                // the location is the same, but size has changed. We are only interested
                // in location at the moment, so we shall ignore irrelevant calls.

                _gesturesManager.OnTouchMove(touch);
            }
        }
        internal void OnTouchEnd(int touchID, Vector3 position)
        {

            if (_touchesMap.ContainsKey(touchID) == false)
                return;// touch with specified ID isn't registered
            GTouch touch = _touchesMap[touchID];
            RaycastHit result = _hitTester.HitTest();
            Vector3 location;
            if (result.collider != null)
            {
                location = result.point;
            }
            else
            {
                location = position;
            }
            touch.UpdateLocation(position, Time.realtimeSinceStartup, location);

            _touchesMap.Remove(touchID);
            _activeTouchesCount--;

            _gesturesManager.OnTouchEnd(touch);

            touch.target = null;
        }
        internal void OnTouchCancel(int touchID, Vector2 position)
        {
            //Debug.Log("TouchesManagers.OnTouchCancel");
            if (_touchesMap.ContainsKey(touchID) == false)
                return;// touch with specified ID isn't registered
            GTouch touch = _touchesMap[touchID];
            touch.UpdateLocation(position, Time.realtimeSinceStartup, position);

            _touchesMap.Remove(touchID);
            _activeTouchesCount--;

            _gesturesManager.OnTouchCancel(touch);

            touch.target = null;
        }
        protected GTouch CreateTouch()
        {
            //TODO: pool
            return new GTouch();
        }
    }
}