using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using System.Threading;

namespace SoftKitty.WSFL
{

    public class EditPrefabAssetScope : IDisposable
    {

        public readonly string assetPath;
        public readonly GameObject prefabRoot;

        public EditPrefabAssetScope(string assetPath)
        {
            this.assetPath = assetPath;
            prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
        }

        public void Dispose()
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    [System.Serializable]
    public class TextInfo
    {
        public enum ComponentTypes
        {
            SceneText,
            SceneTMP,
            PrefabText,
            PrefabTMP,
            Script,
            ScriptableObject
        }
        public ComponentTypes _type;
        public GameObject _obj;
        public string _objName;
        public string _filePath;
        public int _codeLine;
        public string _text;
        public bool _toggle = false;
        public bool _component = false;
        public UnityEngine.Object _asset; 
        public string _fieldPath;   
    }
    public class LocalizationTool : EditorWindow
    {
        public static LocalizationSettings CurrentSettings
        {
            get
            {
                EditorBuildSettings.TryGetConfigObject(LocalizationSettings.CONFIG_NAME, out LocalizationSettings settings);
                return settings;
            }
            set
            {
                var remove = value == null;
                if (remove)
                {
                    EditorBuildSettings.RemoveConfigObject(LocalizationSettings.CONFIG_NAME);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(LocalizationSettings.CONFIG_NAME, value, overwrite: true);
                }
            }
        }
        private string searchContext;
        private Dictionary<string, TextInfo> textDic = new Dictionary<string, TextInfo>();
        private Dictionary<string, TextInfo> scriptDic = new Dictionary<string, TextInfo>();
        Color _buttonColor = new Color(0.2F,1F,0.4F,1F);
        Color _buttonColor2 = new Color(0.3F, 0.6F, 0.8F, 1F);
        Color _titleColor = new Color(0.7F,0.95F,1F,1F);
        bool _toolFold = false;
        bool _componentFold = false;
        bool _codeFold = false;
        bool _fileFold = false;
        bool _componentNew = false;
        bool _codeNew = false;
        bool _pathChanged = true;
        bool _wasRunning = false;
        bool _metaNeedsUpdate = false;
        bool _defaultNeedsUpdate = false;
        Dictionary<string, bool> _needsToUpdate = new Dictionary<string, bool>();
        List<string> _localizationFiles=new List<string>();
        string _newName = "NewLanguage";
        Vector2 scrollPos0;
        Vector2 scrollPos1;
        string prefabSearchPath = "";
        string scriptableObjectSearchPath = "";



