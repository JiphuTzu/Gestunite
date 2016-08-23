using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gestzu.Inputs;
using Gestzu.Extensions;

namespace Gestzu.Core
{
    /// <summary>
    /// 手势的统一入口
    /// </summary>
    public class Gestunite : MonoBehaviour
    {
        public enum InputType
        {
            mouse,
            touch
        }
        public enum HitType
        {
            ray
        }
        private static Gestunite _instance;
        private static object _lock = new object();
        //private static bool _applicationIsQuitting = false;
        public static Gestunite GetInstance()
        {
            //if (_applicationIsQuitting)
            //{
            //    Debug.LogWarning("Gestunite is already destroyed on application quit." 
            //        + " Won't create again - returning null.");
            //    return null;
            //}

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (Gestunite)FindObjectOfType(typeof(Gestunite));

                    if (FindObjectsOfType(typeof(Gestunite)).Length > 1)
                    {
                        Debug.LogError("There should never be more than 1 Gestunite!"
                            + " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        Debug.LogError("Gestunite is needed in the scene! "
                            + " Reopening the scene might fix it.");
                    }
                }

                return _instance;
            }
        }
        //========================================
        //配置文件路径，相对于Application.dataPath的相对路径。“..”表示上一级目录
        [SerializeField]
        private string _configPath = "../exdata/gestunite.ini";
        [SerializeField]
        private HitType _hitter;
        private HitTester _hitTester;
        public HitTester hitTester
        {
            get
            {
                if (_hitTester == null)
                {
                    string type = GetConfig("HITTER", _hitter.ToString());
                    if (type == "ray") _hitTester = GetOrCreate<RayHitTester>();
                    else _hitTester = GetOrCreate<RayHitTester>();
                }
                return _hitTester;
            }
        }
        [SerializeField]
        private InputType _input;
        //
        private InputAdapter _inputAdapter;
        public InputAdapter inputAdapter
        {
            get
            {
                if (_inputAdapter == null)
                {
                    string type = GetConfig("INPUT", _input.ToString());
                    if (type == "touch") _inputAdapter = GetOrCreate<TouchInputAdapter>();
                    else _inputAdapter = GetOrCreate<MouseInputAdapter>();
                }
                return _inputAdapter;
            }
        }
        private TouchesManager _touchesManager;
        public TouchesManager touchesManager
        {
            get
            {
                if (_touchesManager == null)
                {
                    _touchesManager = new TouchesManager(gesturesManager);
                }
                return _touchesManager;
            }
        }
        private GesturesManager _gesturesManager;
        public GesturesManager gesturesManager
        {
            get
            {
                if (_gesturesManager == null)
                {
                    _gesturesManager = new GesturesManager();
                }
                return _gesturesManager;
            }
        }
        [SerializeField]
        private List<GameObject> _roots;
        public List<GameObject> roots
        {
            get
            {
                if (_roots == null)
                {
                    _roots = new List<GameObject> { gameObject };
                }
                return _roots;
            }
        }
        public Ray GetRay()
        {
            return hitTester.GetRay();
        }
        private bool _inited;

        private void Awake()
        {
            CheckInit();
        }
        private void OnEnable()
        {
            CheckInit();
        }
        public void SetLayerMask(LayerMask layerMask)
        {
            hitTester.SetLayerMask(layerMask);
        }

        private void CheckInit()
        {
            if (_inited == false)
            {
                Init();
                _inited = true;
            }
        }
        private void Init()
        {

            inputAdapter.touchesManager = touchesManager;
            //inputAdapters[i].Init();
            //Debug.Log("Gestunite.Init()\tuse inputAdatper " + inputAdapter.name);
            //
            touchesManager.SetHitTester(hitTester);

            touchesManager.SetRoots(_roots);
        }
        //public void OnDestroy()
        //{
        //_applicationIsQuitting = true;
        //}
        protected string GetConfig(string key, string dvalue = null)
        {
            GetConfigs();
            if (_configs == null || _configs.ContainsKey(key) == false) return dvalue;
            return _configs[key];
        }
        
        private Dictionary<string, string> _configs;
        protected void GetConfigs()
        {
            if (_configs != null) return;
            string path = GetConfigPath();
            if (File.Exists(path) == false) return;
            StreamReader reader = new StreamReader(File.Open(path, FileMode.Open));
            char spliter = '=';
            _configs = new Dictionary<string, string>();
            while (true)
            {
                string line = reader.ReadLine();
                //Debug.Log("config " + line);
                if (line == null) break;
                if (line.StartsWith("#")) continue;
                int index = line.IndexOf(spliter);
                if (index < 0 || index != line.LastIndexOf(spliter)) continue;
                string[] kv = line.Split(spliter);
                _configs.Add(kv[0].Trim(), kv[1].Trim());
            }
        }
        protected string GetConfigPath()
        {
            string[] paths = _configPath.Replace('\\', '/').Split('/');
            string path = Application.dataPath;
            for (int i = 0; i < paths.Length; i++)
            {
                string p = paths[i].Trim();
                if (p == "..") path = Directory.GetParent(path).ToString();
                else path += "/" + p;
            }
            return path;
        }
        protected T GetOrCreate<T>() where T : MonoBehaviour
        {
            T t = gameObject.GetComponent<T>();
            if (t == null) t = gameObject.AddComponent<T>();
            return t;
        }
    }
}