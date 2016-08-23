using UnityEngine;
using System.Collections;
using Gestzu.Core;

namespace Gestzu.Gestures
{
    public class RotateGesture : AbstractContinuousGesture<RotateGesture>
    {
        private float _slop = -1;
        public float slop
        {
            get
            {
                if (_slop < 0) _slop = defaultSlop;
                return _slop;
            }
        }
		
		protected GTouch _touch1;
		protected GTouch _touch2;
		protected Vector3 _transformVector;
		protected float _thresholdAngle;
        protected float _rotation = 0;
		public float rotation
		{
            get{
			    return _rotation;
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
            else
            {
                _touch2 = touch;

                _transformVector = _touch2.position-_touch1.position;

                // @see chord length formula
                _thresholdAngle = Mathf.Asin(slop / (2 * _transformVector.magnitude)) * 2;
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            if (touchesCount < 2)
				return;
			
			Vector3 currTransformVector = _touch2.position-_touch1.position;
			float cross = (_transformVector.x * currTransformVector.y) - (currTransformVector.x * _transformVector.y);
			float dot = (_transformVector.x * currTransformVector.x) + (_transformVector.y * currTransformVector.y);
			float rotation = Mathf.Atan2(cross, dot);
			
			if (state == GestureState.POSSIBLE)
			{
				float absRotation = rotation >= 0 ? rotation : -rotation;
				if (absRotation < _thresholdAngle)
				{
					// not recognized yet
					return;
				}
				
				// adjust angle to avoid initial "jump"
				rotation = rotation > 0 ? rotation - _thresholdAngle : rotation + _thresholdAngle;
			}
			
			_transformVector.x = currTransformVector.x;
			_transformVector.y = currTransformVector.y;
            _transformVector.z = currTransformVector.z;
			_rotation = rotation;
			
			UpdateLocation();
			
			if (state == GestureState.POSSIBLE)
			{
                //识别到先赋值
                _touchID = touch.id;
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
            else// == 1
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
            _rotation = 0;
        }
    }
}