        void EditorGUI()
        {
            if(_wasRunning) Localization.UncatchedStringList.Clear();
            _wasRunning = false;
            scrollPos0=GUILayout.BeginScrollView(scrollPos0);
            EditorGUILayout.Separator();
            DrawCurrentSettingsGUI();
            EditorGUILayout.Space();
            var invalidSettings = CurrentSettings == null;
            if (invalidSettings) DisplaySettingsCreationGUI();
            EditorGUILayout.Separator();

            if (invalidSettings)
            {
                GUILayout.EndScrollView();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            int _selected = GetSelectedCount();
            int _setup = GetUnSetupCount();
            GUI.color = _titleColor;
            _toolFold = EditorGUILayout.Foldout(_toolFold, "Localization Texts (" + _selected.ToString() + ")");
            GUI.color = Color.white;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            if (_toolFold)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.backgroundColor = _buttonColor2;
                if (GUILayout.Button("Find Text/TMP components in the current scene"))
                {
                    SearchTextComponentsInTheScene();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUILayout.Label("Prefabs search path (Assets/...)", GUILayout.Width(230));
                prefabSearchPath = GUILayout.TextField(prefabSearchPath, GUILayout.Width(350));
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.backgroundColor = _buttonColor2;
                if (GUILayout.Button("Find Text/TMP components from prefabs in the project"))
                {
                    SearchTextComponentsInTheProject();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUILayout.Label("ScriptableObject search path (Assets/...)", GUILayout.Width(230));
                scriptableObjectSearchPath = GUILayout.TextField(scriptableObjectSearchPath, GUILayout.Width(350));
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.backgroundColor = _buttonColor2;
                if (GUILayout.Button("Find texts in ScriptableObjects"))
                {
                    SearchScriptableObjectsInProject();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.color = Color.green;
                if (_componentNew)
                    GUILayout.Label("New", GUILayout.Width(30));
                else
                    GUILayout.Space(40);
                GUI.color = Color.white;
                if (_componentNew) GUI.backgroundColor = Color.green;
                _componentFold = EditorGUILayout.Foldout(_componentFold, "Text Components List (" + textDic.Keys.Count.ToString() + ")");


                GUI.backgroundColor = Color.gray;
                GUI.color = Color.white;
                if (GUILayout.Button(new GUIContent("O", "Check All"), GUILayout.Width(25)))
                {
                    foreach (string _key in textDic.Keys) textDic[_key]._toggle = true;
                }
                if (GUILayout.Button(new GUIContent("X", "Uncheck All"), GUILayout.Width(25)))
                {
                    foreach (string _key in textDic.Keys) textDic[_key]._toggle = false;
                }

                GUI.color = _selected > 0 ? Color.white : Color.gray;
                if (textDic.Keys.Count > 0 && _setup > 0)
                {
                    GUI.backgroundColor = _selected > 0 ? Color.red : Color.gray;
                    if (GUILayout.Button("Setup All", GUILayout.Width(200)))
                    {
                        ApplyCompoentAll();
                    }
                }
                else
                {
                    GUILayout.Space(200);
                }
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                List<string> _removeList = new List<string>();
                if (_componentFold)
                {
                    _componentNew = false;
                    foreach (string _key in textDic.Keys)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        textDic[_key]._toggle = GUILayout.Toggle(textDic[_key]._toggle, "", GUILayout.Width(20));
                        GUI.color = CurrentSettings.LocalizationTextList.Contains(textDic[_key]._text) ? Color.grey : Color.green;
                        GUILayout.Label(new GUIContent((textDic[_key]._type == TextInfo.ComponentTypes.SceneText || textDic[_key]._type == TextInfo.ComponentTypes.PrefabText) ? "[Text]" : "[TMP]",
                           CurrentSettings.LocalizationTextList.Contains(textDic[_key]._text) ? "Existed" : "New"), GUILayout.Width(40));
                        GUI.color = Color.white;
                        GUI.backgroundColor = new Color(0.2F, 0.5F, 1F, 1F);

                        if (textDic[_key]._type == TextInfo.ComponentTypes.SceneText || textDic[_key]._type == TextInfo.ComponentTypes.SceneTMP)
                        {
                            if (textDic[_key]._obj != null)
                            {
                                if (GUILayout.Button(new GUIContent(ShortenString(textDic[_key]._obj.name, 10), textDic[_key]._obj.name), GUILayout.Width(100)))
                                {
                                    Selection.activeObject = textDic[_key]._obj;
                                }
                            }
                            else
                            {
                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("Missing", GUILayout.Width(100)))
                                {
                                    _removeList.Add(_key);
                                }
                                textDic[_key]._toggle = false;
                            }
                        }
                        else
                        {
                            if (AssetDatabase.LoadMainAssetAtPath(textDic[_key]._filePath) != null)
                            {
                                if (GUILayout.Button(new GUIContent(ShortenString(textDic[_key]._objName, 10), textDic[_key]._objName), GUILayout.Width(100)))
                                {
                                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(textDic[_key]._filePath);
                                }
                            }
                            else
                            {
                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("Missing", GUILayout.Width(100)))
                                {
                                    _removeList.Add(_key);
                                }
                                textDic[_key]._toggle = false;
                            }
                        }
                        GUI.backgroundColor = textDic[_key]._component ? Color.green : Color.red;
                        if (GUILayout.Button(new GUIContent(textDic[_key]._component ? "Ready" : "Setup", ""), GUILayout.Width(100)))
                        {
                            if (!textDic[_key]._component)
                            {
                                ApplyCompoent(textDic[_key]);
                            }
                        }
                        GUI.backgroundColor = Color.white;
                        GUILayout.Label(new GUIContent(ShortenString(textDic[_key]._text, 40), textDic[_key]._text), GUILayout.Width(300));
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();
                    }
                }

                foreach (string _key in _removeList)
                {
                    textDic.Remove(_key);
                }

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                EditorGUILayout.HelpBox("Before click this button, replace all string need Localization in your scripts with:\n" +
                    "Localization.GetString(string)\n" +
                    "Example:\n" +
                    "Replace \"Crafting Success!\" with Localization.GetString(\"Crafting Success!\") ", MessageType.Info, true);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.backgroundColor = _buttonColor2;
                if (GUILayout.Button("Find all texts in the scripts"))
                {
                    SearchScriptInTheProject();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.color = Color.green;
                if (_codeNew)
                    GUILayout.Label("New", GUILayout.Width(30));
                else
                    GUILayout.Space(40);
                GUI.color = Color.white;
                if (_codeNew) GUI.backgroundColor = Color.green;
                _codeFold = EditorGUILayout.Foldout(_codeFold, "Texts From Scripts List (" + scriptDic.Keys.Count.ToString() + ")");

                GUI.backgroundColor = Color.gray;
                GUI.color = Color.white;
                if (GUILayout.Button(new GUIContent("O", "Check All"), GUILayout.Width(25)))
                {
                    foreach (string _key in scriptDic.Keys) scriptDic[_key]._toggle = true;
                }
                if (GUILayout.Button(new GUIContent("X", "Uncheck All"), GUILayout.Width(25)))
                {
                    foreach (string _key in scriptDic.Keys) scriptDic[_key]._toggle = false;
                }
                GUILayout.Space(200);


                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                _removeList.Clear();
                if (_codeFold)
                {
                    _codeNew = false;
                    foreach (string _key in scriptDic.Keys)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        scriptDic[_key]._toggle = GUILayout.Toggle(scriptDic[_key]._toggle, "", GUILayout.Width(20));
                        GUI.color = CurrentSettings.LocalizationTextList.Contains(scriptDic[_key]._text) ? Color.grey : Color.green;
                        GUILayout.Label(new GUIContent("[Script]", CurrentSettings.LocalizationTextList.Contains(scriptDic[_key]._text) ? "Existed" : "New"), GUILayout.Width(50));
                        GUI.color = Color.white;
                        GUI.backgroundColor = new Color(0.2F, 0.5F, 1F, 1F);
                        if (AssetDatabase.LoadMainAssetAtPath(scriptDic[_key]._filePath) != null)
                        {
                            if (GUILayout.Button(new GUIContent(ShortenString(scriptDic[_key]._objName, 10), scriptDic[_key]._objName), GUILayout.Width(100)))
                            {
                                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(scriptDic[_key]._filePath);
                            }
                        }
                        else
                        {
                            GUI.backgroundColor = Color.red;
                            if (GUILayout.Button("Missing", GUILayout.Width(100)))
                            {
                                _removeList.Add(_key);
                            }
                            scriptDic[_key]._toggle = false;
                        }

                        GUI.backgroundColor = Color.white;
                        GUILayout.Label(new GUIContent(ShortenString(scriptDic[_key]._text, 40), scriptDic[_key]._text), GUILayout.Width(300));
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();
                    }
                }

                foreach (string _key in _removeList)
                {
                    scriptDic.Remove(_key);
                }

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);

                GUI.backgroundColor = _selected > 0 ? _buttonColor : Color.gray;
                if (_selected == 0) GUI.color = Color.gray;
                if (GUILayout.Button("Add all checked results (" + _selected.ToString() + ") to [Localization Texts] List (" + CurrentSettings.LocalizationTextList.Count.ToString() + ")"))
                {
                    if (_selected > 0) AddToList();
                }
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUI.color = _titleColor;
            _fileFold = EditorGUILayout.Foldout(_fileFold, "Localization Files (" + _localizationFiles.Count.ToString() + ")");
            GUI.color = Color.white;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (_pathChanged)
            {
                RefreshLocalizationFiles();
            }

            if (_fileFold)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                if (GUILayout.Button("Default Language: " + CurrentSettings.DefaultLanguageName, GUILayout.Width(400)))
                {
                    Selection.activeObject = CurrentSettings;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.color = new Color(0.2F,0.5F,1F,1F);
                GUILayout.Label("[Editor]", GUILayout.Width(45)) ;
                GUI.color = Color.white;
                GUILayout.Label("Localization files relative path:  (\"Assets\" +)");
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);

                if (CurrentSettings.LocalizationRelativePath == "")
                {
                    var script = MonoScript.FromScriptableObject(this);
                    string _thePath = AssetDatabase.GetAssetPath(script);
                    _thePath = _thePath.Replace("Editor/LocalizationTool.cs", "LocalizationFiles").Replace("Assets/", "/");
                    CurrentSettings.LocalizationRelativePath = _thePath;
                    CurrentSettings.ApplyData();
                    _pathChanged = true;
                }
                string _folderPath = GUILayout.TextField(CurrentSettings.LocalizationRelativePath, GUILayout.Width(500));
                if (_folderPath != CurrentSettings.LocalizationRelativePath)
                {
                    CurrentSettings.LocalizationRelativePath = _folderPath;
                    CurrentSettings.ApplyData();
                    _pathChanged = true;
                }
                if (GUILayout.Button("..", GUILayout.Width(30)))
                {
                    CurrentSettings.LocalizationRelativePath = SetPath(Application.dataPath + CurrentSettings.LocalizationRelativePath).Replace(Application.dataPath, "");
                    CurrentSettings.ApplyData();
                    _pathChanged = true;
                }

                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.color = new Color(0.2F, 1F, 0.3F, 1F);
                GUILayout.Label("[Build]", GUILayout.Width(45));
                GUI.color = Color.white;
                GUILayout.Label("Localization files relative path:  (Application.dataPath+)");
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);

               
                if (CurrentSettings.BuildLocalizationRelativePath == "")
                {
                    var script = MonoScript.FromScriptableObject(this);
                    string _thePath = AssetDatabase.GetAssetPath(script);
                    _thePath = _thePath.Replace("Editor/LocalizationTool.cs", "LocalizationFiles").Replace("Assets/", "/");
                    CurrentSettings.BuildLocalizationRelativePath = _thePath;
                    CurrentSettings.ApplyData();
                }
                else
                {
                    string _oldPath = CurrentSettings.BuildLocalizationRelativePath;
                    CurrentSettings.BuildLocalizationRelativePath = GUILayout.TextField(CurrentSettings.BuildLocalizationRelativePath, GUILayout.Width(500));
                    if(_oldPath!= CurrentSettings.BuildLocalizationRelativePath) CurrentSettings.ApplyData();
                }

                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                if (!_localizationFiles.Contains(CurrentSettings.DefaultLanguageName + ".txt"))
                {
                    GUI.backgroundColor = _buttonColor2;
                    if (GUILayout.Button("Create Default Language File (" + CurrentSettings.DefaultLanguageName + ".txt)", GUILayout.Width(350)))
                    {
                        WriteLocalizationFile(CurrentSettings.DefaultLanguageName + ".txt");
                    }
                    GUI.backgroundColor = Color.white;
                }
                if (!_localizationFiles.Contains("Meta.ini"))
                {
                    GUI.backgroundColor = _buttonColor2;
                    if (GUILayout.Button("Create Meta File", GUILayout.Width(150)))
                    {
                        WriteLocalizationFile("Meta.ini");
                    }
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                for (int i = 0; i < _localizationFiles.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    string _name = "";
                    if (_localizationFiles[i] == CurrentSettings.DefaultLanguageName + ".txt")
                    {
                        GUI.backgroundColor = _buttonColor2;
                        _name = "Default Language";
                    }
                    else if (_localizationFiles[i] == "Meta.ini")
                    {
                        GUI.backgroundColor = _buttonColor2;
                        _name = "Meta File";
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                        _name = _localizationFiles[i].Replace(".txt", "").Replace("_", " ");
                    }
                    if (GUILayout.Button(_localizationFiles[i], GUILayout.Width(200)))
                    {
                        string _path = Path.Combine("Assets/" + CurrentSettings.LocalizationRelativePath, _localizationFiles[i]).Replace(@"\", "/");
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(_path);
                    }
                    GUI.color = GUI.backgroundColor;
                    GUILayout.Label(_name, GUILayout.Width(150));


                    GUI.color = Color.white;
                    if (_name == "Default Language" || _name == "Meta File")
                    {
                        if ((_name == "Default Language" && _defaultNeedsUpdate) || (_name == "Meta File" && _metaNeedsUpdate))
                        {
                            GUI.backgroundColor = _buttonColor;
                            if (GUILayout.Button("Update", GUILayout.Width(100)))
                            {
                                WriteLocalizationFile(_localizationFiles[i]);
                                if (_name == "Default Language")_defaultNeedsUpdate = false;
                                if (_name == "Meta File") _metaNeedsUpdate = false;
                            }
                        }
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Recreate", GUILayout.Width(70)))
                        {
                            WriteLocalizationFile(_localizationFiles[i]);
                            if (_name == "Default Language") _defaultNeedsUpdate = false;
                            if (_name == "Meta File") _metaNeedsUpdate = false;
                        }
                    }
                    else
                    {
                        if (_needsToUpdate.ContainsKey(_localizationFiles[i]))
                        {
                            if (_needsToUpdate[_localizationFiles[i]]) {
                                GUI.backgroundColor = _buttonColor;
                                if (GUILayout.Button("Update", GUILayout.Width(100)))
                                {
                                    AppendLocalizationFile(_localizationFiles[i]);
                                    _needsToUpdate[_localizationFiles[i]] = false;
                                }
                            }
                        }
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        {
                            DeleteLocalizationFile(_localizationFiles[i]);
                        }
                        if (GUILayout.Button("Recreate", GUILayout.Width(70)))
                        {
                            WriteLocalizationFile(_localizationFiles[i]);
                            _needsToUpdate[_localizationFiles[i]] = false;
                        }
                    }

                    GUI.backgroundColor = Color.white;
                    GUI.color = Color.white;
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUILayout.Label("Add New Language", GUILayout.Width(120));
                _newName = GUILayout.TextField(_newName, GUILayout.Width(230));
                GUI.backgroundColor = _buttonColor;
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    WriteLocalizationFile(_newName + ".txt");
                }
                GUI.color = Color.white;
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

            }

            GUILayout.EndScrollView();
        }

