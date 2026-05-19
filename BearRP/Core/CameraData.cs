using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP.Core;

public class CameraData {
    public Camera Camera = null!;
    public CullingResults CullingResults;
    
    // Camera matrices related data
    public Vector4 CameraInfo;
}