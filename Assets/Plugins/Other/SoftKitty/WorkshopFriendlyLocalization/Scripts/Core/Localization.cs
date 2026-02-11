using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SoftKitty.WSFL
{
    public class Localization : MonoBehaviour
    {
        public static int SelectedLanguage = 0;//The current language index of the string array returned from ReadLocalizationFiles(string _absulutePath).

        #region Variables
        public static Dictionary<string, string [] > LocalizationDic = new Dictionary<string, string[]>();
        public static LocalizationSettings CurrentSettings;
        public static List<string> UncatchedStringList = new List<string>();
        private static Dictionary<string, int> LocalizationIndex = new Dictionary<string, int>();
        #endregion

        #region Internal Methods
        static int SortByIndex(string p1, string p2)
        {
            return LocalizationIndex[p1].CompareTo(LocalizationIndex[p2]);
        }

        public static string GetKey(string _text)
        {
            return _text.Substring(0, Mathf.Min(_text.Length, 50)).Trim();
        }
        #endregion

        public static string [] ReadLocalizationFiles(string _absulutePath)//Initialize by reading Localization files from the path
        {
            string _path = _absulutePath.Replace(@"\","/");
            if (!_path.EndsWith("/")) _path += "/";
     
            LocalizationDic.Clear();
            LocalizationIndex.Clear();

            CurrentSettings=LocalizationSettings.Load();
           

            string [] _files = Directory.GetFiles(_path, "*.txt");
            List<string> _fileList = new List<string>();

            if (!Application.isPlaying)
            {
                string _file = _path + "Meta.ini";
                if (File.Exists(_file))
                {
                    FileInfo _info = new FileInfo(_file);
                    _fileList.Add(_info.Name);
                    LocalizationIndex.Add("Meta.ini", -1);
                }
            }
            LocalizationIndex.Add(CurrentSettings.DefaultLanguageName + ".txt", 0);
            foreach (string _file in _files) {
                FileInfo _info = new FileInfo(_file);
                _fileList.Add(_info.Name);
                if (!LocalizationIndex.ContainsKey(_info.Name))
                {
                    char _first = _info.Name.ToCharArray()[0];
                    LocalizationIndex.Add(_info.Name, (int)_first);
                }
            }
            
            _fileList.Sort(SortByIndex);

            if (File.Exists(_path + "Meta.ini"))
            {
                string[] _keyContents = File.ReadAllLines(_path + "Meta.ini", System.Text.Encoding.UTF8);
                for (int i = 0; i < _fileList.Count; i++)
                {
                    string[] _contents = File.ReadAllLines(_path + _fileList[i]);
                    for (int u = 0; u < _keyContents.Length; u++)
                    {
                        string _key = GetKey(_keyContents[u]);
                        if (_key.Length > 0)
                        {
                            if (!LocalizationDic.ContainsKey(_key))
                            {
                                LocalizationDic.Add(_key, new string[_fileList.Count]);
                                LocalizationDic[_key][0] = _key;
                            }

                            if (u < _contents.Length)
                                LocalizationDic[_key][i] = _contents[u];
                            else
                                LocalizationDic[_key][i] = _key;
                        }
                    }
                }
            }

            return _fileList.ToArray();
        }


        //Get the translated text of the current language.Set the _forceLanguage to any index number other than -1 to return the translated text of specified language
        public static string GetString(string _text,int _forceLanguage=-1)
        {
            if (LocalizationDic.ContainsKey(GetKey(_text)))
            {
                return LocalizationDic[GetKey(_text)][_forceLanguage == -1 ? SelectedLanguage : _forceLanguage].Replace("<br>", "\n");
            }
            else
            {
#if UNITY_EDITOR
                if (!UncatchedStringList.Contains(_text.Replace("\r", "<br>").Replace("\n", "<br>"))) UncatchedStringList.Add(_text.Replace("\r", "<br>").Replace("\n", "<br>"));
               #endif
            }
            return _text;
        }


    }
}
