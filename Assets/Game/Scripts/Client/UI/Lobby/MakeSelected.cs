using System;
using UnityEngine;

namespace Game.Scripts.Client.UI
{
    public class MakeSelected : MonoBehaviour
    {
        public event Action<int> OnClick;
        [SerializeField] private CanvasGroup _canvasGroup;
        private int _id = 0;

        public void SetUp(int id)
        {
            _id = id;
        }

        public void Select()
        {
            OnClick?.Invoke(_id);
            _canvasGroup.alpha = 1;
        }
        public void Deselect()
        {
            _canvasGroup.alpha = 0;
        }
    }
}
