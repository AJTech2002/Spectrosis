using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public MaterialItem selectedMaterialItem;
    public MaterialRegistrySO RegistrySo;
    public Transform instantiateMatObj;
    
    private void Start()
    {
        List<MaterialItem> materialItems = new List<MaterialItem>();
        foreach (var mat in RegistrySo.materials)
        {
            Transform t = Transform.Instantiate(instantiateMatObj, this.transform);
            t.GetComponent<MaterialItem>().SetMaterial(mat);
            materialItems.Add(t.GetComponent<MaterialItem>());
        }

        materialItems[0].SetSelected();

    }

    public void SelectItem(MaterialItem item)
    {
        if (selectedMaterialItem != null) selectedMaterialItem.Deselect();
        item.Select();
        selectedMaterialItem = item;
    }
    
}
