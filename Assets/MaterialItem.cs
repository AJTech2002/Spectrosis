using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialItem : MonoBehaviour
{
   public bool eraser;

   public MaterialRegistrySO.ObjectMaterial _objectMaterial;
   [SerializeField] private Transform selectedIndicator;
   [SerializeField] private TMPro.TextMeshProUGUI materialName;
   [SerializeField] private Image materialImage;


   public void SetMaterial(MaterialRegistrySO.ObjectMaterial mat)
   {
      materialName.text = mat.type.ToString();
      materialImage.color = mat.color;
      _objectMaterial = mat;
   }
   
   public void SetSelected()
   {
      GameObject.FindObjectOfType<MaterialManager>().SelectItem(this);
   }

   public void Select()
   {
      selectedIndicator.gameObject.SetActive(true);
   }
   public void Deselect()
   {
      selectedIndicator.gameObject.SetActive(false);
   }

}
