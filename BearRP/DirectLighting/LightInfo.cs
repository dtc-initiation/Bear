using System.Runtime.InteropServices;
using UnityEngine;

namespace BearRP.DirectLighting;

[StructLayout(LayoutKind.Sequential)]
public readonly struct LightInfo {
    public readonly Vector4 positionId;
    public readonly Vector4 colorDistance;
    public readonly Vector4 forwardAngleCos;

    public LightInfo(Vector4 positionId, Vector4 colorDistance, Vector4 forwardAngleCos) {
        this.positionId = positionId;
        this.colorDistance = colorDistance;
        this.forwardAngleCos = forwardAngleCos;
    }
}