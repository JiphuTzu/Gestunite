using UnityEngine;
using System;
using System.Collections;
using Gestzu.Core;
using Gestzu.Utils;
namespace Gestzu.Gestures
{
    [FlagsAttribute]
    public enum SwipeDirection
    {
        RIGHT = 1,
        LEFT = 2,
        UP = 4,
        DOWN = 8,
    }
    public class SwipeGesture : AbstractDiscreteGesture<SwipeGesture>
    {
        private static float ANGLE = 40 * Mathf.Deg2Rad;
		private static float MAX_DURATION = 0.5f;
		private static float MIN_OFFSET = 12.0f;
		private static float MIN_VELOCITY = 2 * MIN_OFFSET / MAX_DURATION;

        /**
		 * "Dirty" region around touch begin location which is not taken into account for
		 * recognition/failing algorithms.
		 * 
		 * @default Gesture.DEFAULT_SLOP
		 */
		private float _slop = -1;
        public float slop
        {
            get
            {
                if (_slop < 0) _slop = defaultSlop;
                return _slop;
            }
        }
		public uint numTouchesRequired = 1;
        [EnumFlags]
		public SwipeDirection direction = (SwipeDirection)15;
		
		/**
		 * The duration of period (in milliseconds) in which SwipeGesture must be recognized.
		 * If gesture is not recognized during this period it fails. Default value is 500 (half a
		 * second) and generally should not be changed. You can change it though for some special
		 * cases, most likely together with <code>minVelocity</code> and <code>minOffset</code>
		 * to achieve really custom behavior. 
		 * 
		 * @default 500
		 * 
		 * @see #minVelocity
		 * @see #minOffset
		 */
		public float maxDuration = MAX_DURATION;
		
		/**
		 * Minimum offset (in pixels) for gesture to be recognized.
		 * Default value is <code>Capabilities.screenDPI / 6</code> and generally should not
		 * be changed.
		 */
		public float minOffset = MIN_OFFSET;
		
		/**
		 * Minimum velocity (in pixels per millisecond) for gesture to be recognized.
		 * Default value is <code>2 * minOffset / maxDuration</code> and generally should not
		 * be changed.
		 * 
		 * @see #minOffset
		 * @see #minDuration
		 */
		public float minVelocity = MIN_VELOCITY;