        void RuntimeGUI()
        {
            _pathChanged = true;
            if (!_wasRunning)
            {
                RefreshLocalizationFiles();
            }
            _wasRunning = true;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.Label("Switch Language:");
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            for (int i = 0; i < _localizationFiles.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.backgroundColor = Localization.SelectedLanguage == i ? Color.green : Color.gray;
                if (GUILayout.Button(_localizationFiles[i].Replace(".txt","").Replace("_"," "),GUILayout.Width(200) ))
                {
                    Localization.SelectedLanguage = i;
                }
                GUI.backgroundColor = Color.white;
                if (_localizationFiles[i] == CurrentSettings.DefaultLanguageName + ".txt")
                {
                    GUILayout.Label("Default Language",GUILayout.Width(150));
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.Label("Uncatched string:", GUILayout.Width(130));
            if (Localization.UncatchedStringList.Count > 0)
            {
                GUI.backgroundColor = _buttonColor;
                if (GUILayout.Button("Add to [Localization Text] List", GUILayout.Width(250)))
                {
                    for (int i = 0; i < Localization.UncatchedStringList.Count; i++)
                    {
                        if (!CurrentSettings.LocalizationTextList.Contains(Localization.UncatchedStringList[i])) CurrentSettings.LocalizationTextList.Add(Localization.UncatchedStringList[i]);
                    }
                    Localization.UncatchedStringList.Clear();
                    CurrentSettings.ApplyData();
                    VerifyMetaAndDefaultFile();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            

            scrollPos1=GUILayout.BeginScrollView(scrollPos1);
            if (Localization.UncatchedStringList.Count > 0)
            {
                GUI.color = Color.red;
                for (int i = 0; i < Localization.UncatchedStringList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUILayout.Label(Localization.UncatchedStringList[i]);
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUILayout.Label("None");
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUI.color = Color.white;

            

            GUILayout.EndVertical();
        }
        void OnGUI()
        {
            GUIStyle box = GUI.skin.button;
            GUIStyle header = new GUIStyle();
            header.fontSize = 20;
            header.alignment = TextAnchor.MiddleCenter;

            if (Application.isPlaying)
            {
                RuntimeGUI();
            }
            else
            {
                EditorGUI();
            }
        }

        private void WriteLocalizationFile(string _fileName)
        {
            if (File.Exists(Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName))
            {
                File.Delete(Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName);
            }
            File.WriteAllLines(Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName,
                 CurrentSettings.LocalizationTextList, System.Text.Encoding.UTF8);
            RefreshLocalizationFiles();
            AssetDatabase.Refresh();
        }

        private void AppendLocalizationFile(string _fileName)
        {
            List<string> _result = new List<string>();
            string _path = Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName;
            int _startLine = 0;
            if (File.Exists(_path))
            {
                string[] _lines = File.ReadAllLines(_path, System.Text.Encoding.UTF8);
                _result.AddRange(_lines);
                _startLine =  _lines.Length;
                File.Delete(_path);
            }

            for (int i = _startLine; i < CurrentSettings.LocalizationTextList.Count;i++) {
                _result.Add(CurrentSettings.LocalizationTextList[i]);
            }

            File.WriteAllLines(_path, _result, System.Text.Encoding.UTF8);
            RefreshLocalizationFiles();
            AssetDatabase.Refresh();
        }

        private void DeleteLocalizationFile(string _fileName)
        {
            if (File.Exists(Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName))
            {
                File.Delete(Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _fileName);
            }
            RefreshLocalizationFiles();
            AssetDatabase.Refresh();
        }
        private void RefreshLocalizationFiles()
        {
            _localizationFiles.Clear();
            _localizationFiles.AddRange(Localization.ReadLocalizationFiles(Application.dataPath + CurrentSettings.LocalizationRelativePath));
            VerifyMetaAndDefaultFile();
            _pathChanged = false;
        }

        private void VerifyMetaAndDefaultFile()
        {
            for (int i = 0; i < _localizationFiles.Count; i++)
            {
                string _name = _localizationFiles[i].Replace(".txt", "").Replace(".ini", "").Replace("_", " ");

                string _path = Application.dataPath + CurrentSettings.LocalizationRelativePath + "/" + _localizationFiles[i];
                if (File.Exists(_path))
                {
                    string[] _lines = File.ReadAllLines(_path, System.Text.Encoding.UTF8);
                    bool _update = (_lines.Length != CurrentSettings.LocalizationTextList.Count);

                    if (_name == CurrentSettings.DefaultLanguageName)
                        _defaultNeedsUpdate = _update;
                    else if (_name == "Meta")
                        _metaNeedsUpdate = _update;
                    else if (_needsToUpdate.ContainsKey(_localizationFiles[i]))
                        _needsToUpdate[_localizationFiles[i]] = _update;
                    else
                        _needsToUpdate.Add(_localizationFiles[i], _update);
                }
            }
        }
        private string SetPath(string _defaultPath)
        {
            var path = EditorUtility.OpenFolderPanel("Please select the folder of the Localization files.", _defaultPath, "LocalizationExample");
            var invalidPath = string.IsNullOrEmpty(path);
            if (invalidPath) return null;
            return path;
        }

        private int GetSelectedCount()
        {
            int _result = 0;
            foreach (string _key in textDic.Keys)
            {
                if (textDic[_key]._toggle)
                {
                    _result++;
                }
            }
            foreach (string _key in scriptDic.Keys)
            {
                if (scriptDic[_key]._toggle)
                {
                    _result++;
                }
            }
            return _result;
        }

        private int GetUnSetupCount()
        {
            int _result = 0;
            foreach (string _key in textDic.Keys)
            {
                if (!textDic[_key]._component && textDic[_key]._toggle)
                {
                    _result++;
                }
            }
            return _result;
        }

        private string ShortenString(string _text, int _length)
        {
            return _text.Substring(0, Mathf.Min(_text.Length, _length)) + (_text.Length > _length ? ".." : "");
        }

        private void ApplyCompoent(TextInfo _info)
        {
            if (!_info._toggle)
                return;

            switch (_info._type)
            {
                case TextInfo.ComponentTypes.SceneText:
                    ApplySceneText(_info);
                    break;

                case TextInfo.ComponentTypes.SceneTMP:
                    ApplySceneTMP(_info);
                    break;

                case TextInfo.ComponentTypes.PrefabText:
                case TextInfo.ComponentTypes.PrefabTMP:
                    ApplyPrefabText(_info);
                    break;

                case TextInfo.ComponentTypes.ScriptableObject:
                    ApplyScriptableObject(_info);
                    break;

                case TextInfo.ComponentTypes.Script:
                    // ничего не делаем
                    break;
            }
        }
        private void ApplySceneText(TextInfo info)
        {
            if (info._obj == null) return;

            if (!info._obj.GetComponent<LocalizationTextLegacy>())
                info._obj.AddComponent<LocalizationTextLegacy>();

            EditorUtility.SetDirty(info._obj);
            info._component = true;
        }
        private void ApplySceneTMP(TextInfo info)
        {
            if (info._obj == null) return;

            if (!info._obj.GetComponent<LocalizationTMP>())
                info._obj.AddComponent<LocalizationTMP>();

            EditorUtility.SetDirty(info._obj);
            info._component = true;
        }
        private void ApplyPrefabText(TextInfo info)
        {
            if (!File.Exists(info._filePath)) return;

            using (var editScope = new EditPrefabAssetScope(info._filePath))
            {
                foreach (var t in editScope.prefabRoot.GetComponentsInChildren<Text>(true))
                {
                    if (t.text == info._text && !t.GetComponent<LocalizationTextLegacy>())
                        t.gameObject.AddComponent<LocalizationTextLegacy>();
                }

                foreach (var t in editScope.prefabRoot.GetComponentsInChildren<TMPro.TMP_Text>(true))
                {
                    if (t.text == info._text && !t.GetComponent<LocalizationTMP>())
                        t.gameObject.AddComponent<LocalizationTMP>();
                }
            }

            info._component = true;
        }
        private void ApplyScriptableObject(TextInfo info)
        {
            // Просто помечаем как обработанный
            info._component = true;
        }




        private void ApplyCompoentAll()
        {
            foreach (string _key in textDic.Keys)
            {
                ApplyCompoent(textDic[_key]);
            }
        }

        private void AddToList()
        {
            int _result=0;
            for (int i=0;i<_localizationFiles.Count;i++) {
                string _name = _localizationFiles[i].Replace(".txt", "").Replace(".ini", "").Replace("_", " ");
                if (_name != CurrentSettings.DefaultLanguageName && _name!= "Meta" && !CurrentSettings.LocalizationTextList.Contains(_name))
                    CurrentSettings.LocalizationTextList.Add(_name);
            }
            ApplyCompoentAll();
            foreach (string _key in textDic.Keys)
            {
                if (textDic[_key]._toggle && !CurrentSettings.LocalizationTextList.Contains(textDic[_key]._text))
                {
                    CurrentSettings.LocalizationTextList.Add(textDic[_key]._text);
                    _result++;
                    textDic[_key]._toggle = false;
                }
            }
            foreach (string _key in scriptDic.Keys)
            {
                if (scriptDic[_key]._toggle && !CurrentSettings.LocalizationTextList.Contains(scriptDic[_key]._text))
                {
                    CurrentSettings.LocalizationTextList.Add(scriptDic[_key]._text);
                    _result++;
                    scriptDic[_key]._toggle = false;
                }
            }
            _codeNew = false;
            _componentNew = false;
            CurrentSettings.ApplyData();
            VerifyMetaAndDefaultFile();
            EditorUtility.DisplayDialog("Localization Tool", "Added (" + _result + ") NEW texts to the [Localization Texts] List.", "Confirm");
        }

        private void SearchTextComponentsInTheScene()
        {
            Text [] _texts= UnityEngine.Object.FindObjectsOfType<Text>(true);
            TMPro.TMP_Text [] _tmps = UnityEngine.Object.FindObjectsOfType<TMPro.TMP_Text>(true);
            int _count = _texts.Length + _tmps.Length;
            int _step = 0;
            int _result = 0;
            for (int i=0;i< _texts.Length;i++) {
                _step++;
                EditorUtility.DisplayProgressBar("Localization Tool", "Searching Text Components in the scene...", _step*1F / _count);
                string _text = _texts[i].text.Replace("\r", "<br>").Replace("\n", "<br>");
                if (!textDic.ContainsKey(Localization.GetKey(_text))) {
                    TextInfo _info = new TextInfo();
                    _info._type = TextInfo.ComponentTypes.SceneText;
                    _info._obj = _texts[i].gameObject;
                    _info._text = _text;
                    _info._toggle = !CurrentSettings.LocalizationTextList.Contains(_texts[i].text);
                    _info._component = _texts[i].gameObject.GetComponent<LocalizationTextLegacy>();
                    textDic.Add(Localization.GetKey(_text), _info);
                    _result++;
                }
                Thread.Sleep((int)(0.01F * 1000F));
            }

            for (int i = 0; i < _tmps.Length; i++)
            {
                _step++;
                EditorUtility.DisplayProgressBar("Localization Tool", "Searching TMP Components in the scene...", _step * 1F / _count);
                string _text = _tmps[i].text.Replace("\r", "<br>").Replace("\n", "<br>");
                if (!textDic.ContainsKey(Localization.GetKey(_text)))
                {
                    TextInfo _info = new TextInfo();
                    _info._type = TextInfo.ComponentTypes.SceneTMP;
                    _info._obj = _tmps[i].gameObject;
                    _info._text = _text;
                    _info._toggle = !CurrentSettings.LocalizationTextList.Contains(_tmps[i].text);
                    _info._component = _tmps[i].gameObject.GetComponent<LocalizationTMP>();
                    textDic.Add(Localization.GetKey(_text), _info);
                    _result++;
                }
                Thread.Sleep((int)(0.01F * 1000F));
            }

            EditorUtility.ClearProgressBar();
            if (_result > 0) _componentNew = true;
            EditorUtility.DisplayDialog("Localization Tool Search Result", "Found ("+ _result + ") NEW Text/TMP Components in the current scene.","Confirm");
        }

        private void SearchTextComponentsInTheProject()
        {
            string _mainPath = "Assets";
            string[] folders = GetSearchFolders(prefabSearchPath);
            string[] guids = AssetDatabase.FindAssets("t:Prefab", folders);
            int _count = guids.Length;
            int _step = 0;
            int _result = 0;
            foreach (string _guid in guids)
            {
                _step++;
                EditorUtility.DisplayProgressBar("Localization Tool", "Searching TMP Components in the project...", _step * 1F / _count);
                string _path = AssetDatabase.GUIDToAssetPath(_guid);
                GameObject _obj= (GameObject)AssetDatabase.LoadAssetAtPath(_path, typeof(GameObject));
                foreach (var _text in _obj.GetComponentsInChildren<Text>(true)) {
                    string _content = _text.text.Replace("\r", "<br>").Replace("\n", "<br>");
                    if (!textDic.ContainsKey(Localization.GetKey(_content)))
                    {
                        TextInfo _info = new TextInfo();
                        _info._type = TextInfo.ComponentTypes.PrefabText;
                        _info._filePath = _path;
                        _info._objName = _text.gameObject.name;
                        _info._text = _content;
                        _info._toggle = !CurrentSettings.LocalizationTextList.Contains(_text.text);
                        _info._component = _text.gameObject.GetComponent<LocalizationTextLegacy>();
                        textDic.Add(Localization.GetKey(_content), _info);
                        _result++;
                    }
                }
                foreach (var _text in _obj.GetComponentsInChildren<TMPro.TMP_Text>(true))
                {
                    string _content = _text.text.Replace("\r", "<br>").Replace("\n", "<br>");
                    if (!textDic.ContainsKey(Localization.GetKey(_content)))
                    {
                        TextInfo _info = new TextInfo();
                        _info._type = TextInfo.ComponentTypes.PrefabTMP;
                        _info._filePath = _path;
                        _info._objName = _text.gameObject.name;
                        _info._text = _content;
                        _info._toggle = !CurrentSettings.LocalizationTextList.Contains(_text.text);
                        _info._component = _text.gameObject.GetComponent<LocalizationTMP>();
                        textDic.Add(Localization.GetKey(_content), _info);
                        _result++;
                    }
                }
                Thread.Sleep((int)(0.01F * 1000F));
            }
            EditorUtility.ClearProgressBar();
            if (_result > 0) _componentNew = true;
            EditorUtility.DisplayDialog("Localization Tool Search Result", "Found (" + _result + ") NEW Text/TMP Components from all prefabs in the project.", "Confirm");
        }

        private void SearchScriptInTheProject()
        {
            string _mainPath = "Assets";
            string[] Folders = AssetDatabase.GetSubFolders(_mainPath);
            string[] guids = AssetDatabase.FindAssets("t:Script", Folders);
            int _count = guids.Length;
            int _step = 0;
            int _result = 0;
            foreach (string _guid in guids)
            {
                _step++;
                EditorUtility.DisplayProgressBar("Localization Tool", "Searching text of scripts in the project...", _step * 1F / _count);
                string _path = AssetDatabase.GUIDToAssetPath(_guid);
                if (File.Exists(_path) && 
                    !_path.Contains("LocalizationTool.cs") && !_path.Contains("Localization.cs") && !_path.Contains("LocalizationTextLegacy.cs") && !_path.Contains("LocalizationTMP.cs")) {
                    string [] _scriptLines = File.ReadAllLines(_path, System.Text.Encoding.UTF8);
                    for (int i = 0; i < _scriptLines.Length; i++)
                    {
                        int _start = 0;
                        string _searchText = "Localization.GetString";
                        int _timeout = 0;
                        while (_start >= 0 && _timeout < 30)
                        {
                            _start = _scriptLines[i].IndexOf(_searchText, _start);
                            if (_start >= 0)
                            {
                                int _index1 = _scriptLines[i].IndexOf("\"", _start + _searchText.Length) + 1;
                                if (_index1 > _start && _index1<_start+25)
                                {
                                    int _index2 = _scriptLines[i].IndexOf("\"", _index1);
                                    if (_index2 > _index1)
                                    {
                                        string _text = _scriptLines[i].Substring(_index1, _index2 - _index1).Replace("\r", "<br>").Replace("\n", "<br>");
                                        _start = _index2;
                                        if (!scriptDic.ContainsKey(Localization.GetKey(_text)))
                                        {
                                            TextInfo _info = new TextInfo();
                                            _info._type = TextInfo.ComponentTypes.Script;
                                            _info._filePath = _path;
                                            _info._objName = Path.GetFileName(_path);
                                            _info._text = _text;
                                            _info._codeLine = i+1;
                                            _info._toggle = !CurrentSettings.LocalizationTextList.Contains(_text);
                                            scriptDic.Add(Localization.GetKey(_text), _info);
                                            _result++;
                                        }
                                    }
                                }
                            }
                            _timeout++;
                        }
                    }
                }

                Thread.Sleep((int)(0.01F * 1000F));
            }
            EditorUtility.ClearProgressBar();
            if (_result > 0) _codeNew = true;
            EditorUtility.DisplayDialog("Localization Tool Search Result", "Found (" + _result + ") text from all scripts in the project.", "Confirm");
        }

        private void SearchScriptableObjectsInProject()
        {
            string[] folders = GetSearchFolders(scriptableObjectSearchPath);
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", folders);
            int _count = guids.Length;
            int _step = 0;
            int _result = 0;
            

            foreach (string guid in guids)
            {
                _step++;
                EditorUtility.DisplayProgressBar(
                    "Localization Tool",
                    "Searching ScriptableObjects...",
                    _step * 1f / _count
                );

                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                SerializedObject so = new SerializedObject(asset);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.String &&
                        !string.IsNullOrEmpty(prop.stringValue))
                    {
                        string text = prop.stringValue
                            .Replace("\r", "<br>")
                            .Replace("\n", "<br>");

                        string key = Localization.GetKey(text);
                        if (textDic.ContainsKey(key)) continue;

                        var info = new TextInfo
                        {
                            _type = TextInfo.ComponentTypes.ScriptableObject,
                            _asset = asset,
                            _filePath = path,
                            _objName = asset.name,
                            _fieldPath = prop.propertyPath,
                            _text = text,
                            _toggle = !CurrentSettings.LocalizationTextList.Contains(text),
                            _component = true // тут всегда true
                        };

                        textDic.Add(key, info);
                        _result++;
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            if (_result > 0) _componentNew = true;

            EditorUtility.DisplayDialog(
                "Localization Tool",
                $"Found ({_result}) texts in ScriptableObjects.",
                "OK"
            );
        }
        private string[] GetSearchFolders(string path)
        {
            if (string.IsNullOrEmpty(path))
                return new[] { "Assets" };

            path = path.Replace("\\", "/");

            if (!path.StartsWith("Assets"))
            {
                EditorUtility.DisplayDialog(
                    "Localization Tool",
                    "Search path must start with 'Assets/'",
                    "OK"
                );
                return new[] { "Assets" };
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog(
                    "Localization Tool",
                    $"Folder not found:\n{path}",
                    "OK"
                );
                return new[] { "Assets" };
            }

            return new[] { path };
        }




        private void DrawCurrentSettingsGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            var settings = EditorGUILayout.ObjectField("Current Settings", CurrentSettings,
                typeof(LocalizationSettings), allowSceneObjects: false) as LocalizationSettings;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            if (settings) DrawCurrentSettingsMessage();
           
           
        }


        private void DisplaySettingsCreationGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            const string message = "You have no Localization Settings. Would you like to create one?";
            EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUI.backgroundColor = _buttonColor;
            var openCreationdialog = GUILayout.Button("Create");
            GUI.backgroundColor = Color.white;
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            if (openCreationdialog)
            {
                CurrentSettings = SaveLocalizationAsset();
            }
        }

        private static LocalizationSettings SaveLocalizationAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                title: "Save Localization Settings", defaultName: "DefaultLocalizationSettings", extension: "asset",
                message: "Please enter a filename to save the projects Localization settings.");
            var invalidPath = string.IsNullOrEmpty(path);
            if (invalidPath) return null;

            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = settings;
            return settings;
        }

        private void DrawCurrentSettingsMessage()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            const string message = "This is the current Localization Settings and " +
                "it will be automatically included into any builds.";
            EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }


    }

}
