using UnityEngine;
using System.Collections;
namespace Gestzu.Core
{
    public class GTouch
    {
        /**
		 * Touch point ID.
		 */
		public int id;
		/**
		 * The original event target for this touch (touch began with).
		 */
		public GameObject target;
		
		public float sizeX;
		public float sizeY;
		public float pressure;
        private bool _locationInited;
        protected Vector3 _location = Vector3.zero;
        public Vector3 location
        {
            get
            {
                return new Vector3(_location.x, _location.y, _location.z);
            }
        }
        protected Vector3 _prevLocation;
        public Vector3 prevLocation
        {
            get
            {
                return new Vector3(_prevLocation.x, _prevLocation.y, _prevLocation.z);
            }
        }
        protected Vector3 _position = Vector3.zero;
		public Vector3 position
        {
            get{
			    return new Vector3(_position.x,_position.y,_position.z);
            }
		}
        protected Vector3 _prevPosition;
		public Vector3 prevPosition
		{
            get{
			    return new Vector3(_prevPosition.x,_prevPosition.y,_prevPosition.z);
            }
		}
		
		
		protected Vector3 _beginPosition;
		public Vector3 beginPosition
		{
            get{
			    return new Vector3(_beginPosition.x,_beginPosition.y,_beginPosition.z);
            }
		}
		
		
		public Vector3 positionOffset
		{
            get
            {
			    return _position - _beginPosition;
            }
		}
		
		
		protected float _time;
		public float time
		{
            get{
			    return _time;
            }
		}
		internal void SetTime(float value)
		{
			_time = value;
		}
		
		
		protected float _beginTime;
		public float beginTime
		{
            get{
			    return _beginTime;
            }
		}
		internal void SetBeginTime(float value)
		{
			_beginTime = value;
		}
		
        public GTouch()
        {
        }
        internal void SetLocation(Vector3 pos,float time,Vector3 loc)
		{
            _position = new Vector3(pos.x, pos.y, pos.z);
            _beginPosition = new Vector3(pos.x, pos.y, pos.z);
            _prevPosition = new Vector3(pos.x, pos.y, pos.z);
            _location = new Vector3(loc.x,loc.y,loc.z);
            _prevLocation = new Vector3(loc.x, loc.y, loc.z);

            _time = time;
			_beginTime = time;
            _locationInited = true;
		}
        internal bool UpdateLocation(Vector3 pos, float time,Vector3 loc)
		{
            if (_locationInited == true)
            {
                if (_position.Equals(pos) == true)
                    return false;

                _prevPosition.x = _position.x;
                _prevPosition.y = _position.y;
                _prevPosition.z = _position.z;

                _position.x = pos.x;
                _position.y = pos.y;
                _position.z = pos.z;

                _prevLocation.x = _location.x;
                _prevLocation.y = _location.y;
                _prevLocation.z = _location.z;

                _location.x = loc.x;
                _location.y = loc.y;
                _location.z = loc.z;

                _time = time;
            }
            else
            {
                SetLocation(pos, time,loc);
            }
			
			return true;
		}
    }
}