using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BearCore.Scene;

public class LoadingSceneHandler : SceneHandler {
    // TODO register progress bar callback
    protected override IProgress<float>? GetProgress() {
        return new Progress<float>();
    }

    protected override async Awaitable HandleSceneAsync() {
        await base.HandleSceneAsync();
        // TODO Move onto game scene
        // await SceneManager.LoadSceneAsync("RadianceCascadeScene");
        await SceneManager.LoadSceneAsync("2.25DScene");
    }
}