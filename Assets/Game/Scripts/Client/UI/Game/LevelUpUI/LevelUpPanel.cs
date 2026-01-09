using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Skills;
using Game.Scripts.Services.UI;
using Michsky.MUIP;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game
{
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private ModalWindowManager _modalWindowManager;
        private UIService _uiService => Service<UIService>.Instance;
        private bool _opened => _modalWindowManager.isOn;
        [Header("UI")]
        [SerializeField] private GameObject _levelScreen;
        [SerializeField] private GameObject _waitingScreen;
        [SerializeField] private Transform _upgradeHolder;
        [SerializeField] private SkillLevelUp _skillLevelUpPrefab;


        public void ShowWaitOtherPlayerText()
        {
            _modalWindowManager.titleText = "Ожидание других игроков...";
            _modalWindowManager.UpdateUI();
            //_waitingScreen.SetActive(true);
        }
        public void HideWaitOtherPlayerText()
        {
          //  _waitingScreen.SetActive(false);
        }
        public void ShowLevelScreen()
        {
            _modalWindowManager.UpdateUI();
            foreach (Transform child in _upgradeHolder) 
                Destroy(child.gameObject);
            _levelScreen.SetActive(true);
        }
        public void HideLevelScreen()
        {
            _modalWindowManager.UpdateUI();
            _levelScreen.SetActive(false);
        }
        public void CloseWindow()
        {
           
            _modalWindowManager.CloseWindow();
            Cursor.visible = false;
            //HideWaitOtherPlayerText();
            HideLevelScreen();
            
        }
        public void CloseWindowImmediately()
        {
            _modalWindowManager.UpdateUI();
            _modalWindowManager.CloseWindow();
            _modalWindowManager.gameObject.SetActive(false);
            Cursor.visible = false;
           // HideWaitOtherPlayerText();
            HideLevelScreen();
            
        }
        public void OpenWaitWindow()
        {
            ShowWaitOtherPlayerText();
            foreach (Transform child in _upgradeHolder) 
                Destroy(child.gameObject);
            _modalWindowManager.OpenWindow();
            Cursor.visible = false;
            
        }

        public void OpenWindow(List<SkillData> skills,GameStateLevelUp state)
        {
            _modalWindowManager.titleText = "Выберите скилл";
            _modalWindowManager.UpdateUI();
          //  HideWaitOtherPlayerText();
            ShowLevelScreen();
            foreach (var skill in skills)
            {
                 var skillPanel = Instantiate(_skillLevelUpPrefab, _upgradeHolder);
                 skillPanel.Init(skill,state);
            }
            _modalWindowManager.OpenWindow();
            Cursor.visible = true;
            
        }
    }
}
