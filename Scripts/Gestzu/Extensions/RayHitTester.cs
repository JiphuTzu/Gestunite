using UnityEngine;
using System.Collections;
using Gestzu.Core;
using System;

namespace Gestzu.Extensions
{
    public class RayHitTester : HitTester
    {
        [SerializeField]
        private Camera _camera;
        public bool debug;
        private RaycastHit _result = new RaycastHit();
        private Ray _ray;
        private LayerMask _layerMask = Physics.DefaultRaycastLayers;
        public override void SetCamera(Camera cam)
        {
            _camera = cam;
        }
        public override Camera GetCamera()
        {
            return _camera;
        }
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
        public override RaycastHit HitTest(Vector3 position,bool worldPosition)
        {
            RaycastHit result = new RaycastHit();
            if (worldPosition == true) position = _camera.WorldToScreenPoint(position);
            Raycast(position, out result);
            return result;
        }
        private void Update()
        {
            _ray = Raycast(Input.mousePosition, out _result);
        }
        private Ray Raycast(Vector3 position, out RaycastHit result)
        {
            if (_camera == null) _camera = Camera.main;
            Ray ray = _camera.ScreenPointToRay(position);
            // regular 3D raycast
            bool hit = Physics.Raycast(ray, out result, Mathf.Infinity,_layerMask);
            //#if UNITY_EDITOR
            // vizualise ray
            if (debug==true)
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