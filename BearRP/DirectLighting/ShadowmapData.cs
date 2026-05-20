using UnityEngine;

namespace BearRP.DirectLighting;

public class ShadowmapData {
    public int NumPass;
    public Mesh ShadowMesh = null!;
    public Material ShadowmapMaterial = null!;
    public ComputeBuffer LightBuffer = null!;
    public int InstanceCount;
    public MaterialPropertyBlock Mpb = null!;
}