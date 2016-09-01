using UnityEngine;
using UnityEngine.EventSystems;
using Gestzu.Core;
//============================================================
//@author	JiphuTzu
//@create	7/29/2016
//@company	STHX
//
//@description:
//============================================================
namespace Gestzu.Inputs
{
    public class TouchInputAdapter : InputAdapter
    {
        public override void Init()
        {
            //³õÊ¼»¯
        }

        private void Update()
        {
            //在uGUI上
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject() == true) return;
            for (int i = 0; i < Input.touchCount; ++i)
            {
                CheckTouchIndex(Input.touches[i]);
            }
        }

        private void CheckTouchIndex(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _touchesManager.OnTouchBegin(touch.fingerId, touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                _touchesManager.OnTouchMove(touch.fingerId, touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _touchesManager.OnTouchEnd(touch.fingerId, touch.position);
            }
        }
    }
}
