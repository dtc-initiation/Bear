using BearCore.Manager;
using BearCore.Scene;
using UnityEngine;

namespace Wandering.Scene;

public class GameSceneHandler : SceneHandler {
    [SerializeField] private BearGpManager bearGpManager = null!;

    protected override async Awaitable HandleSceneAsync() {
        bearGpManager.Setup();
        bearGpManager.Init();
        await Awaitable.NextFrameAsync();
    }
}