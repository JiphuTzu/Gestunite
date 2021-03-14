using UnityEngine;
using System.Collections;
namespace Gestzu.Core
{
    public abstract class HitTester:MonoBehaviour
    {
        public abstract RaycastHit HitTest();
        public abstract void SetLayerMask(int layerMask = Physics.DefaultRaycastLayers);
        public abstract RaycastHit HitTest(Vector3 point, bool worldPosition);
        public abstract Ray GetRay();
        public abstract void SetCamera(Camera cam);
        public abstract Camera GetCamera();
    }
}