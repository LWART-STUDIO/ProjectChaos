using SaintsField.Playa;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI
{
   public class MaterialTimeWraper : MonoBehaviour
   {
      private Material _material;


      [ExecuteAlways]
      private void Update()
      {
         if (_material == null)
         {
            GetMaterial();
            if (_material == null)
               return;
         }
            
           
         _material.SetFloat("_UnscaledTime", _material.GetFloat("_UnscaledTime") + Time.unscaledDeltaTime);
      }
      [Button]
      private void GetMaterial()
      {
         TryGetComponent<Renderer>(out var renderer);
         if (renderer != null)
         {
            _material = renderer.material;
            return;
         }
          
         TryGetComponent<Image>(out var image);
         if (image != null)
         {
            _material = image.material;
            return;
         }
      }
   
   }
}
