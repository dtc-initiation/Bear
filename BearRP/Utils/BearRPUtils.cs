using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP.Utils;

public static class BearRPUtils {
    public static bool GetOrLoadResource<Type>(ref Type resource, string resourcePath) where Type : Object {
        if (resource == null) {
            resource = Resources.Load<Type>(resourcePath);
            return true;
        }
        return false;
    }

    public static bool GetOrLoadMaterial(ref Material material, string shaderPath) {
        if (material == null) {
            Shader shader = Shader.Find(shaderPath);
            if (shader == null) {
                return false;
            }
            material = new Material(shader);
            return true;
        }
        return false;
    }
    
    
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