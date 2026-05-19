using System;
using UnityEngine;

namespace BearCore.Scene;

public abstract class SceneHandler : MonoBehaviour {
    protected virtual IProgress<float>? GetProgress() => null;
    
    protected virtual async Awaitable HandleSceneAsync() {
        await Manager.Manager.InitializeGlobalManagerAsync(GetProgress());
    }

    private async void Start() {
        await HandleSceneAsync();
    }
}