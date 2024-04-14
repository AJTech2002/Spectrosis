using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialRegistry", menuName = "MaterialRegistry")]
public class MaterialRegistrySO : ScriptableObject
{
    [System.Serializable]
    public enum ObjectMaterialType
    {
        Metal,
        Wood,
        Glass,
        Plastic,
        Rubber,
        Mouth
    }

    [Serializable]
    public struct ObjectMaterial
    {
        public ObjectMaterialType type;
        public Color color;
        public float attack;
        public float decay;
        public float sustain;
        public float release;
        public float damping;
        public float cutoff;
        public float chorus;
    }

    public List<ObjectMaterial> materials = new List<ObjectMaterial>();
}
