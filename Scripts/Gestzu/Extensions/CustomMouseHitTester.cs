using UnityEngine;
using System.Collections;
using Gestzu.Core;
using System;
//============================================================
//@author	JiphuTzu
//@create	4/15/2016
//@company	SOBF
//
//@description:
//============================================================
namespace Gestzu.Extensions
{
	public class CustomMouseHitTester : HitTester {
        private Camera _camera;
        public override void SetCamera(Camera cam)
        {
            _camera = cam;
        }
        public override Camera GetCamera()
        {
            return _camera;
        }
        public GameObject cursorPrefab;
        public Vector3 rotationOffset = new Vector3(310.0F, 120.0F, 0.0F);
        public float defaultCursorDistance = 30.0F;
        public float defaultScale = 0.8F;
        public bool debug;
        //
        private GameObject _cursor;
        private RaycastHit _result = new RaycastHit();
        private Ray _ray;
        [SerializeField]
        private LayerMask _layerMask = Physics.DefaultRaycastLayers;
        public override Ray GetRay()
        {
            return _ray;
        }
        public override RaycastHit HitTest()
        {
            return _result;
        }
        public override void SetLayerMask(int layerMask = Physics.DefaultRaycastLayers)
        {
            _layerMask = layerMask;
        }
        public override RaycastHit HitTest(Vector3 position, bool worldPosition)
        {
            RaycastHit result = new RaycastHit();
            Raycast(position, out result,false);
            return result;
        }
        private void Start()
        {
            if (cursorPrefab == null)
            {
                Debug.LogWarning("需要设置一个代替光标的预制体");
            }
            else
            {
                Cursor.visible = false;
                _cursor = Instantiate(cursorPrefab);
                _cursor.transform.SetParent(transform, false);
                _cursor.transform.rotation = Quaternion.Euler(rotationOffset);
                _cursor.transform.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
                _cursor.name = "cursor";
            }
        }
        private void Update()
        {
            _ray = Raycast(Input.mousePosition, out _result,true);
        }
        private Ray Raycast(Vector3 position, out RaycastHit result,bool isMouse)
        {
            Ray ray = _camera.ScreenPointToRay(position);
            // regular 3D raycast
            bool hit = Physics.Raycast(ray, out result, Mathf.Infinity,_layerMask);
            if (isMouse == false) return ray;
            if (hit == true)
            {
                _cursor.transform.position = result.point;
            }
            else
            {
                position.z = defaultCursorDistance;
                _cursor.transform.position = ray.origin + ray.direction * defaultCursorDistance;
            }
            //#if UNITY_EDITOR
            // vizualise ray
            if (debug == true)
            {
                //lineRenderer.SetPosition(0, ray.origin+new Vector3(0.0f,-0.3f));
                if (hit)
                {
                    Vector3 hitPos = result.point;

                    Debug.DrawLine(ray.origin, hitPos, Color.green, 0.5f);
                    //lineRenderer.SetPosition(1, hitPos);
                }
                else
                {
                    //Debug.Log("Raycase " +debug+" -> "+ ray);
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * 9999.0f, Color.red, 0.5f);
                    //lineRenderer.SetPosition(1, ray.origin + ray.direction * 50);
                }
            }
            //#endif
            return ray;
        }
    }
}