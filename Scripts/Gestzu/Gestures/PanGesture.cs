using UnityEngine;
using System.Collections;
using Gestzu.Core;

namespace Gestzu.Gestures
{
    public enum PanGestureDirection
    {
        NO_DIRECTION,
        VERTICAL,
        HORIZONTAL
    }
    public class PanGesture : AbstractContinuousGesture<PanGesture>
    {
        private float _slop = -1;
        public float slop
        {
            get
            {
                if(_slop<0)
                    _slop = defaultSlop;
                return _slop;
            }
        }
        /**
         * Used for initial slop overcome calculations only.
         */
        public PanGestureDirection direction = PanGestureDirection.NO_DIRECTION;
        /** @private */
        [SerializeField]
        private uint _maxNumTouchesRequired = 10;

        /**
         * 
         */
        public uint maxNumTouchesRequired
        {
            get
            {
                return _maxNumTouchesRequired;
            }
            set
            {
                if (_maxNumTouchesRequired == value)
                    return;

                if (value < minNumTouchesRequired)
                    Debug.LogError("maxNumTouchesRequired must be not less then minNumTouchesRequired");

                _maxNumTouchesRequired = value;
            }
        }


        /** @private */
        [SerializeField]
        private uint _minNumTouchesRequired = 1;

        /**
         * 
         */
        public uint minNumTouchesRequired
        {
            get
            {
                return _minNumTouchesRequired;

            }
            set
            {
                if (_minNumTouchesRequired == value)
                    return;

                if (value > maxNumTouchesRequired)
                    Debug.LogError("minNumTouchesRequired must be not greater then maxNumTouchesRequired");

                _minNumTouchesRequired = value;
            }
        }
        protected float _screenOffsetX;
        public float screenOffsetX
        {
            get
            {
                return _screenOffsetX;
            }
        }
        protected float _screenOffsetY;
        public float screenOffsetY
        {
            get
            {
                return _screenOffsetY;
            }
        }
        protected float _offsetX = 0;
        public float offsetX
        {
            get
            {
                return _offsetX;
            }
        }


        protected float _offsetY = 0;
        public float offsetY
        {
            get
            {
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
        internal override bool CanBePreventedByGesture(Gesture preventingGesture)
        {
            return preventingGesture.state == GestureState.BEGAN || preventingGesture.state == GestureState.CHANGED;
        }
        internal override bool CanPreventGesture(Gesture preventedGesture)
        {
            return state == GestureState.BEGAN || state == GestureState.CHANGED;
        }
        protected override void OnTouchBegin(Core.GTouch touch)
        {
            if (touchesCount > maxNumTouchesRequired)
            {
                FailOrIgnoreTouch(touch);
                return;
            }

            if (touchesCount >= minNumTouchesRequired)
            {
                UpdateLocation();
            }
        }
        protected override void OnTouchMove(Core.GTouch touch)
        {
            if (touchesCount < minNumTouchesRequired)
                return;

            float prevLocationX;
            float prevLocationY;
            float prevLocationZ;
            //
            float prevPositionX;
            float prevPositionY;
            if (state == GestureState.POSSIBLE)
            {
                prevLocationX = _location.x;
                prevLocationY = _location.y;
                prevLocationZ = _location.z;
                //
                prevPositionX = _position.x;
                prevPositionY = _position.y;
                //
                UpdateLocation();

                // Check if finger moved enough for gesture to be recognized
                //Debug.Log(this.direction + " move " + positionOffset.magnitude + " slop " + slop);
                if (CanBegin(touch.positionOffset))//faster isNaN(slop)
                {
                    // NB! += instead of = for the case when this gesture recognition is delayed via requireGestureToFail
                    _offsetX += _location.x - prevLocationX;
                    _offsetY += _location.y - prevLocationY;
                    _offsetZ += _location.z - prevLocationZ;
                    //
                    _screenOffsetX = _position.x - prevPositionX;
                    _screenOffsetY = _position.y - prevPositionY;
                    //识别到先赋值
                    _touchID = touch.id;
                    SetState(GestureState.BEGAN);
                }
            }
            else if (state == GestureState.BEGAN || state == GestureState.CHANGED)
            {
                prevLocationX = _location.x;
                prevLocationY = _location.y;
                prevLocationZ = _location.z;
                //
                prevPositionX = _position.x;
                prevPositionY = _position.y;

                UpdateLocation();

                _offsetX = _location.x - prevLocationX;
                _offsetY = _location.y - prevLocationY;
                _offsetZ = _location.z - prevLocationZ;

                _screenOffsetX = _position.x - prevPositionX;
                _screenOffsetY = _position.y - prevPositionY;

                SetState(GestureState.CHANGED);
            }
        }
        protected bool CanBegin(Vector3 offset)
        {
            offset.z = 0;

            if (direction == PanGestureDirection.VERTICAL)
            {
                if (Mathf.Abs(offset.y) > Mathf.Abs(offset.x))
                {
                    offset.x = 0;
                }
                else
                {
                    return false;
                }
            }
            else if (direction == PanGestureDirection.HORIZONTAL)
            {
                if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                {
                    offset.y = 0;
                }
                else
                {
                    return false;
                }
            }
            return offset.magnitude > slop;
        }
        protected override void OnTouchEnd(GTouch touch)
        {
            if (touchesCount < minNumTouchesRequired)
            {
                if (state == GestureState.POSSIBLE)
                {
                    SetState(GestureState.FAILED);
                }
                else
                {
                    SetState(GestureState.ENDED);
                }
            }
            else
            {
                UpdateLocation();
            }
        }
        protected override void ResetNotificationProperties()
        {
            base.ResetNotificationProperties();
            _offsetX = _offsetY = _offsetZ = 0;
        }
    }
}