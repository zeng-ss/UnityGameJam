using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneObjectManager : MonoBehaviour
{
    private static SceneObjectManager _instance;
    public static SceneObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObj = new GameObject("[SceneObjectManager]");
                _instance = managerObj.AddComponent<SceneObjectManager>();
                DontDestroyOnLoad(managerObj);
            }
            return _instance;
        }
    }

    private Dictionary<string, GameObject> _tagToObj = new ();
    private Dictionary<string, GameObject> _nameToObj = new ();
    public bool IsObjectValid(GameObject obj) => obj != null && !obj.Equals(null);

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this) Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _tagToObj.Clear();
        _nameToObj.Clear();
    }

    // 核心获取方法（缓存+验证）
    public T GetObjectByTag<T>(string tag) where T : Component
    {
        if (_tagToObj.TryGetValue(tag, out var cachedObj) && IsObjectValid(cachedObj))
            return cachedObj.GetComponent<T>();

        var newObj = GameObject.FindGameObjectWithTag(tag);
        if (newObj != null) _tagToObj[tag] = newObj;
        return newObj?.GetComponent<T>();
    }

    public T GetObjectByName<T>(string name) where T : Component
    {
        if (_nameToObj.TryGetValue(name, out var cachedObj) && IsObjectValid(cachedObj))
            return cachedObj.GetComponent<T>();

        var newObj = GameObject.Find(name);
        if (newObj != null) _nameToObj[name] = newObj;
        return newObj?.GetComponent<T>();
    }
    
    public GameObject GetObjectByName(string name) 
    {
        if (_nameToObj.TryGetValue(name, out var cachedObj) && IsObjectValid(cachedObj))
            return cachedObj;

        var newObj = GameObject.Find(name);
        if (newObj != null) _nameToObj[name] = newObj;
        return newObj;
    }
    
    public GameObject GetObjectByTag(string tag) 
    {
        if (_tagToObj.TryGetValue(tag, out var cachedObj) && IsObjectValid(cachedObj))
            return cachedObj;

        var newObj = GameObject.FindGameObjectWithTag(tag);
        if (newObj != null) _tagToObj[tag] = newObj;
        return newObj;
    }

}