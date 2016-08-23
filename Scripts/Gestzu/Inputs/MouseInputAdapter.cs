using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Gestzu.Core;

namespace Gestzu.Inputs
{
    public class MouseInputAdapter : InputAdapter
    {

        public override void Init()
        {
            //初始化
        }
        private void Awake()
        {
            Debug.Log("mouse input adapter .awake()");
        }
        //public void Dispose()
        //{
        //销毁
        //}

        private void Update()
        {
            //在uGUI上
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject() == true) return;
            CheckButtonIndex(0);
            CheckButtonIndex(1);
            CheckButtonIndex(2);
        }
        protected void CheckButtonIndex(int index)
        {
            if (Input.GetMouseButtonDown(index) == true)
            {
                _touchesManager.OnTouchBegin(index, Input.mousePosition);
            }
            else if (Input.GetMouseButton(index) == true)
            {
                _touchesManager.OnTouchMove(index, Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(index) == true)
            {
                _touchesManager.OnTouchEnd(index, Input.mousePosition);
            }
        }
    }
}