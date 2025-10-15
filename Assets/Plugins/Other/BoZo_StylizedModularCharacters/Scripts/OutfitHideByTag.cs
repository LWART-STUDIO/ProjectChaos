using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bozo.ModularCharacters
{
    public class OutfitHideByTag : MonoBehaviour
    {
        OutfitSystem system;
        [SerializeField] HideSettings[] settings;

        public void OnEnable()
        {
            system = GetComponentInParent<OutfitSystem>();
            system.OnOutfitChanged += SetHide;
        }

        public void OnDisable()
        {
            system = GetComponentInParent<OutfitSystem>(true);
            system.OnOutfitChanged -= SetHide;
        }

        private void SetHide(Outfit arg0)
        {
            foreach (var item in settings)
            {
                if (system.ContainsTag(item.tag))
                {
                    item.renderer.gameObject.SetActive(false);
                }
                else
                {
                    item.renderer.gameObject.SetActive(true);
                }
            }
        }

        [System.Serializable]
        private struct HideSettings
        {
            public SkinnedMeshRenderer renderer;
            public string tag;
        } 
    }
}
