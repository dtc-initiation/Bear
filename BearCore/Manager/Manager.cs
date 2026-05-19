using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BearCore.Manager;

public class Manager : MonoBehaviour  {
    [SerializeField] private List<ManagerBase> managerList = new();
    
    private static Manager _instance;
    private readonly Dictionary<Type, ManagerBase> _managerLookup = new();
    
    public static Manager Main => _instance;

    public static T Get<T>() where T : ManagerBase {
        if (_instance == null) {
            throw new InvalidOperationException("Central manager is not initialized");
        }
        if (_instance._managerLookup.TryGetValue(typeof(T), out ManagerBase manager)) {
            return (T) manager;
        } 
        throw new InvalidOperationException($"Manager of type {typeof(T)} not found");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void BeforeAssembliesLoaded() {
        // Log system info and parse command line
        // Initialize platform interface
        // Initialize filesystem
        //  - Platform save directory
    }
    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void BeforeSceneLoad() {
        // Validate platform
        // Parse Version string
        // Check system req
        
    }

    public static async Awaitable InitializeGlobalManagerAsync(IProgress<float>? progress) {
        // Load GlobalManager Prefab
        var globalManagerPath = "GlobalManager";
        ResourceRequest req = Resources.LoadAsync<GameObject>(globalManagerPath);
        while (!req.isDone) {
            await Awaitable.NextFrameAsync();
        }
        GameObject globalManagerPrefab = req.asset as GameObject;
        if (globalManagerPrefab == null) {
            throw new InvalidOperationException("GlobalManager prefab not found in Resources DIR");
        }
        
        // Instantiate Inactive
        globalManagerPrefab.SetActive(false);
        var instanceTemp = Instantiate(globalManagerPrefab);
        var manager = instanceTemp.GetComponent<Manager>();
        if (manager == null) {
            throw new InvalidOperationException("Manager component not attached to Manager Prefab");
        }
        DontDestroyOnLoad(instanceTemp);
        
        // Register Core managers into lookup
        manager.managerList.Sort();
        foreach (ManagerBase listedManager in manager.managerList) {
            manager._managerLookup.Add(listedManager.GetType(), listedManager);
        }
        
        // Setup Pass
        for (int i = 0; i < manager.managerList.Count; i++) {
            manager.managerList[i].Setup();
        }
        
        // Init Pass
        for (int i = 0; i < manager.managerList.Count; i++) {
            manager.managerList[i].Init();
        }
        
        // Activate and set instance
        instanceTemp.SetActive(true);
        _instance = manager;

        progress.Report(1f);
    }

    private void OnDestroy() {
        _managerLookup.Clear();
        managerList.Clear();
        Destroy(_instance);
    }
}