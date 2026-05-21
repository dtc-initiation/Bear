using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP.Utils;

public static class BearRPUtils {
    public static Mesh CreateQuad() => new Mesh() {
        vertices = [
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f)
        ],
        uv = [
            Vector2.zero,
            Vector2.right,
            Vector2.one,
            Vector2.up
        ],
        triangles = [0, 1, 2, 0, 2, 3],
        indexFormat = IndexFormat.UInt16
    };
    
    public static readonly Mesh Quad = new Mesh() {
        vertices = [
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f)
        ],
        uv = [
            Vector2.zero,
            Vector2.right,
            Vector2.one,
            Vector2.up
        ],
        triangles = [0, 1, 2, 0, 2, 3],
        indexFormat = IndexFormat.UInt16
    };
}