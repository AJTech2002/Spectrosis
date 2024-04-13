using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialRegistry", menuName = "MaterialRegistry")]
public class MaterialRegistrySO : ScriptableObject
{
    public enum ObjectMaterialType
    {
        Metal,
        Wood,
        Glass,
        Plastic,
        Rubber
    }

    [Serializable]
    public struct ObjectMaterial
    {
        public ObjectMaterialType type;
        public float attack;
        public float decay;
        public float sustain;
        public float release;
        public float damping;
    }

    public List<ObjectMaterial> materials = new List<ObjectMaterial>();
}
