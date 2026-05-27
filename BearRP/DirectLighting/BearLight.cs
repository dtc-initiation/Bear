using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BearRP.DirectLighting;

[RequireComponent(typeof(Light))]
public class BearLight : MonoBehaviour {
    [Header("Base Configs")]
    [SerializeField] private Color lightColor;
    [SerializeField] private float lightIntensity;
    [SerializeField] private float distance;
    
    [Header("Cone")]
    [SerializeField] private LightType lightType;
    [SerializeField] private Vector2 forwardDir;
    [SerializeField, Range(0f, 180f)] private float coneInner;
    [SerializeField, Range(0f, 180f)] private float coneOuter;
    
    

    public Light Light; 
    
    public Color Color =>  lightColor;
    public float Distance => distance;
    
    public float Intensity => lightIntensity;
    public LightType LightType => lightType;
    public Vector2 ForwardDir => forwardDir;
    public float ConeInner => coneInner;
    public float ConeOuter => coneOuter;
    

    private void OnValidate() {
        forwardDir = forwardDir.normalized;
        if (coneInner > coneOuter) {
            coneOuter = coneInner;
        }
        
        if ((lightType != LightType.Point) && (lightType != LightType.Spot)) {
            lightType = LightType.Point;
        }
        
        if (Light != null) {
            Light.type = lightType;
            Light.color = lightColor;
            Light.intensity = lightIntensity;
            Light.range = distance;
        }
    }
}