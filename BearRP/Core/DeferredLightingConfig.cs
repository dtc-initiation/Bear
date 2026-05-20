using UnityEngine;

namespace BearRP.Core;

public readonly struct DeferredLightingConfig {
    public readonly Material DeferredLightingMaterial;
    
    public DeferredLightingConfig(Material deferredMaterial) {
        DeferredLightingMaterial = deferredMaterial;
    }
    
}