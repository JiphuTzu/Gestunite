using UnityEngine;
using System.Collections;
using Gestzu.Core;
namespace Gestzu.Gestures
{
    public class TransformGesture : AbstractContinuousGesture<TransformGesture>
    {
		
		protected GTouch _touch1;
		protected GTouch _touch2;
		protected Vector3 _transformVector;
        private float _slop =-1;
        public float slop
        {
            get
            {
                if (_slop < 0) _slop = defaultSlop;
                return _slop;
            }
        }
        protected float _offsetX = 0;
		public float offsetX
		{
            get{
			    return _offsetX;
            }
		}
		
		protected float _offsetY = 0;
		public float offsetY
		{
            get{
			    return _offsetY;
            }
		}
        protected float _offsetZ = 0;
        public float offsetZ
        {
            get
            {
                return _offsetZ;
            }
        }
        protected float _rotation = 0;
		public float rotation
		{
            get{
			    return _rotation;
            }
		}
		
		
		protected float _scale = 1;
		public float scale
		{
            get{
			    return _scale;
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
        public override void Reset()
        {
            _touch1 = null;
            _touch2 = null;
            base.Reset();
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

                _transformVector = _touch2.position - _touch1.position;
            }

            UpdateLocation();

            if (state == GestureState.BEGAN || state == GestureState.CHANGED)
            {
                // notify that location (and amount of touches) has changed
                SetState(GestureState.CHANGED);
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            Vector3 prevLocation = new Vector3(_position.x,_position.y,_position.z);
			UpdateLocation();
			
			if (state == GestureState.POSSIBLE)
			{
				if (slop > 0 && touch.positionOffset.magnitude < slop)
				{
					// Not recognized yet
					if (_touch2!=null)
					{
						// Recalculate _transformVector to avoid initial "jump" on recognize
						_transformVector = _touch2.position-_touch1.position;
					}
					return;
				}
			}
			_offsetX = _position.x - prevLocation.x;
			_offsetY = _position.y - prevLocation.y;
            _offsetZ = _position.z - prevLocation.z;
			
			if (_touch2!=null)
			{
                Vector3 currTransformVector = _touch2.position - _touch1.position;
				_rotation = Mathf.Atan2(currTransformVector.y, currTransformVector.x) - Mathf.Atan2(_transformVector.y, _transformVector.x);
				_scale = currTransformVector.magnitude / _transformVector.magnitude;
				_transformVector = _touch2.position - _touch1.position;
			}
			
			SetState(state == GestureState.POSSIBLE ? GestureState.BEGAN : GestureState.CHANGED);
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
            _offsetX = _offsetY = _offsetZ = 0;
            _rotation = 0;
            _scale = 1;
        }
    }
}