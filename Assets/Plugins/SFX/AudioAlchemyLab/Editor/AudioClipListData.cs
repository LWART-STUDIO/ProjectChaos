using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AudioAlchemy.AudioTools
{
    [CreateAssetMenu(fileName = "MyAudioClipList", menuName = "Assets/Plugins/SFX/AudioAlchemyLab/MyAudioClipList", order = 1)]
    public class AudioClipListData : ScriptableObject
    {
        [SerializeField]
        private List<AudioClip> favoriteClips = new List<AudioClip>();

        [SerializeField]
        private int selectedSortingOption = 0;

        [SerializeField]
        private int selectedClipIndex;

        [SerializeField]
        private bool isFavoritesActive = false;

        [SerializeField]
        private List<string> clipOrder = new List<string>();

        [SerializeField]
        private List<string> selectedAudioSourceNames = new List<string>();

        // New fields for storing individual audio source settings
        [SerializeField]
        private Dictionary<string, float> audioSourceVolumes = new Dictionary<string, float>();

        [SerializeField]
        private Dictionary<string, float> audioSourcePitches = new Dictionary<string, float>();

        [SerializeField]
        private Dictionary<string, bool> audioSourceLoops = new Dictionary<string, bool>();

        [SerializeField]
        private Dictionary<string, bool> audioSourcePlayOnAwake = new Dictionary<string, bool>();

        // Properties to access these fields
        public List<AudioClip> FavoriteClips => favoriteClips;
        public int SelectedSortingOption { get => selectedSortingOption; set => selectedSortingOption = value; }
        public int SelectedClipIndex { get => selectedClipIndex; set => selectedClipIndex = value; }
        public bool IsFavoritesActive { get => isFavoritesActive; set => isFavoritesActive = value; }
        public List<string> ClipOrder => clipOrder;
        public List<string> SelectedAudioSourceNames
        {
            get => selectedAudioSourceNames;
            set => selectedAudioSourceNames = value;
        }

        public Dictionary<string, float> AudioSourceVolumes => audioSourceVolumes;
        public Dictionary<string, float> AudioSourcePitches => audioSourcePitches;
        public Dictionary<string, bool> AudioSourceLoops => audioSourceLoops;
        public Dictionary<string, bool> AudioSourcePlayOnAwake => audioSourcePlayOnAwake;

        // Example of a method that might need the EditorUtility class.
        public void SaveData()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
