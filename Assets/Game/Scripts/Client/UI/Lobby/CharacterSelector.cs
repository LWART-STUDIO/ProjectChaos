using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Client.UI.Lobby
{
    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField] private List<CharacterCard> _cards;

        private void OnEnable()
        {
            _cards=new List<CharacterCard>();
            _cards.AddRange(transform.GetComponentsInChildren<CharacterCard>());
            SetUp();
        }

        private void SetUp()
        {
            for (var index = 0; index < _cards.Count; index++)
            {
                var card = _cards[index];
                card.SetUp(index);
                card.WasSelected += CharacterSelect;
            }
        }

        public void CharacterSelect(CharacterCard card)
        {
            for (var index = 0; index < _cards.Count; index++)
            {
                var localcCard = _cards[index];
               if(localcCard==card)
                   continue;
               localcCard.Unselect();
            }
        }

        private void OnDisable()
        {
            for (var index = 0; index < _cards.Count; index++)
            {
                var card = _cards[index];
                card.WasSelected -= CharacterSelect;
            }
        }
    }
}
