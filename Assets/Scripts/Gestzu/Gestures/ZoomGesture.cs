using UnityEngine;
using System.Collections;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class ZoomGesture : AbstractContinuousGesture<ZoomGesture>
    {
		public bool lockAspectRatio = true;
		
		protected GTouch _touch1;
		protected GTouch _touch2;
        private float _slop = -1;
        public float slop
        {
            get
            {
                if (_slop < 0) _slop = defaultSlop;
                return _slop;
            }
        }
		protected Vector3 _transformVector;
		protected float _initialDistance;
        protected float _scaleX = 1;
		public float scaleX
		{
            get{
			    return _scaleX;
            }
		}
		
		protected float _scaleY = 1;
		public float scaleY
		{
            get{
			    return _scaleY;
            }
		}
        protected float _scaleZ = 1;
        public float scaleZ
        {
            get
            {
                return _scaleZ;
            }
        }
        public override int touchID
        {
            get
            {
                if (_touch1 == null) return -1;
                return _touch1.id;
            }
        }
        protected override void OnTouchBegin(GTouch touch)
        {
            if (touchesCount > 2)
            {
                FailOrIgnoreTouch(touch);
                return;
            }

            if (touchesCount == 1)
            {
                _touch1 = touch;
            }
            else// == 2
            {
                _touch2 = touch;

                _transformVector = _touch2.position - _touch1.position;
                _initialDistance = _transformVector.magnitude;
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            if (touchesCount < 2)
				return;
			
			Vector3 currTransformVector = _touch2.position - _touch1.position;
			
			if (state == GestureState.POSSIBLE)
			{
				float d = currTransformVector.magnitude - _initialDistance;
				float absD = d >= 0 ? d : -d;
				if (absD < slop)
				{
					// Not recognized yet
					return;
				}
				
				if (slop > 0)
				{
					// adjust _transformVector to avoid initial "jump"
					Vector3 slopVector = new Vector3(currTransformVector.x,currTransformVector.y,currTransformVector.z);
					slopVector.Normalize();
                    _transformVector = slopVector * (_initialDistance + (d >= 0 ? slop : -slop));
				}
			}
			
			
			if (lockAspectRatio)
			{
				_scaleX *= currTransformVector.magnitude / _transformVector.magnitude;
				_scaleY = _scaleX;
			}
			else
			{
				_scaleX *= currTransformVector.x / _transformVector.x;
				_scaleY *= currTransformVector.y / _transformVector.y;
			}
			
			_transformVector.x = currTransformVector.x;
			_transformVector.y = currTransformVector.y;
			
			UpdateLocation();
			
			if (state == GestureState.POSSIBLE)
			{
				SetState(GestureState.BEGAN);
			}
			else
			{
				SetState(GestureState.CHANGED);
			}
        }
        protected override void OnTouchEnd(GTouch touch)
        {
            if (touchesCount == 0)
            {
                if (state == GestureState.BEGAN || state == GestureState.CHANGED)
                {
                    SetState(GestureState.ENDED);
                }
                else if (state == GestureState.POSSIBLE)
                {
                    SetState(GestureState.FAILED);
                }
            }
            else//== 1
            {
                if (touch == _touch1)
                {
                    _touch1 = _touch2;
                }
                _touch2 = null;

                if (state == GestureState.BEGAN || state == GestureState.CHANGED)
                {
                    UpdateLocation();
                    SetState(GestureState.CHANGED);
                }
            }
        }
        protected override void ResetNotificationProperties()
        {
            base.ResetNotificationProperties();
            _scaleX = _scaleY = _scaleZ = 1;
        }
    }
}

