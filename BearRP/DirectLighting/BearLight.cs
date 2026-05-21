using System;
using UnityEngine;

namespace BearRP.DirectLighting;

[RequireComponent(typeof(Light))]
public class BearLight : MonoBehaviour {
    [SerializeField] private Color lightColor;
    [SerializeField] private float lightIntensity;
    [SerializeField] private float coneAngleSize;
    [SerializeField] private Vector2 forwardDir;
    [SerializeField] private float distance;

    public Light Light; 
    
    public Color Color =>  lightColor;
    public float Intensity => lightIntensity;
    public float ConeAngleSize => coneAngleSize;
    public Vector2 ForwardDir => forwardDir;
    public float Distance => distance;

    private void OnValidate() {
        if (Light != null) {
            Light.color = lightColor;
            Light.intensity = lightIntensity;
            Light.range = distance;
        }
    }
}