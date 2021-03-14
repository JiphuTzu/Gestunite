using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Gestzu.Core;
//============================================================
//@author	JiphuTzu
//@create	4/22/2016
//@company	SOBF
//
//@description:Ìí¼Ó¹öÂÖ¼ì²â
//============================================================
namespace Gestzu.Gestures
{
	public class WheelGesture : MonoBehaviour {
        public event UnityAction<WheelGesture> OnWheel;
        //
        public string wheelName = "Mouse ScrollWheel";
        public float sensitivity = 0.01F;
        private HitTester _hitTester;
        /**
		 * FIXME
		 */
        public GameObject target
        {
            get
            {
                return gameObject;
            }
        }
        private float _delta;
        public float delta
        {
            get
            {
                return _delta;
            }
        }
		private void Update () {
            if (OnWheel == null) return;
            float d = Input.GetAxis(wheelName);
            if (Mathf.Abs(d) < sensitivity) return;
            if (_hitTester == null)
            {
                _hitTester = Gestunite.GetInstance().GetComponent<HitTester>();
            }
            RaycastHit result = _hitTester.HitTest();
            if (result.collider == null)
            {
                if (gameObject == GetActiveRoot())
                {
                    _delta = d;
                    OnWheel(this);
                }
                return;
            }
            if (WheelOnMe(result.collider.gameObject) == true)
            {
                _delta = d;
                OnWheel(this);
            }
		}
        private GameObject GetActiveRoot()
        {
            List<GameObject> roots = Gestunite.GetInstance().roots;
            for(int i = 0; i < roots.Count; i++)
            {
                GameObject root = roots[i];
                if (root.activeSelf == true) return root;
            }
            return null;
        }
        private bool WheelOnMe(GameObject target)
        {
            while (target != null)
            {
                if (target == gameObject) return true;
                if (target.transform.parent != null)
                {
                    target = target.transform.parent.gameObject;
                }
                else
                {
                    target = null;
                }
            }
            return false;
        }
	}
}