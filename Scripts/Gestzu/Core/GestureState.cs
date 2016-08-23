using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gestzu.Core
{

    public class GestureState {
        public static GestureState POSSIBLE = new GestureState("POSSIBLE",false,new string[]{"RECOGNIZED","BEGAN","FAILED"});
		public static GestureState RECOGNIZED = new GestureState("RECOGNIZED", true,new string[]{"POSSIBLE"});
		public static GestureState BEGAN = new GestureState("BEGAN",false,new string[]{"CHANGED","ENDED","CANCELLED"});
        public static GestureState CHANGED = new GestureState("CHANGED", false, new string[] { "CHANGED", "ENDED", "CANCELLED" });
        public static GestureState ENDED = new GestureState("ENDED", true, new string[] { "POSSIBLE" });
        public static GestureState CANCELLED = new GestureState("CANCELLED", true, new string[] { "POSSIBLE" });
        public static GestureState FAILED = new GestureState("FAILED", true, new string[] { "POSSIBLE" });

        private Dictionary<string,bool> _validTransitionStateMap = new Dictionary<string,bool>();
        private string _name;
        private string _eventType;
        private bool _isEndState;

        public string name
        {
            get
            {
                return _name;
            }
        }

        public GestureState(string name, bool isEndState,string[] validNextStates)
        {
            _name = FormatName(name);
            _eventType = "gesture" + name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
            _isEndState = isEndState;
            int i = validNextStates.Length;
            while (--i >= 0)
            {
                _validTransitionStateMap[FormatName(validNextStates[i])] = true;
            }
        }
        
        public override string ToString()
        {
            return _name;
        }
        public string ToEventType()
        {
            return _eventType;
        }
        public bool CanTransitionTo(GestureState state)
        {
            return _validTransitionStateMap.ContainsKey(state.name);
        }
        public bool IsEndState
        {
            get
            {
                return _isEndState;
            }
        }
        private string FormatName(string name)
        {
            return "GestureState." + name.ToUpper();
        }
	}
}