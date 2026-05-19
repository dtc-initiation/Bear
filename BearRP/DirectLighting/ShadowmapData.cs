using UnityEngine;

namespace BearRP.DirectLighting;

public class ShadowmapData {
    public Mesh ShadowMesh = null!;
    public Material ShadowmapMaterial = null!;
    public ComputeBuffer LightBuffer = null!;
    public int InstanceCount;
    public MaterialPropertyBlock Mpb = null!;
}