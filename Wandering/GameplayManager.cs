using System;
using BearCore.Manager;
using JetBrains.Annotations;
using UnityEngine;
using Wandering.World;

namespace Wandering;

public class GameplayManager : BearGpManager {
    private static GameplayManager? _instance;
    public static GameplayManager? Instance => _instance;
    
    [SerializeField] private WanderingWorld? world = null!;
    
    public WanderingWorld? World => world;
    
    public override void Setup() {
        // Create Instance
        CreateManagerInstance();
        // Read session state
        // Determine new game vs load game
        // Load or generate world
    }

    private void CreateManagerInstance() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }
    }

    public override void Init() {
        // Initialize sub-systems with resolved data
        // (world, navigation, characters)
        InitializeWorld();
    }

    public override void Deinit() {
        // Cleanup game systems
    }

    private void InitializeWorld() {
        if (world == null) {
            throw new InvalidOperationException("WanderingWorld not provided");
        }
        world.InitializeWorld();
    }
}