        protected Vector3 _offset = new Vector3();
		protected float _startTime;
		protected bool _noDirection;
		protected Vector3 _avrgVel = new Vector3();
		protected Coroutine _timer;
        public float offsetX
		{
            get{
			    return _offset.x;
            }
		}
		public float offsetY
		{
            get{
			    return _offset.y;
            }
		}
        public float offsetZ
        {
            get
            {
                return _offset.z;
            }
        }
        public override void Reset()
        {
            _startTime = 0;
            _offset.x = 0;
            _offset.y = 0;
            _offset.z = 0;
            StopTimer();
            base.Reset();
        }
        protected override void OnTouchBegin(GTouch touch)
        {
            if (touchesCount > numTouchesRequired)
            {
                FailOrIgnoreTouch(touch);
                return;
            }

            if (touchesCount == 1)
            {
                // Because we want to fail as quick as possible
                _startTime = touch.time;

                StopTimer();
                StartTimer(maxDuration);
            }
            if (touchesCount == numTouchesRequired)
            {
                UpdateLocation();
                _avrgVel.x = _avrgVel.y = _avrgVel.z = 0;

                // cache direction condition for performance
                _noDirection = (((SwipeDirection)0) == direction);
            }
        }
        protected override void OnTouchMove(GTouch touch)
        {
            if (touchesCount < numTouchesRequired)
				return;
			
			float totalTime = touch.time - _startTime;
			if (totalTime < 0.01f)
				return;//It was somehow THAT MUCH performant on one Android tablet
			
			float prevCentralPointX = _centralPosition.x;
			float prevCentralPointY = _centralPosition.y;
            //float prevCentralPointZ = _centralPosition.z;
			UpdateCentralPoint();
			
			_offset.x = _centralPosition.x - _position.x;
			_offset.y = _centralPosition.y - _position.y;
            _offset.z = _centralPosition.z - _position.z;
			float offsetLength = _offset.magnitude;
			
			// average velocity (total offset to total duration)
			_avrgVel.x = _offset.x / totalTime;
			_avrgVel.y = _offset.y / totalTime;
            _avrgVel.z = _offset.z / totalTime;
			float avrgVel = _avrgVel.magnitude;
			
			if (_noDirection)
			{
				if ((offsetLength > slop) &&
					(avrgVel >= minVelocity || offsetLength >= minOffset))
				{
                    //识别到先赋值
                    _touchID = touch.id;
					SetState(GestureState.RECOGNIZED);
				}
			}
			else
			{
				float recentOffsetX = _centralPosition.x - prevCentralPointX;
				float recentOffsetY = _centralPosition.y - prevCentralPointY;
                //float recentOffsetZ = _centralPosition.z - prevCentralPointZ;
				//faster Math.abs()
				float absVelX = _avrgVel.x > 0 ? _avrgVel.x : -_avrgVel.x;
				float absVelY = _avrgVel.y > 0 ? _avrgVel.y : -_avrgVel.y;
                //float absVelZ = _avrgVel.z > 0 ? _avrgVel.z : -_avrgVel.z;
				
				if (absVelX > absVelY)
				{
					float absOffsetX = _offset.x > 0 ? _offset.x : -_offset.x;
					
					if (absOffsetX > slop)//faster isNaN()
					{
						if ((recentOffsetX < 0 && (direction & SwipeDirection.LEFT) == 0) ||
							(recentOffsetX > 0 && (direction & SwipeDirection.RIGHT) == 0) ||
							Mathf.Abs(Mathf.Atan(_offset.y/_offset.x)) > ANGLE)
						{
							// movement in opposite direction
							// or too much diagonally
							
							SetState(GestureState.FAILED);
						}
						else if (absVelX >= minVelocity || absOffsetX >= minOffset)
						{
                            //识别到先赋值
                            _touchID = touch.id;
							_offset.y = 0;
							SetState(GestureState.RECOGNIZED);
						}
					}
				}
				else if (absVelY > absVelX)
				{
					float absOffsetY = _offset.y > 0 ? _offset.y : -_offset.y;
					if (absOffsetY > slop)//faster isNaN()
					{
						if ((recentOffsetY < 0 && (direction & SwipeDirection.UP) == 0) ||
							(recentOffsetY > 0 && (direction & SwipeDirection.DOWN) == 0) ||
							Mathf.Abs(Mathf.Atan(_offset.x/_offset.y)) > ANGLE)
						{
							// movement in opposite direction
							// or too much diagonally
							
							SetState(GestureState.FAILED);
						}
						else if (absVelY >= minVelocity || absOffsetY >= minOffset)
						{
                            //识别到先赋值
                            _touchID = touch.id;
							_offset.x = 0;
							SetState(GestureState.RECOGNIZED);
						}
					}
				}
				// Give some tolerance for accidental offset on finger press (slop)
				else if (offsetLength > slop)//faster isNaN()
				{
					SetState(GestureState.FAILED);
				}
			}
        }
        protected override void ResetNotificationProperties()
        {
            base.ResetNotificationProperties();
            _offset.x = _offset.y = _offset.z = 0;
        }
        protected override void OnTouchEnd(GTouch touch)
        {
            if (touchesCount < numTouchesRequired)
            {
                SetState(GestureState.FAILED);
            }
        }
        private void StopTimer()
        {
            if (_timer != null)
            {
                StopCoroutine(_timer);
                _timer = null;
            }
        }
        private void StartTimer(float time)
        {
            _timer = StartCoroutine(TimerComplete(time));
        }
        private IEnumerator TimerComplete(float time)
        {
            yield return new WaitForSeconds(time);
            if (state == GestureState.POSSIBLE)
            {
                SetState(GestureState.FAILED);
            }
        }
    }
}