using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Gestzu.Gestures;
using System;
//============================================================
//@author	JiphuTzu
//@create	7/29/2016
//@company	STHX
//
//@description:
//============================================================
namespace Utzu
{
    public class Example : MonoBehaviour
    {
        public Text debugText;
        public PanGesture singleFingerRootPan;
        public PanGesture multiFingerRootPan;
        public ZoomGesture rootZoom;
        public TapGesture rootTap;
        public TapGesture cubeTap;
        public PanGesture singleSpherePan;
        public PanGesture multiCylinderPan;
        public ZoomGesture capsuleZoom;
        private void Awake()
        {
            singleFingerRootPan.OnChanged += OnCameraRotate;
            multiFingerRootPan.OnChanged += OnCameraUpDown;
            rootZoom.OnChanged += OnCameraZoom;
            rootTap.OnRecognized += OnReset;
            cubeTap.OnRecognized += OnCubeTap;
            singleSpherePan.OnBegan += OnSpherePanBegin;
            singleSpherePan.OnChanged += OnSpherePan;
            singleSpherePan.OnEnded += OnSpherePanEnd;
            capsuleZoom.OnBegan += OnCapsuleZoomBegin;
            capsuleZoom.OnChanged += OnCapsuleZoom;
            capsuleZoom.OnEnded += OnCapsuleZoomEnd;
        }

        private void OnCapsuleZoomEnd(ZoomGesture gesture)
        {
            debugText.text = "OnCapsuleZoomEnd" + gesture.scaleX + "," + gesture.scaleY + "\n" + debugText.text;
        }

        private void OnCapsuleZoom(ZoomGesture gesture)
        {
            debugText.text = "OnCapsuleZoom" + gesture.scaleX + "," + gesture.scaleY + "\n" + debugText.text;
        }

        private void OnCapsuleZoomBegin(ZoomGesture gesture)
        {
            debugText.text = "OnCapsuleZoomBegin" + gesture.scaleX + "," + gesture.scaleY + "\n" + debugText.text;
        }

        private void OnSpherePanEnd(PanGesture gesture)
        {
            debugText.text = "OnSpherePanEnd" + gesture.screenOffsetX + "," + gesture.screenOffsetY + "\n" + debugText.text;
        }

        private void OnSpherePan(PanGesture gesture)
        {
            debugText.text = "OnSpherePan" + gesture.screenOffsetX + "," + gesture.screenOffsetY + "\n" + debugText.text;
        }

        private void OnSpherePanBegin(PanGesture gesture)
        {
            debugText.text = "OnSpherePanBegin" + gesture.screenOffsetX + "," + gesture.screenOffsetY + "\n" + debugText.text;
        }

        private void OnCubeTap(TapGesture gesture)
        {
            debugText.text = "OnCubeTap" + gesture.position + " -- " + gesture.location + "\n" + debugText.text;
        }

        private void OnReset(TapGesture gesture)
        {
            debugText.text = "OnReset" + gesture.position + " -- " + gesture.location + "\n" + debugText.text;
        }

        private void OnCameraZoom(ZoomGesture gesture)
        {
            debugText.text = "OnCameraZoom" + gesture.scaleX + " -- " + gesture.scaleY + "\n" + debugText.text;
        }

        private void OnCameraUpDown(PanGesture gesture)
        {
            debugText.text = "OnCameraUpDown" + gesture.screenOffsetX + " -- " + gesture.screenOffsetY + "\n" + debugText.text;
        }

        private void OnCameraRotate(PanGesture gesture)
        {
            debugText.text = "OnCameraRotate" + gesture.screenOffsetX + " -- " + gesture.screenOffsetY + "\n" + debugText.text;
        }
    }
}