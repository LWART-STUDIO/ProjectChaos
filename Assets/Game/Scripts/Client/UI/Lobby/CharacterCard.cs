using System;
using Michsky.MUIP;
using UnityEngine;

namespace Game.Scripts.Client.UI.Lobby
{
    public class CharacterCard : MonoBehaviour
    {
        [SerializeField] private MakeSelected _makeSelected;
        [SerializeField] private ButtonManager _buttonManager;
        public event Action<CharacterCard> WasSelected;
        private int _id;

        private void OnEnable()
        {
            _makeSelected.OnClick += MakeSelected;
        }

        public void SetUp(int id)
        {
            _buttonManager.buttonText = "Имя персонажа";
            _id = id;
            _makeSelected.SetUp(_id);
        }
        private void MakeSelected(int characterId)
        {
            WasSelected?.Invoke(this);
        }

        public void Unselect()
        {
            _makeSelected.Deselect();
        }

        private void OnDisable()
        {
            _makeSelected.OnClick -= MakeSelected;
        }
        
    }
}
