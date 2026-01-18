using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI; // Add this for UI components like ScrollRect

namespace AudioAlchemy.AudioTools
{
    public class AudioAlchemyLab : EditorWindow
    {
        #region Singleton Instance
        private static AudioAlchemyLab _instance;
        private static readonly object _lock = new object();

        public static AudioAlchemyLab Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (AudioAlchemyLab)GetWindow(typeof(AudioAlchemyLab));
                        _instance.titleContent = new GUIContent("Audio Alchemy Lab");
                        _instance.OnEnable(); // Call OnEnable to initialize things when the instance is created
                    }
                    return _instance;
                }
            }
        }
        #endregion

        #region Audio Management
        // Audio Clip Management
        private List<AudioClip> audioClips = new List<AudioClip>(); // All available audio clips
        private List<AudioClip> favorites = new List<AudioClip>(); // User's favorite audio clips
        private List<AudioClip> previousClips = new List<AudioClip>(); // To restore previous state
        private List<AudioClip> filteredAudioClips = new List<AudioClip>(); // Filtered list based on search

        // Audio Clip Playback Settings
        private int currentClipIndex = 0; // Index of the currently selected clip
        private string selectedClipName = ""; // Name of the currently selected clip
        private float volume = 1.0f; // Global volume setting
        private float pitch = 1.0f; // Global pitch setting
        private bool isLooping = false; // Whether the current clip is looping
        private float loopDelay = 1f; // Delay between loops
        private bool isPaused = false; // Whether playback is paused
        private bool wasPlaying = false;

        private float pausedTime = 0f; // Time position when paused
        private float nextPlayTime; // Time for next scheduled play
        private float crossfadeDuration = 1.0f; // Duration of the crossfade in seconds

        [SerializeField] private bool playOnAwakeEnabled = false; // Whether clips play on awake

        // Audio Configuration Settings
        private int bufferSize = 512; // Default buffer size
        private int minBufferSize = 256; // Minimum buffer size
        private int maxBufferSize = 2048; // Maximum buffer size for future-proofing
        #endregion

        #region Audio Source Management
        // Audio Source Management
        private AudioSource sfxAudioSource; // Audio source for playing SFX
        private AudioClipListData audioClipListData; // Data container for audio clips
        private List<AudioSource> audioSourcesInScene = new List<AudioSource>(); // Audio sources in the scene
        private List<bool> audioSourceToggles = new List<bool>(); // Toggles for enabling/disabling audio sources
        private Vector2 audioSourceScrollPosition; // Scroll position for the audio sources list
        private Vector2 audioClipScrollPosition; // Scroll position for the audio clips list
        private bool showAudioSources = true; // Whether to show the audio sources section
        #endregion

        #region SFX Clips
        // Sound Effects Clips
        private AudioClip logoClickSFX; // SFX for logo click
        private AudioClip folderClickSFX; // SFX for folder click
        private AudioClip openSFX; // SFX for opening
        private AudioClip closeSFX; // SFX for closing
        private AudioClip trashSFX; // SFX for trash action
        private AudioClip heartSFX; // SFX for heart action (favorites)
        private AudioClip settingsSFX; // SFX for settings and logo clicks
        private AudioClip toggleSFX; // SFX for toggling audio sources
        private AudioClip searchSpellSFX; // SFX for the Arcane Detection spell
        private AudioClip clearAllSFX; // SFX for clearing all selections
        private AudioClip cancelSFX; // SFX for canceling actions

        #endregion

        #region UI and Display Settings
        // UI Layout and Display Settings
        private Vector2 scrollPosition; // Scroll position for the main UI
        private float scrollViewHeight; // Height of the scroll view
        private float heightOfElementsAbove = 200f; // Height of elements above the scroll view (approximate)
        private string versionNumber = "v5.2"; // Version number displayed in the UI
        private string searchString = ""; // Current search string input
        private Color selectedTextColor = Color.green; // Text color for selected items
        private Color defaultTextColor = Color.black; // Default text color
        private bool showLogo = true; // Whether to display the logo
        private bool isClosing = false; // Whether the application is closing
        private bool playSequentially = false; // Whether to play clips sequentially
        private bool playRandomly = false; // Whether to play clips randomly
        #endregion

        #region Sorting Options
        // Sorting Options
        private int selectedSortingOption = 0; // 0: Alphabetical, 1: Numerical, 2: Date Added, 3: Length
        private List<bool> togglesBeforeSorting;
        private GUIContent[] sortingIcons; // Array to hold sorting icons
        private AudioClip sortSFX; // Audio clip for sorting SFX
        private bool hasSorted = false; // Flag to check if sorting has occurred
        #endregion

        #region Section Expansion Toggles
        // Section Expansion Toggles
        private bool sectionOneExpanded = true; // Toggle for additional sections
        #endregion

        #region Animation and Effects
        // Animation and Effects
        private bool isAnimating = false; // Whether an animation is currently playing
        private float animationDuration = 3f; // Duration of the animation
        private float animationTimer = 0f; // Timer for animations
        private float[] samples = new float[1024]; // Array for storing audio samples
        #endregion

        #region UI Styles
        // UI Styles
        private Color basePurpleColor; // Base color for the UI
        private Color hoverPurpleColor; // Hover color for the UI
        private Color clickPurpleColor; // Click color for the UI
        private GUIStyle settingsButtonStyle; // GUIStyle for the settings button
        private GUIStyle xButtonStyle; // GUIStyle for the X button
        #endregion

        #region Sorting Flag
        // Sorting Flag
        private bool needsSorting = false;  // Flag to indicate when sorting is needed
        #endregion

        #region KeyboardHandling
        // At the top of your AudioAlchemyLab class
        private AudioClipKeyboardHandler keyboardHandler;
        #endregion



        #region Menu Item
        // Menu Item to Open the Window
        [MenuItem("Tools/Audio Alchemy Lab")]
        public static void ShowWindow()
        {
            _ = Instance; // Access the Instance to ensure the window is created and shown
        }
        #endregion

        private void OnEnable()
        {
            LoadState(); // Load any previously saved state
            LoadSelectedAudioSources(); // Load the toggled audio sources
            bool wasFavoritesActive = EditorPrefs.GetBool("AudioAlchemy_IsFavoritesActive", false);
            if (wasFavoritesActive)
            {
                ToggleFavorites();  // Activate favorites view if it was active before closing
            }

            LoadSelectedAudioSources(); // Load the saved audio sources and their settings

            // Delay call to ensure Unity is ready before continuing
            EditorApplication.delayCall += () =>
            {
                ContinueInitialization(); // Proceed with additional setup
                LoadOrCreateAudioClipListData(); // Load or create the audio clip list asset
                PlayOpeningSFX(); // Play the opening sound effect
            };

            keyboardHandler = new AudioClipKeyboardHandler(this); // Initialize keyboard handling

            LoadSFXAssets(); // Load sound effects
            InitializeOrRestoreAudioSource(); // Initialize or restore the audio source
            SortToggleIntialize(); // Initialize sorting icons

            minSize = new Vector2(200, 600); // Set the minimum size of the window
        }


        private void LoadSFXAssets()
        {
            try
            {
                logoClickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/KeyPress.wav");
                folderClickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/KeyPress.wav");
                openSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/AALClip.wav");
                closeSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/GoodBye.wav");
                heartSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/Heart.wav");
                trashSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/Trash.wav");
                searchSpellSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/SearchSpell.wav");
                settingsSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/settingsSFX.wav");
                toggleSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/toggleSFX.wav");
                sortSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/sortSFX.wav");
                clearAllSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/ClearAll.wav");
                cancelSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Plugins/SFX/AudioAlchemyLab/SFX/Cancel.wav");

            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading SFX assets: {ex.Message}");
            }
        }

        private void PlayOpeningSFX()
        {
            // Play the opening sound effect if available
            if (openSFX != null && sfxAudioSource != null)
            {
                sfxAudioSource.clip = openSFX;
                sfxAudioSource.Play();
            }
        }



        private void SortToggleIntialize()
        {
            // Initialize sorting icons with their respective images and tooltips
            sortingIcons = new GUIContent[4];
            sortingIcons[0] = new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/SortAlphabeticalAsc.png") as Texture2D, "Sort Alphabetically Ascending");
            sortingIcons[1] = new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/SortAlphabeticalDesc.png") as Texture2D, "Sort Alphabetically Descending");
            sortingIcons[2] = new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/SortNumericalAsc.png") as Texture2D, "Sort Numerically Ascending");
            sortingIcons[3] = new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/SortNumericalDesc.png") as Texture2D, "Sort Numerically Descending");

            // Check if icons are loaded correctly
            for (int i = 0; i < sortingIcons.Length; i++)
            {
                if (sortingIcons[i].image == null)
                {
                    Debug.LogError("Sorting icon " + i + " not loaded correctly! Please check the file paths.");
                }
            }
        }

        private void OnDisable()
        {
            SaveState(); // Save the current state
            SaveSelectedAudioSources(); // Save the toggled audio sources
            EditorPrefs.SetBool("AudioAlchemy_IsFavoritesActive", audioClips == favorites);

            if (isClosing)
            {
                return; // Exit if already closing to prevent duplicate logic execution
            }

            isClosing = true; // Mark the window as in the process of closing

            if (sfxAudioSource != null && closeSFX != null)
            {
                sfxAudioSource.clip = closeSFX;
                sfxAudioSource.Play();

                // Delay cleanup until the SFX finishes playing
                StartDelaySequence(2f); // Wait for 2 seconds before cleanup
            }
            else
            {
                FallbackCleanup(); // Perform immediate cleanup if there's no closing sound
            }
        }

        private void FallbackCleanup()
        {
            // Stop all playback and save the audio source state before cleaning up
            StopAllPlayback();
            SaveAudioSourceState();

            // Unsubscribe from EditorApplication events to avoid memory leaks
            UnsubscribeFromEditorEvents();

            // Destroy the AudioSource if it exists
            DestroyAudioSource();

            // Save the buffer size setting
            EditorPrefs.SetInt("BufferSize", bufferSize);

            // Perform any additional cleanup
            CleanupAllAudioSources();

        }

        private void UnsubscribeFromEditorEvents()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update -= CheckAndPlayNextSequentially;
            EditorApplication.update -= CheckAndPlayNextRandomly;
            EditorApplication.update -= LoopWithDelay;
        }

        private void DestroyAudioSource()
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                DestroyImmediate(sfxAudioSource.gameObject);
                sfxAudioSource = null; // Clear the reference to avoid any potential null checks elsewhere
            }
        }

        private void ContinueInitialization()
        {
            try
            {
                AutoDetectAudioSources(); // Automatically detect audio sources in the scene

                if (!EditorApplication.isCompiling)
                {
                    LoadOrCreateAudioClipListData();
                }


                GatherAudioClips(Application.dataPath); // Gather audio clips from the specified path

                LoadFavorites(); // Load favorites from the ScriptableObject

                LoadPlayOnAwakeState(); // Load the Play On Awake state from EditorPrefs

                LoadAndApplyBufferSize(); // Load and apply the DSP buffer size

                minSize = new Vector2(200, 700); // Set the minimum size of the window

                EditorApplication.update += OnEditorUpdate; // Subscribe to the Editor update event
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during initialization: {ex.Message}");
            }
        }


        private void LoadOrCreateAudioClipListData()
        {
            try
            {
                // Define the asset path
                string assetPath = "Assets/Plugins/SFX/AudioAlchemyLab/MyAudioClipList.asset";

                // Load the existing asset if it exists
                audioClipListData = AssetDatabase.LoadAssetAtPath<AudioClipListData>(assetPath);

                // If it doesn't exist, create it
                if (audioClipListData == null)
                {
                    //Debug.Log("Creating new asset at " + assetPath);
                    audioClipListData = ScriptableObject.CreateInstance<AudioClipListData>();
                    AssetDatabase.CreateAsset(audioClipListData, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    //Debug.Log("Loaded existing asset from " + assetPath);
                }

                // Ensure the asset is marked as dirty if it's modified
                EditorUtility.SetDirty(audioClipListData);
                AssetDatabase.SaveAssets();

                // Debug info to check the state of the asset
                //Debug.Log("Favorite Clips Count: " + audioClipListData.FavoriteClips.Count);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading or creating AudioClipListData: {ex.Message}");
            }
        }

        private void LoadFavorites()
        {
            // Load favorites from the ScriptableObject
            favorites = new List<AudioClip>(audioClipListData.FavoriteClips);
        }

        private void LoadPlayOnAwakeState()
        {
            // Load the Play On Awake state from EditorPrefs
            playOnAwakeEnabled = EditorPrefs.GetBool("PlayOnAwakeState", false);
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.playOnAwake = playOnAwakeEnabled;
            }
        }

        private void LoadAndApplyBufferSize()
        {
            // Load and apply the DSP buffer size
            bufferSize = EditorPrefs.GetInt("BufferSize", 512);
            UpdateBufferSize(bufferSize);
        }

        #region Audio Source Management and Cleanup

        // Method to update the DSP buffer size of the audio system
        private void UpdateBufferSize(int newBufferSize)
        {
            var audioConfig = AudioSettings.GetConfiguration();
            audioConfig.dspBufferSize = newBufferSize;
            AudioSettings.Reset(audioConfig);
        }

        // Method to unload unused audio clips and free up memory
        private void UnloadUnusedAudioClips()
        {
            Resources.UnloadUnusedAssets(); // Unload unused assets, including audio clips
            System.GC.Collect(); // Trigger garbage collection to free up memory
        }

        // Method to clean up only the preview audio source used for SFX playback
        private void CleanupAllAudioSources()
        {
            //Debug.Log("Running CleanupAllAudioSources...");

            // Stop and destroy the preview AudioSource used by the tool
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                Debug.Log("Stopping and destroying sfxAudioSource.");
                sfxAudioSource.Stop();
                sfxAudioSource.enabled = false;
                DestroyImmediate(sfxAudioSource.gameObject); // Destroy the preview AudioSource's GameObject
                sfxAudioSource = null; // Clear the reference
            }

            // Unload unused assets and force garbage collection
            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            //Debug.Log("CleanupAllAudioSources completed.");
        }

        // Method to delay the destruction of the preview AudioSource, allowing the closeSFX to finish
        private void DelayedCleanupAndDestroyAudioSource(float waitTime)
        {
            EditorApplication.delayCall += () =>
            {
                CleanupAllAudioSources(); // Only cleanup the preview AudioSource

                // Perform additional cleanup or save operations if needed
                EditorPrefs.SetInt("BufferSize", bufferSize);

                isClosing = false; // Reset the closing flag after the sequence is done
            };
        }

        // Method to save the state of the AudioSource and associated settings
        private void SaveAudioSourceState()
        {
            audioClipListData.FavoriteClips.Clear(); // Clear the current list
            audioClipListData.FavoriteClips.AddRange(favorites); // Save the favorite clips
            EditorUtility.SetDirty(audioClipListData); // Mark the ScriptableObject as dirty to save changes
            AssetDatabase.SaveAssets(); // Save the changes to the asset database

            // Delay the subsequent cleanup and closing operations
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    if (sfxAudioSource != null)
                    {
                        sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        sfxAudioSource.Stop();
                        sfxAudioSource.enabled = false;
                        DestroyImmediate(sfxAudioSource.gameObject); // Destroy the AudioSource game object
                    }

                    EditorPrefs.SetBool("PlayOnAwakeState", playOnAwakeEnabled); // Save the playOnAwake setting

                    // Delay the closing of the window to allow cleanup to complete
                    EditorApplication.delayCall += () =>
                    {
                        isClosing = false;
                        Close(); // Close the EditorWindow
                    };
                };
            };
        }


        #endregion


        #region Audio Source Initialization and Management

        // Method to initialize or restore the main AudioSource used for SFX playback
        private void InitializeOrRestoreAudioSource()
        {
            if (sfxAudioSource == null)
            {
                //Debug.Log("sfxAudioSource is null, creating new one.");

                // Create a new GameObject to hold the AudioSource if it doesn't exist
                GameObject sfxAudioSourceObject = new GameObject("AALSFXAudioSource");
                sfxAudioSource = sfxAudioSourceObject.AddComponent<AudioSource>();

                // Hide the GameObject in the hierarchy
                sfxAudioSourceObject.hideFlags = HideFlags.HideInHierarchy;

                // Get the current audio configuration and apply the buffer size setting
                var audioConfig = AudioSettings.GetConfiguration();
                audioConfig.dspBufferSize = bufferSize;
                AudioSettings.Reset(audioConfig);

                sfxAudioSource.playOnAwake = false;  // Ensure it doesn't play automatically
                sfxAudioSource.enabled = true;  // Ensure the AudioSource is enabled

                //Debug.Log("sfxAudioSource initialized and hidden in the hierarchy.");
            }
            else if (!sfxAudioSource.isActiveAndEnabled)
            {
                Debug.Log("sfxAudioSource exists but is disabled, enabling it.");
                sfxAudioSource.enabled = true;  // Reactivate if previously disabled

                // Ensure the GameObject is still hidden
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        // Method to automatically detect all AudioSources in the scene, excluding the main SFX AudioSource
        private void AutoDetectAudioSources()
        {
            try
            {
                audioSourcesInScene.Clear();

                // Find all AudioSource components in the scene
                var allAudioSources = FindObjectsOfType<AudioSource>();

                // Filter out the AALSFXAudioSource
                foreach (var source in allAudioSources)
                {
                    if (source != sfxAudioSource && source.name != "AALSFXAudioSource")
                    {
                        audioSourcesInScene.Add(source);
                    }
                }

                // Initialize the toggles for each detected AudioSource
                audioSourceToggles = new List<bool>(new bool[audioSourcesInScene.Count]);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error detecting audio sources: {ex.Message}");
            }
        }


        private void ApplyClipAndSettingsToSelectedSources()
        {
            if (audioSourcesInScene == null || audioSourcesInScene.Count == 0)
            {
                Debug.LogWarning("No audio sources found in the scene.");
                return;
            }

            foreach (var source in audioSourcesInScene)
            {
                int index = audioSourcesInScene.IndexOf(source);
                if (audioSourceToggles[index])
                {
                    source.clip = sfxAudioSource.clip;  // Assign the current audio clip
                    source.volume = volume;             // Apply volume
                    source.pitch = pitch;               // Apply pitch
                    source.loop = isLooping;            // Apply loop setting
                    source.playOnAwake = playOnAwakeEnabled;  // Apply play-on-awake setting
                }
            }
        }

        // Method to select or deselect all audio sources
        private void SelectAllAudioSources(bool selectAll)
        {
            for (int i = 0; i < audioSourceToggles.Count; i++)
            {
                audioSourceToggles[i] = selectAll;
            }
        }

        // Method to clear all selections of audio sources
        private void ClearAllSelections()
        {
            SelectAllAudioSources(false);
        }

        // Method to play the opening sound effect
        private void PlayOpeningSound()
        {
            if (openSFX != null && sfxAudioSource != null)
            {
                sfxAudioSource.clip = openSFX;
                sfxAudioSource.Play();
            }
        }

        #endregion

        #region UI Utility Methods

        // This function creates a 2x2 texture of a given color
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        #endregion

        #region Coroutine-like Delayed Actions

        // Method to start a delay sequence, simulating a coroutine
        private void StartDelaySequence(float waitTime)
        {
            double startTime = EditorApplication.timeSinceStartup;

            // Use EditorApplication.update to simulate a coroutine
            EditorApplication.update += DelayedActions;

            void DelayedActions()
            {
                if (EditorApplication.timeSinceStartup - startTime >= waitTime)
                {
                    EditorApplication.update -= DelayedActions;

                    // Call FallbackCleanup to perform all necessary cleanup tasks, including saving buffer size
                    FallbackCleanup();

                    // Reset the closing flag
                    isClosing = false;
                }
            }
        }

        // Method to continuously update the EditorWindow
        private void OnEditorUpdate()
        {
            Repaint();
        }

        #endregion


        #region Update and Animation Methods

        // Update method that handles animations if the isAnimating flag is set
        private void Update()
        {
            // Handle animation progress and repaint the window
            if (isAnimating)
            {
                animationTimer += Time.deltaTime;

                if (animationTimer >= animationDuration)
                {
                    isAnimating = false;
                    animationTimer = 0f;
                }

                Repaint(); // Repaint to reflect the animation
            }
        }

        #endregion

        #region Utility Methods

        // Method to resize an icon texture to a specified width and height
        private Texture2D ResizeIcon(Texture2D originalIcon, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(originalIcon, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D resizedIcon = new Texture2D(width, height);
            resizedIcon.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            resizedIcon.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return resizedIcon;
        }

        #endregion

        #region GUI Rendering Methods

        // Main OnGUI method that handles rendering the editor window UI
        private void OnGUI()
        {
            DefineStyles(); // Define styles for various UI elements
            keyboardHandler.HandleKeyboardShortcuts(); // Handle keyboard shortcuts here
            DrawHeader(); // Draw the header section

            if (!showLogo)
            {
                DrawSettingsButton(); // Draw settings button if the logo is hidden
            }
            else
            {
                DrawLogoAndControls(); // Draw the logo and associated controls if the logo is shown
            }

            DrawSpectralWaveform(); // Draw the spectral waveform visualization
            DrawPlaybackControls(); // Draw playback control buttons
            DrawPlaybackModeControls(); // Draw playback mode controls (sequential, random, etc.)

            // Sort audio clips if the flag is set
            if (needsSorting)
            {
                SortAudioClips();
            }
            DrawAudioClipList(); // Draw the list of audio clips
        }

        // Method to define custom styles for the UI elements
        private void DefineStyles()
        {
            // Define the style for the "Settings" button
            basePurpleColor = new Color(0.85f, 0.65f, 0.95f);  // Normal state color
            hoverPurpleColor = new Color(0.95f, 0.85f, 0.98f); // Hover state color
            clickPurpleColor = new Color(0.7f, 0.5f, 0.9f);    // Active (click) state color

            settingsButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = GUI.contentColor },
                hover = { textColor = hoverPurpleColor }, // Very light purple, almost white
                active = { textColor = clickPurpleColor }, // Use a lighter shade for the selected color
                alignment = TextAnchor.MiddleCenter // Center the text
            };

            xButtonStyle = CreateXButtonStyle(); // Define style for the 'X' button used for clearing search, etc.
        }

        #endregion


        #region GUI Drawing Methods

        // Draws the header section of the editor window
        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();

            // Style for the selected clip name
            GUIStyle selectedClipNameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
            };
            GUILayout.Label(selectedClipName, selectedClipNameStyle); // Display the selected clip name

            GUILayout.FlexibleSpace(); // Add flexible space to separate elements

            // Style for the version number display
            GUIStyle versionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 12,
            };
            GUILayout.Label(versionNumber, versionStyle); // Display the version number

            GUILayout.EndHorizontal();
        }

        // Draws the "Arcane Detection" button with animation
        private void DrawArcaneDetectionButton()
        {
            float scale = 1f;
            float rotation = 0f;

            // Handle animation if isAnimating is true
            if (isAnimating)
            {
                float progress = Mathf.Clamp01(animationTimer / animationDuration);

                // Animation for scaling and rotation
                scale = Mathf.Lerp(1f, 0.85f, Mathf.SmoothStep(0f, 1f, progress));
                rotation = Mathf.Sin(progress * Mathf.PI * 3f) * 5f;

                animationTimer += Time.deltaTime * 0.39f; // Speed multiplier for the animation
                if (progress >= 1f)
                {
                    isAnimating = false;
                    animationTimer = 0f;
                }

                Repaint(); // Redraw the UI to reflect animation changes
            }

            // Save the current GUI matrix
            Matrix4x4 originalMatrix = GUI.matrix;

            // Calculate button rectangle
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Arcane Detection"), GUI.skin.button, GUILayout.Height(23));

            // Apply matrix transformation for the animation
            GUI.matrix = Matrix4x4.TRS(new Vector3(buttonRect.x + buttonRect.width / 2, buttonRect.y + buttonRect.height / 2, 0), Quaternion.Euler(0f, 0f, rotation), new Vector3(scale, scale, 1f));
            GUI.matrix *= Matrix4x4.TRS(-new Vector3(buttonRect.x + buttonRect.width / 2, buttonRect.y + buttonRect.height / 2, 0), Quaternion.identity, Vector3.one);

            // Style for the Arcane Detection button
            GUIStyle arcaneButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = GUI.contentColor },
                hover = { textColor = hoverPurpleColor }, // Hover state color
                active = { textColor = clickPurpleColor }, // Active state color
                alignment = TextAnchor.MiddleCenter // Center the text
            };

            // Tooltip content for the button
            GUIContent arcaneButtonContent = new GUIContent("Arcane Detection", "Cast a spell to detect all AudioSources in the scene.");

            // Draw the button with animation
            GUI.backgroundColor = basePurpleColor;
            if (GUI.Button(buttonRect, arcaneButtonContent, arcaneButtonStyle))
            {
                isAnimating = true;
                animationTimer = 0f; // Reset animation timer
                PlaySFX(searchSpellSFX); // Play sound effect
                AutoDetectAudioSources(); // Trigger audio source detection
            }

            GUI.backgroundColor = Color.white; // Reset background color after drawing

            // Restore the original GUI matrix
            GUI.matrix = originalMatrix;
        }

        // Draws the Settings button
        private void DrawSettingsButton()
        {
            // Calculate the button rectangle
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Settings"), settingsButtonStyle, GUILayout.Height(25));

            // Set the background color depending on the button state
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                GUI.backgroundColor = Event.current.type == EventType.MouseDown ? clickPurpleColor : hoverPurpleColor;
            }
            else
            {
                GUI.backgroundColor = basePurpleColor;
            }

            // Draw the Settings button
            if (GUI.Button(buttonRect, new GUIContent("Settings", "Click to show settings"), settingsButtonStyle))
            {
                showLogo = true; // Show the logo when settings button is clicked
                PlaySFX(settingsSFX);  // Play settings sound effect
                Repaint(); // Redraw the UI to reflect changes
            }

            // Reset the GUI background color after the button
            GUI.backgroundColor = Color.white;
        }

        #endregion


        #region GUI Drawing Methods

        // Draws the logo and associated controls in the editor window
        private void DrawLogoAndControls()
        {
            Texture2D logoTexture = EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/AAL.png") as Texture2D;
            if (logoTexture != null)
            {
                // Calculate logo dimensions
                float logoWidth = Mathf.Min(position.width * 0.9f, 300f);
                float logoHeight = logoWidth / 3f;

                // Center the logo horizontally
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Rect logoRect = GUILayoutUtility.GetRect(logoWidth, logoHeight, GUILayout.ExpandWidth(true));

                // Clickable logo to hide/show settings
                if (GUI.Button(logoRect, new GUIContent("", "Click to hide/show settings"), GUIStyle.none))
                {
                    showLogo = false; // Toggle visibility of the logo
                    PlaySFX(settingsSFX); // Play the settings sound effect
                    Repaint(); // Refresh the UI
                }

                // Draw the logo texture
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // Draw additional control buttons and sections
                DrawControlButtons();
                DrawArcaneDetectionButton(); // Add Arcane Detection button below the logo
                DrawAudioSourceControls(); // Display audio source controls below the detection button
                DrawVolumePitchLoopBuffersPlayOnAwake(); // Draw controls for volume, pitch, loop, buffer size, etc.
            }
        }

        // Draws the main control buttons below the logo
        private void DrawControlButtons()
        {
            GUILayout.BeginHorizontal();

            // Draw buttons for folder selection, refresh, and search bar
            DrawSelectFolderButton();
            DrawRefreshButton();
            DrawSearchBarAndClearButton();

            GUILayout.EndHorizontal();

            // Filter the audio clips based on the search string
            filteredAudioClips = audioClips
                .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // Draws the folder selection button
        private void DrawSelectFolderButton()
        {
            GUIContent folderButtonIconContent = new GUIContent(EditorGUIUtility.IconContent("FolderOpened Icon").image, "Select a folder to load audio clips");
            if (GUILayout.Button(folderButtonIconContent, GUILayout.Width(24), GUILayout.Height(24)))
            {
                PlaySFX(folderClickSFX); // Play folder click sound effect
                string folderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, ""); // Open folder panel
                PlaySFX(folderClickSFX);

                if (!string.IsNullOrEmpty(folderPath))
                {
                    GatherAudioClips(folderPath); // Load audio clips from the selected folder
                    Repaint(); // Refresh the UI
                }
            }
        }

        // Draws the refresh button
        private void DrawRefreshButton()
        {
            GUIContent refreshIconContent = new GUIContent(EditorGUIUtility.IconContent("Refresh").image, "Refresh audio clips");
            if (GUILayout.Button(refreshIconContent, GUILayout.Width(24), GUILayout.Height(24)))
            {
                RefreshAudioClips(); // Refresh the list of audio clips
                PlaySFX(toggleSFX); // Play toggle sound effect
                Repaint(); // Refresh the UI
            }
        }

        // Draws the search bar and clear button
        private void DrawSearchBarAndClearButton()
        {
            string previousSearchString = searchString;
            searchString = GUILayout.TextField(searchString, GUILayout.Height(22), GUILayout.ExpandWidth(true)); // Search bar input

            // Refilter and sort clips if the search string changes
            if (previousSearchString != searchString)
            {
                filteredAudioClips = audioClips
                    .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                SortAudioClips(); // Sort the filtered clips
            }

            // Draw the clear search button
            if (GUILayout.Button(new GUIContent("X", "Clear search"), xButtonStyle, GUILayout.Width(22), GUILayout.Height(22)))
            {
                searchString = ""; // Clear the search string
                GUI.FocusControl(null); // Remove focus from the search bar
                PlaySFX(trashSFX); // Play trash sound effect

                // Re-apply sorting and refresh the list
                filteredAudioClips = audioClips.ToList();
                SortAudioClips(); // Sort the clips after clearing the search
            }
        }

        #endregion

        #region GUI Drawing Methods

        // Draws the controls for managing audio sources in the scene
        private void DrawAudioSourceControls()
        {
            bool previousState = showAudioSources; // Store the previous foldout state

            // Draw foldout for audio sources
            showAudioSources = EditorGUILayout.Foldout(showAudioSources, "Audio Sources", true);

            // Play sound effect if the foldout state changes
            if (previousState != showAudioSources)
            {
                PlaySFX(settingsSFX);
            }

            // If foldout is expanded, display the audio sources list
            if (showAudioSources)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5); // Padding

                // Draw toggle to select/deselect all audio sources
                bool allSelected = audioSourceToggles.All(t => t);
                bool newAllSelected = EditorGUILayout.Toggle(allSelected, GUILayout.Width(20));
                GUILayout.Label("All", GUILayout.Width(20));

                if (newAllSelected != allSelected)
                {
                    SelectAllAudioSources(newAllSelected);
                    PlaySFX(toggleSFX);
                    Repaint();
                }

                // Clear all button
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/clearAll.png") as Texture2D, "Clear all selections"), xButtonStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    ClearAllSelections();
                    PlaySFX(trashSFX);
                    Repaint();
                }

                GUIStyle plusButtonStyle = CreatePlusButtonStyle();

                // Create the button with the "+" symbol using the style
                if (GUILayout.Button(new GUIContent("+", "Create a new Audio Source"), plusButtonStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    CreateNewAudioSource();
                    PlaySFX(cancelSFX); // Play a sound effect when the button is clicked
                    Repaint(); // Refresh the UI
                }

                // Delete button
                if (GUILayout.Button(new GUIContent("X", "Delete selected audio sources"), xButtonStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    PlaySFX(trashSFX);
                    DeleteSelectedAudioSources(); // Call the deletion method
                    Repaint();
                }

                GUILayout.EndHorizontal();

                // Define maximum scroll height and create scroll view for audio sources
                float maxScrollHeight = Mathf.Min(180f, audioSourcesInScene.Count * 50f) / 2f;
                audioSourceScrollPosition = GUILayout.BeginScrollView(audioSourceScrollPosition, GUILayout.Height(maxScrollHeight));

                // Iterate through the audio sources and draw toggle and object field for each
                for (int i = 0; i < audioSourcesInScene.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5); // Padding

                    bool oldValue = audioSourceToggles[i];
                    audioSourceToggles[i] = EditorGUILayout.Toggle(audioSourceToggles[i], GUILayout.Width(20));

                    // Play sound effect if toggle value changes
                    if (oldValue != audioSourceToggles[i])
                    {
                        PlaySFX(toggleSFX);
                    }

                    // Draw object field for the audio source
                    audioSourcesInScene[i] = (AudioSource)EditorGUILayout.ObjectField(audioSourcesInScene[i], typeof(AudioSource), true);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }

        // Draws the controls for adjusting volume, pitch, looping, buffer size, and play on awake settings
        private void DrawVolumePitchLoopBuffersPlayOnAwake()
        {
            GUILayout.Space(3); // Add space above the Volume control

            // Volume control slider
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Volume", GUILayout.Width(50));
            float newVolume = EditorGUILayout.Slider(volume, 0.0f, 1.0f);
            if (newVolume != volume)
            {
                volume = newVolume;
                UpdateVolume();
            }
            EditorGUILayout.EndHorizontal();

            // Pitch control slider
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pitch", GUILayout.Width(50));
            float newPitch = EditorGUILayout.Slider(pitch, 0.1f, 3.0f);
            if (newPitch != pitch)
            {
                pitch = newPitch;
                UpdatePitch();
            }
            EditorGUILayout.EndHorizontal();

            // Buffer size control slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Buffer Size", GUILayout.Width(65));
            int newBufferSize = EditorGUILayout.IntSlider(bufferSize, minBufferSize, maxBufferSize);
            if (newBufferSize != bufferSize)
            {
                bufferSize = newBufferSize;
                UpdateBufferSize(bufferSize);
            }
            GUILayout.EndHorizontal();

            // Looping control
            GUILayout.BeginHorizontal();
            GUILayout.Label("Loop", GUILayout.Width(50));
            bool newLooping = EditorGUILayout.Toggle(isLooping, GUILayout.Width(15));
            if (newLooping != isLooping)
            {
                ToggleLooping();
                PlaySFX(toggleSFX);
            }

            // Loop delay slider
            if (isLooping)
            {
                float newLoopDelay = EditorGUILayout.Slider(loopDelay, 0f, 2f, GUILayout.ExpandWidth(true));
                if (newLoopDelay != loopDelay)
                {
                    loopDelay = newLoopDelay;
                    UpdateLoopingState();
                }
            }
            else
            {
                GUILayout.Label("", GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            // Apply loop state to selected audio sources
            if (audioSourcesInScene.Any())
            {
                foreach (var source in audioSourcesInScene)
                {
                    if (source != null && audioSourceToggles[audioSourcesInScene.IndexOf(source)])
                    {
                        source.loop = isLooping;
                    }
                }
            }

            // Play on Awake control
            GUILayout.BeginHorizontal();
            GUILayout.Label("Play On Awake", GUILayout.Width(100));
            bool newPlayOnAwakeState = EditorGUILayout.Toggle(playOnAwakeEnabled, GUILayout.Width(15));

            if (newPlayOnAwakeState != playOnAwakeEnabled)
            {
                TogglePlayOnAwake();
                PlaySFX(toggleSFX);
            }
            GUILayout.EndHorizontal();

            // Apply Play on Awake state to selected audio sources
            if (audioSourcesInScene.Any())
            {
                foreach (var source in audioSourcesInScene)
                {
                    if (source != null)
                    {
                        source.playOnAwake = playOnAwakeEnabled;
                    }
                }
            }
        }

        // Draws the spectral waveform in the UI
        private void DrawSpectralWaveform()
        {
            // Define the rectangle for the waveform and draw it
            Rect spectralRect = GUILayoutUtility.GetRect(position.width - 20, 50);
            DrawSpectralWaveform(spectralRect);
        }

        #endregion


        #region Playback Control Methods

        // Draws the playback controls (Play, Pause, Stop, Skip Back, Skip Forward)
        private void DrawPlaybackControls()
        {
            float topRowButtonWidth = Mathf.FloorToInt((position.width - 16) / 5f); // Calculate button width

            GUILayout.BeginHorizontal();

            // Play button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_PlayButton").image, "Play audio"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAllPlayback(); // Stop any ongoing playback before starting a new one
                PlayAudio();
            }

            // Pause button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("PauseButton").image, "Pause audio"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                PauseAudio();
            }

            // Stop button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_PreMatQuad").image, "Stop audio"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAudio();
            }

            // Skip Back button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Animation.PrevKey").image, "Skip back"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAllPlayback();
                SkipBackAudio(); // Skip to the previous audio clip
            }

            // Skip Forward button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Animation.NextKey").image, "Skip forward"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAllPlayback();
                SkipForwardAudio(); // Skip to the next audio clip
            }

            GUILayout.EndHorizontal();
        }

        // Draws the playback mode controls (Sequential, Shuffle, Sort, Favorites)
        private void DrawPlaybackModeControls()
        {
            float topRowButtonWidth = Mathf.FloorToInt((position.width - 16) / 5f); // Calculate button width

            GUILayout.BeginHorizontal();

            // Sequential playback button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/Sequential.png") as Texture2D, "Toggle sequential playback"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAllPlayback();
                ToggleSequential(); // Toggle sequential playback mode
            }

            // Shuffle playback button
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.Load("Assets/Plugins/SFX/AudioAlchemyLab/Icons/Shuffle.png") as Texture2D, "Toggle shuffle playback"), GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                StopAllPlayback();
                ToggleShuffle(); // Toggle shuffle playback mode
            }

            // Sort button (cycles through sorting options)
            if (GUILayout.Button(sortingIcons[selectedSortingOption], GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                CycleSortingOption(); // Cycle through sorting options
                TriggerSort(); // Apply the sorting
                PlaySFX(sortSFX); // Play sort sound effect
            }

            // Favorites tab button
            Texture2D favoritesIcon = EditorGUIUtility.Load(audioClips == favorites ? "Assets/Plugins/SFX/AudioAlchemyLab/Icons/RedHeart.png" : "Assets/Plugins/SFX/AudioAlchemyLab/Icons/Heart.png") as Texture2D;
            GUIContent favoritesContent = new GUIContent(favoritesIcon, "Toggle between the Favorites View on or off");

            if (GUILayout.Button(favoritesContent, GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                ToggleFavorites();
                Repaint();
            }

            // Clear Favorites button (only shown when Favorites view is active)
            if (audioClips == favorites && GUILayout.Button(new GUIContent("X", "Clear all favorites"), xButtonStyle, GUILayout.Height(30f), GUILayout.Width(topRowButtonWidth)))
            {
                favorites.Clear();
                filteredAudioClips.Clear();
                PlaySFX(trashSFX); // Play trash sound effect
                Repaint();
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Audio Clip List

        // Draws the list of audio clips with options to play, add to favorites, and locate in project
        private void DrawAudioClipList()
        {
            // Ensure we are using the correct, sorted list
            if (hasSorted)
            {
                hasSorted = false; // Reset the flag after updating the UI.
            }

            float totalAvailableWidth = position.width - 30f; // Available width for buttons
            float heartButtonWidth = 32f; // Width for the favorite button
            float folderButtonWidth = 32f; // Width for the folder button
            float deleteButtonWidth = 32f; // Width for the delete button
            float playButtonWidth = totalAvailableWidth - heartButtonWidth - folderButtonWidth - deleteButtonWidth - 2f; // Adjusted width for the play button

            // Scroll view for the audio clip list
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));

            GUILayout.BeginVertical(); // Wrap the scroll content in a vertical layout group

            for (int i = 0; i < filteredAudioClips.Count; i++)
            {
                var clip = filteredAudioClips[i];

                // Skip if the clip is null (destroyed)
                if (clip == null) continue;

                EditorGUILayout.BeginHorizontal();

                // Check if the clip is currently selected
                bool isSelected = (audioClips.IndexOf(clip) == currentClipIndex);

                // Define button style based on selection state
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedHeight = 32f,
                    normal = { textColor = isSelected ? selectedTextColor : defaultTextColor },
                    hover = { textColor = hoverPurpleColor },
                    fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal
                };

                string buttonText = " \u266B " + (clip != null ? clip.name : "No Clip Selected");

                // Favorite button (toggles favorite state)
                Texture2D favoriteIcon = EditorGUIUtility.Load(favorites.Contains(clip) ? "Assets/Plugins/SFX/AudioAlchemyLab/Icons/RedHeart.png" : "Assets/Plugins/SFX/AudioAlchemyLab/Icons/Heart.png") as Texture2D;
                GUIContent favoriteContent = new GUIContent(favoriteIcon, "Add/Remove from favorites");

                if (GUILayout.Button(favoriteContent, GUILayout.Width(heartButtonWidth), GUILayout.Height(32f)))
                {
                    ToggleFavorite(clip);
                    Repaint();
                }

                // Play button
                if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(32f), GUILayout.Width(playButtonWidth)))
                {
                    SelectAudioClip(i);
                    PlayAudio();
                    Repaint();
                }

                // Folder button (locates the clip in the project)
                Texture2D folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
                GUIContent folderContent = new GUIContent(folderIcon, "Ping audio clip in project");

                if (GUILayout.Button(folderContent, GUI.skin.button, GUILayout.Width(folderButtonWidth), GUILayout.Height(32f)))
                {
                    Selection.activeObject = clip;
                    EditorGUIUtility.PingObject(clip);
                    PlaySFX(folderClickSFX); // Play folder click sound effect
                    Repaint();
                }

                // Delete button (deletes the clip from the project)
                Texture2D deleteIcon = EditorGUIUtility.IconContent("TreeEditor.Trash").image as Texture2D;
                GUIContent deleteContent = new GUIContent(deleteIcon, "Delete this audio clip from the project");

                if (GUILayout.Button(deleteContent, GUI.skin.button, GUILayout.Width(deleteButtonWidth), GUILayout.Height(32f)))
                {
                    PlaySFX(trashSFX);
                    DeleteAudioClip(clip);
                    Repaint();

                    // Make sure the layout is properly ended before breaking
                    EditorGUILayout.EndHorizontal();  // End the current horizontal layout
                    break;  // Exit the loop to avoid accessing the destroyed clip
                }

                // If we didn't break the loop, end the horizontal layout here
                EditorGUILayout.EndHorizontal();

                // Handle drag and drop functionality for the selected clip
                if (isSelected)
                {
                    EditorGUILayout.BeginHorizontal();

                    Rect dragAreaRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(totalAvailableWidth + 8f), GUILayout.Height(30f));
                    dragAreaRect.x += 2f;

                    GUIStyle centeredTextStyle = new GUIStyle(EditorStyles.objectField)
                    {
                        alignment = TextAnchor.MiddleCenter // Center the text
                    };

                    GUI.Box(dragAreaRect, new GUIContent(clip.name, "Drag this clip to other inspectors"), centeredTextStyle);

                    if (Event.current.type == EventType.MouseDrag && dragAreaRect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new UnityEngine.Object[] { clip };
                        DragAndDrop.StartDrag("Drag Audio Clip");
                        Event.current.Use();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical(); // End the vertical layout group inside the scroll view
            GUILayout.EndScrollView(); // End the scroll view
        }

        #endregion

        private void DeleteAudioClip(AudioClip clip)
        {
            string assetPath = AssetDatabase.GetAssetPath(clip);

            if (EditorUtility.DisplayDialog("Confirm Deletion", $"Are you sure you want to delete {clip.name} from the project?", "Delete", "Cancel"))
            {
                // Play the trash SFX
                PlaySFX(trashSFX);

                // Delete the asset from the project
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();

                // Remove the clip from the lists
                audioClips.Remove(clip);
                filteredAudioClips.Remove(clip);

                //Debug.Log($"Deleted audio clip: {clip.name}");
            }
            else
            {
                PlaySFX(settingsSFX);
            }
        }

        private void CycleAudioSourceSortingOption()
        {
            // Cycle through the sorting options (4 states: Alphabetical Asc, Desc, Numerical Asc, Desc)
            selectedSortingOption = (selectedSortingOption + 1) % 4;

            // Trigger sorting for audio sources
            SortAudioSources();
            PlaySFX(sortSFX);

            // Refresh the UI after sorting
            Repaint();
        }

        private void CreateNewAudioSource()
        {
            GameObject newAudioSourceObject = new GameObject("New Audio Source");
            AudioSource newAudioSource = newAudioSourceObject.AddComponent<AudioSource>();

            // Optionally, configure the AudioSource with default settings
            newAudioSource.playOnAwake = false;

            // Hide the GameObject if needed, similar to how sfxAudioSource is handled
            newAudioSourceObject.hideFlags = HideFlags.None; // Change this if you want to hide it

            // Add the new audio source to the list
            audioSourcesInScene.Add(newAudioSource);
            audioSourceToggles.Add(false);

            // Optionally, log or play a sound effect to confirm creation
            PlaySFX(settingsSFX);
            // Refresh the list of audio sources in the UI
            AutoDetectAudioSources();
            Repaint();
        }

        #region Sorting Methods

        // Cycles through the available sorting options and triggers sorting
        private void CycleSortingOption()
        {
            // Cycle through the sorting options (4 states: Alphabetical Asc, Desc, Numerical Asc, Desc)
            selectedSortingOption = (selectedSortingOption + 1) % 4;

            // Mark that sorting is needed and trigger the sort process
            needsSorting = true;
            TriggerSort();
            PlaySFX(sortSFX);

            // Refresh the UI after sorting
            Repaint();
        }

        private GUIStyle CreatePlusButtonStyle()
        {
            Color greenColor = new Color(0.2f, 1f, 0.2f); // Green color

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22, // Slightly larger font size for better visibility
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter, // Ensure text is centered
                padding = new RectOffset(0, 0, -5, 0), // Remove any padding to center the text perfectly
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }, // Light grey (normal state)
                hover = { textColor = greenColor }, // Green color on hover
                active = { textColor = greenColor }, // Green color when clicked
            };

            return style;
        }


        // Sorts the audio clips based on the selected sorting option
        private void SortAudioClips()
        {
            if (!needsSorting) return;

            switch (selectedSortingOption)
            {
                case 0: // Alphabetical Ascending
                    audioClips = audioClips.OrderBy(clip => clip.name).ToList();
                    Debug.Log("Alphabetical Ascending sorting performed.");
                    break;
                case 1: // Alphabetical Descending
                    audioClips = audioClips.OrderByDescending(clip => clip.name).ToList();
                    Debug.Log("Alphabetical Descending sorting performed.");
                    break;
                case 2: // Numerical Ascending
                    audioClips = audioClips
                        .OrderBy(clip => ExtractNumberFromName(clip.name))
                        .ThenBy(clip => clip.name) // In case of a tie in numbers, sort alphabetically
                        .ToList();
                    Debug.Log("Numerical Ascending sorting performed.");
                    break;
                case 3: // Numerical Descending
                    audioClips = audioClips
                        .OrderByDescending(clip => ExtractNumberFromName(clip.name))
                        .ThenByDescending(clip => clip.name) // In case of a tie in numbers, sort alphabetically
                        .ToList();
                    Debug.Log("Numerical Descending sorting performed.");
                    break;
                default:
                    Debug.LogWarning("Invalid sorting option selected.");
                    break;
            }

            // Apply the search filter after sorting
            filteredAudioClips = audioClips.Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            needsSorting = false;
            hasSorted = true;
            Repaint(); // Refresh the UI after sorting
        }

        // Extracts the first number found in the clip's name for numerical sorting
        private int ExtractNumberFromName(string name)
        {
            int number = 0;
            string digits = new string(name.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(digits))
            {
                int.TryParse(digits, out number);
            }
            return number;
        }

        // Triggers sorting of the audio clips based on the current sorting option
        private void TriggerSort()
        {
            if (!needsSorting) return;

            // Determine the target list to sort (audio clips or favorites)
            List<AudioClip> targetClips = audioClips == favorites ? favorites : audioClips;

            switch (selectedSortingOption)
            {
                case 0: // Alphabetical Ascending (formerly Numerical Ascending)
                    targetClips = targetClips.OrderBy(clip => ExtractNumberFromName(clip.name)).ToList();
                    break;
                case 1: // Alphabetical Descending
                    targetClips = targetClips.OrderByDescending(clip => clip.name).ToList();
                    break;
                case 2: // Numerical Ascending (formerly Alphabetical Ascending)
                    targetClips = targetClips.OrderBy(clip => clip.name).ToList();
                    break;
                case 3: // Numerical Descending
                    targetClips = targetClips.OrderByDescending(clip => ExtractNumberFromName(clip.name)).ToList();
                    break;
                default:
                    Debug.LogWarning("Invalid sorting option selected.");
                    break;
            }

            // Update the correct list (audio clips or favorites)
            if (audioClips == favorites)
            {
                favorites = targetClips;
                audioClips = favorites; // Ensure favorites are shown correctly after sorting
            }
            else
            {
                audioClips = targetClips;
            }

            // Apply the search filter after sorting
            filteredAudioClips = targetClips
                .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            needsSorting = false;
            hasSorted = true;

            // Update the UI after sorting
            Repaint();
        }

        #endregion

        #region Sequential Playback Methods

        // Checks if the next audio clip should be played sequentially and handles the logic
        private void CheckAndPlayNextSequentially()
        {
            if (sfxAudioSource != null && !sfxAudioSource.isPlaying && playSequentially)
            {
                PlayNextAudioSequentially();
            }
            else if (sfxAudioSource == null)
            {
                Debug.LogError("AudioSource is null or destroyed!");
                EditorApplication.update -= CheckAndPlayNextSequentially;
            }
            else if (playSequentially)
            {
                // Re-register the callback to keep checking until manually stopped
                EditorApplication.update -= CheckAndPlayNextSequentially;
                EditorApplication.update += CheckAndPlayNextSequentially;
            }
        }

        // Plays the next audio clip sequentially and wraps around if necessary
        private void PlayNextAudioSequentially()
        {
            if (audioClips.Count == 0) return;

            // Increment the current clip index, wrapping around if necessary
            currentClipIndex = (currentClipIndex + 1) % audioClips.Count;

            // Select and play the next audio clip
            SelectAudioClip(currentClipIndex);
            PlayAudio();

            // Adjust the scroll position to keep the selected clip in view
            AdjustScrollPositionForSkip(1);

            //Debug.Log($"Playing next sequential audio: currentClipIndex = {currentClipIndex}");
        }

        #endregion

        #region Clip Scrolling and Selection

        // Keeps the selected clip in view by adjusting the scroll position
        private void KeepClipInView(int clipIndex)
        {
            if (clipIndex < 0 || clipIndex >= filteredAudioClips.Count)
                return;

            float clipHeight = 32f; // Assuming each clip has a uniform height
            float scrollableAreaHeight = scrollViewHeight; // Height of the scrollable area
            float selectedClipTop = clipIndex * clipHeight; // Top position of the selected clip

            // Calculate the target scroll position to center the selected clip
            float targetScrollPosition = selectedClipTop - (scrollableAreaHeight - clipHeight) / 2;

            // Adjust the scroll position based on the selected clip's position relative to the view
            if (selectedClipTop < scrollPosition.y)
            {
                targetScrollPosition = selectedClipTop - clipHeight; // Adjust upwards
            }
            else if (selectedClipTop + clipHeight > scrollPosition.y + scrollableAreaHeight)
            {
                targetScrollPosition = selectedClipTop - scrollableAreaHeight + clipHeight; // Adjust downwards
            }

            // Apply an offset for fine-tuning
            float adjustmentOffset = 1.5f * clipHeight;
            targetScrollPosition -= adjustmentOffset;

            // Ensure the scroll position stays within valid bounds
            scrollPosition.y = Mathf.Clamp(targetScrollPosition, 0, Mathf.Max(0, (filteredAudioClips.Count * clipHeight) - scrollableAreaHeight));
        }

        // Handles the logic for skipping forward to the next audio clip
        public void SkipForwardAudio()
        {
            if (currentClipIndex < filteredAudioClips.Count - 1)
            {
                currentClipIndex++;
                SelectAudioClip(currentClipIndex);
                PlayAudio();
                AdjustScrollPositionForSkip(1); // Adjust the scroll position downwards
            }
        }

        // Handles the logic for skipping back to the previous audio clip
        public void SkipBackAudio()
        {
            if (currentClipIndex > 0)
            {
                currentClipIndex--;
                SelectAudioClip(currentClipIndex);
                PlayAudio();
                AdjustScrollPositionForSkip(-1); // Adjust the scroll position upwards
            }
        }

        // Adjusts the scroll position based on the current clip index
        private void AdjustScrollPosition()
        {
            float elementHeight = 32f;
            float selectedElementHeight = 64f;

            // Calculate the target offset for the scroll position
            float targetOffset = currentClipIndex * elementHeight;

            if (currentClipIndex == audioClips.IndexOf(filteredAudioClips[currentClipIndex]))
            {
                targetOffset = currentClipIndex * selectedElementHeight;
            }

            // Adjust the scroll position based on the selected clip's position relative to the view
            if (targetOffset > scrollPosition.y + (position.height - heightOfElementsAbove) - selectedElementHeight)
            {
                scrollPosition.y += elementHeight; // Scroll down
            }
            else if (targetOffset < scrollPosition.y)
            {
                scrollPosition.y -= elementHeight; // Scroll up
            }

            // Ensure the scroll position stays within valid bounds
            scrollPosition.y = Mathf.Clamp(scrollPosition.y, 0, Mathf.Max(0, (filteredAudioClips.Count * elementHeight) - position.height));
        }

        // Scrolls directly to the selected clip
        private void ScrollToClip(int clipIndex)
        {
            float elementHeight = 32f;
            float selectedElementHeight = 64f;
            scrollPosition.y = clipIndex * elementHeight;

            if (clipIndex == currentClipIndex)
            {
                scrollPosition.y = clipIndex * selectedElementHeight;
            }
        }

        // Plays a random audio clip and adjusts the scroll to keep it in view
        private void PlayNextAudioRandomly()
        {
            if (audioClips.Count == 0) return;

            int newClipIndex;
            do
            {
                newClipIndex = UnityEngine.Random.Range(0, audioClips.Count);
            } while (newClipIndex == currentClipIndex);

            currentClipIndex = newClipIndex;
            SelectAudioClip(currentClipIndex);
            PlayAudio();

            // Delay the scroll adjustment to ensure layout is calculated
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    KeepClipInView(currentClipIndex);
                };
            };
        }

        // Adjusts the scroll position based on the direction of the skip (forward or back)
        private void AdjustScrollPositionForSkip(int direction)
        {
            float elementHeight = 32f; // Assuming uniform height for each element
            scrollPosition.y += direction * elementHeight; // Adjust the scroll position

            // Ensure the scroll position stays within valid bounds
            scrollPosition.y = Mathf.Clamp(scrollPosition.y, 0, Mathf.Max(0, (filteredAudioClips.Count * elementHeight) - position.height));
        }

        // Selects an audio clip based on the provided index and updates the selected clip name
        private void SelectAudioClip(int index)
        {
            if (index >= 0 && index < filteredAudioClips.Count)
            {
                AudioClip selectedClip = filteredAudioClips[index];
                currentClipIndex = audioClips.IndexOf(selectedClip);
                selectedClipName = selectedClip.name;

                Repaint(); // Refresh the UI to reflect the selected clip
            }
        }

        #endregion

        #region Audio Source Initialization and Playback

        // Ensures that the AudioSource is initialized and active
        private void EnsureAudioSourceIsInitialized()
        {
            if (sfxAudioSource == null || !sfxAudioSource.isActiveAndEnabled)
            {
                Debug.LogWarning("AudioSource is null or destroyed. Attempting to reinitialize.");
                InitializeOrRestoreAudioSource();
            }
        }

        // Checks if the AudioSource should play the next clip randomly
        private void CheckAndPlayNextRandomly()
        {
            EnsureAudioSourceIsInitialized();

            if (sfxAudioSource == null)
            {
                Debug.LogError("Failed to reinitialize AudioSource. Aborting.");
                return;
            }

            if (!sfxAudioSource.isPlaying && playRandomly)
            {
                PlayNextAudioRandomly();
            }
            else if (playRandomly)
            {
                EditorApplication.update -= CheckAndPlayNextRandomly;
                EditorApplication.update += CheckAndPlayNextRandomly;
            }
        }

        #endregion

        #region UI and Display Adjustments

        // Calculates the total height occupied by UI elements above the scroll view
        private float GetOccupiedHeightByOtherUI()
        {
            float height = 0f;

            if (showLogo)
            {
                height += 100f; // Adjust based on the actual height of your logo or settings UI
            }

            if (showAudioSources)
            {
                height += 150f; // Adjust based on the actual height of your audio sources UI
            }

            // Add height of any other UI elements that take up space above the scroll view

            return height;
        }

        // Draws the spectral waveform for the currently playing audio
        private void DrawSpectralWaveform(Rect rect)
        {
            if (sfxAudioSource != null && sfxAudioSource.clip != null && sfxAudioSource.isPlaying)
            {
                sfxAudioSource.GetOutputData(samples, 0);
                Handles.BeginGUI();
                Handles.color = new Color(0.1f, 0.6f, 1f);

                float lineThickness = 2f;  // Increased thickness for better visibility
                float centerY = rect.y + rect.height / 2;

                for (int i = 0; i < samples.Length - 1; i++)
                {
                    float xPos = rect.x + i * rect.width / (samples.Length - 1);
                    float yPos = centerY + samples[i] * rect.height / 2;
                    float nextYPos = centerY + samples[i + 1] * rect.height / 2;

                    Handles.DrawAAPolyLine(lineThickness, new Vector3(xPos, yPos), new Vector3(xPos + rect.width / samples.Length, nextYPos));
                }

                Handles.EndGUI();
            }
        }

        #endregion

        #region Utility Methods

        // Plays a sound effect using the AudioSource
        private void PlaySFX(AudioClip clip)
        {
            if (sfxAudioSource != null && clip != null)
            {
                sfxAudioSource.PlayOneShot(clip, 0.25f);
            }
        }

        // Refreshes the list of audio clips and applies sorting and filtering
        private void RefreshAudioClips()
        {
            GatherAudioClips(Application.dataPath); // Gather audio clips from the specified path
            filteredAudioClips = audioClips
                .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList(); // Apply the search filter
            SortAudioClips(); // Sort the filtered list based on the current sorting option
            Repaint(); // Refresh the UI to display the sorted and filtered list of clips

            // Optional: Play a sound effect if needed
            // PlaySFX(folderClickSFX);
        }

        // Creates a custom style for the "X" button
        private GUIStyle CreateXButtonStyle()
        {
            Color redHeartColor = new Color(1f, 0.4f, 0.4f); // Corresponds to #FF6666
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }, // Light grey
                hover = { textColor = redHeartColor },
                active = { textColor = redHeartColor },
            };
            return style;
        }

        #endregion

        #region Playback Mode Toggles

        // Toggles sequential playback mode on or off
        private void ToggleSequential()
        {
            StopAudio();
            playSequentially = !playSequentially;
            playRandomly = false;

            if (playSequentially)
            {
                PlayAudio();
                EditorApplication.update += CheckAndPlayNextSequentially;
            }
        }

        // Toggles shuffle playback mode on or off
        private void ToggleShuffle()
        {
            StopAudio();
            playRandomly = !playRandomly;
            playSequentially = false;

            if (playRandomly)
            {
                PlayNextAudioRandomly();
                EditorApplication.update += CheckAndPlayNextRandomly;
            }
        }

        #endregion

        #region Favorites Management

        // Toggles between showing the full list of audio clips and only the user's favorites
        private void ToggleFavorites()
        {
            PlaySFX(heartSFX);

            if (audioClips == favorites)
            {
                // Restore the full list of audio clips
                audioClips = previousClips;
                GatherAudioClips(Application.dataPath); // Refresh the audioClips list

                // Reapply search filter and sorting to the restored list
                filteredAudioClips = audioClips
                    .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                SortAudioClips(); // Apply sorting based on the current option
            }
            else
            {
                // Store the current list and show only favorites
                previousClips = new List<AudioClip>(audioClips);
                audioClips = favorites;

                // Filter favorites based on the current search string
                filteredAudioClips = audioClips
                    .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                SortAudioClips(); // Ensure sorting is applied even in favorites view
            }

            // Save the favorites state to preserve it across sessions
            EditorPrefs.SetBool("AudioAlchemy_IsFavoritesActive", audioClips == favorites);

            Repaint(); // Refresh the UI to reflect the changes
        }

        // Adds or removes an audio clip from the user's favorites list
        private void ToggleFavorite(AudioClip clip)
        {
            PlaySFX(heartSFX);

            if (favorites.Contains(clip))
            {
                favorites.Remove(clip); // Remove from favorites
            }
            else
            {
                favorites.Add(clip); // Add to favorites
            }

            // Save the updated favorites list to the ScriptableObject
            audioClipListData.FavoriteClips.Clear();  // Clear the current list
            audioClipListData.FavoriteClips.AddRange(favorites);  // Add the updated list of favorites
            EditorUtility.SetDirty(audioClipListData);
            AssetDatabase.SaveAssets();

            // Update the filtered audio clips list based on the current view (all or favorites)
            if (audioClips == favorites)
            {
                filteredAudioClips = favorites
                    .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }
            else
            {
                filteredAudioClips = audioClips
                    .Where(clip => string.IsNullOrEmpty(searchString) || clip.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            Repaint(); // Refresh the UI to reflect the updated favorites
        }


        #endregion

        #region UI and Audio Management Methods

        // Calculates the available height for the scroll view by subtracting the heights of expanded sections
        private float CalculateScrollViewHeight()
        {
            float totalHeight = position.height;

            // Adjust for each section's height
            if (sectionOneExpanded)
            {
                totalHeight -= 150f; // Approximate height of the settings section when expanded
            }

            if (showAudioSources)
            {
                totalHeight -= 180f; // Approximate height of the audio sources section when expanded
            }

            return Mathf.Max(totalHeight, 100f); // Ensure there's always some space
        }

        // Gathers audio clips from a specified directory and adds them to the audioClips list
        private void GatherAudioClips(string rootPath)
        {
            try
            {
                audioClips.Clear(); // Clear the current list of audio clips

                string[] audioFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => (s.EndsWith(".wav") || s.EndsWith(".ogg")) && !Path.GetFileName(s).StartsWith("._"))
                    .Select(s => s.Replace("\\", "/").Replace(Application.dataPath, "Assets"))
                    .ToArray();

                foreach (string assetPath in audioFiles)
                {
                    if (audioClips.Any(clip => AssetDatabase.GetAssetPath(clip) == assetPath))
                    {
                        continue; // Skip if already loaded
                    }

                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (clip != null)
                    {
                        audioClips.Add(clip);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load audio file: {assetPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error gathering audio clips: {ex.Message}");
            }
        }


        // Stops the currently playing audio and resets playback settings
        private void StopAudio()
        {
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                sfxAudioSource.Stop();
                isPaused = false;
                pausedTime = 0f;

                StopAllPlayback(); // Stop all playback modes, including looping
            }
        }

        // Pauses the currently playing audio and stores the current playback time
        public void PauseAudio()
        {
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                pausedTime = sfxAudioSource.time;
                sfxAudioSource.Pause();
                isPaused = true;
                StopAllPlayback(); // Stop looping and other playback modes
            }
            else if (wasPlaying) // wasPlaying is a flag you set when something is actually playing
            {
                Debug.LogWarning("Cannot pause audio: AudioSource is null or not playing.");
            }
        }


        // Stops all playback modes and detaches associated update methods
        private void StopAllPlayback()
        {
            playSequentially = false;
            playRandomly = false;
            isLooping = false;

            EditorApplication.update -= CheckAndPlayNextSequentially;
            EditorApplication.update -= CheckAndPlayNextRandomly;
            EditorApplication.update -= LoopWithDelay;

            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.Stop();
            }
        }

        // Toggles the looping state for audio playback
        private void ToggleLooping()
        {
            isLooping = !isLooping;  // Toggle the looping state

            // Apply the looping state to all selected audio sources
            if (audioSourcesInScene.Any())
            {
                foreach (var source in audioSourcesInScene)
                {
                    if (source != null && audioSourceToggles[audioSourcesInScene.IndexOf(source)])
                    {
                        source.loop = isLooping;
                    }
                }
            }

            if (isLooping)
            {
                StartLooping();  // Start looping if enabled
            }
            else
            {
                StopAllPlayback();  // Stop playback immediately when looping is disabled
                RefreshLoopDelay();  // Reset the loop delay settings
            }

            Repaint();  // Ensure the UI updates to reflect the change
        }

        // Resets the loop delay and ensures proper playback settings
        private void RefreshLoopDelay()
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.Stop();
                sfxAudioSource.time = 0;
                nextPlayTime = 0;
                isPaused = false;

                // Reassign the current clip if valid
                if (currentClipIndex >= 0 && currentClipIndex < audioClips.Count)
                {
                    sfxAudioSource.clip = audioClips[currentClipIndex];
                }
            }

            // Recalculate and update the looping state
            UpdateLoopingState();
        }

        // Handles looping playback with a delay between repetitions
        private void LoopWithDelay()
        {
            if (isLooping && sfxAudioSource != null && !sfxAudioSource.isPlaying)
            {
                float delay = Mathf.Max(0, loopDelay);
                if (Time.realtimeSinceStartup >= nextPlayTime)
                {
                    sfxAudioSource.Play();
                    nextPlayTime = Time.realtimeSinceStartup + sfxAudioSource.clip.length + delay;
                }
            }
            else if (sfxAudioSource == null)
            {
                EditorApplication.update -= LoopWithDelay; // Stop looping if the AudioSource is null
            }
        }

        // Updates the looping state by detaching and reattaching the LoopWithDelay method
        private void UpdateLoopingState()
        {
            EditorApplication.update -= LoopWithDelay; // Detach first

            if (isLooping && sfxAudioSource != null)
            {
                StartLooping(); // Reattach if looping is enabled
            }
        }

        // Toggles the play-on-awake state for selected audio sources
        private void TogglePlayOnAwake()
        {
            playOnAwakeEnabled = !playOnAwakeEnabled;  // Toggle play on awake

            // Apply the play-on-awake state to all selected audio sources
            foreach (var source in audioSourcesInScene)
            {
                if (source != null && audioSourceToggles[audioSourcesInScene.IndexOf(source)])
                {
                    source.playOnAwake = playOnAwakeEnabled;
                }
            }
        }

        #endregion

        #region Audio Playback Management

        // Plays the selected audio clip from the beginning or resumes if paused
        public void PlayAudio()
        {
            if (currentClipIndex >= 0 && currentClipIndex < audioClips.Count)
            {
                AudioClip selectedClip = audioClips[currentClipIndex];

                if (sfxAudioSource != null)
                {
                    sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    sfxAudioSource.enabled = true;
                    sfxAudioSource.playOnAwake = playOnAwakeEnabled;

                    if (isPaused && pausedTime > 0)
                    {
                        // Resume playback from where it was paused
                        sfxAudioSource.time = pausedTime;
                        sfxAudioSource.Play();
                        isPaused = false;
                        pausedTime = 0f;
                    }
                    else
                    {
                        // Start playback from the beginning
                        sfxAudioSource.Stop();
                        sfxAudioSource.clip = selectedClip;
                        sfxAudioSource.time = 0f;
                        sfxAudioSource.Play();
                        selectedClipName = selectedClip.name;
                        isPaused = false;
                    }
                    wasPlaying = true;

                    // Apply the current clip to all selected audio sources
                    ApplyClipAndSettingsToSelectedSources();

                    // Handle very short clips
                    if (sfxAudioSource.clip != null && sfxAudioSource.clip.length <= 0.1f)
                    {
                        // Consider looping for very short clips
                        sfxAudioSource.loop = true;
                        sfxAudioSource.Play();

                        // Stop after twice the length to ensure full playback
                        EditorApplication.delayCall += () => StopLoopingShortClip();
                    }
                }
                else
                {
                    Debug.LogError("AudioSource is not assigned or has been destroyed!");
                }
            }
            else
            {
                Debug.LogWarning("Invalid currentClipIndex. Cannot play audio.");
            }
        }

        // Method to stop looping very short clips after playback
        private void StopLoopingShortClip()
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.loop = false;
            }
        }

        // Handles looping of the audio with a delay if specified
        private void LoopAudio()
        {
            if (sfxAudioSource != null && sfxAudioSource.enabled && isLooping && !sfxAudioSource.isPlaying && !isPaused && Time.realtimeSinceStartup >= nextPlayTime)
            {
                if (loopDelay > 0f && sfxAudioSource.clip != null && sfxAudioSource.clip.length > 0)
                {
                    float delay = sfxAudioSource.time + sfxAudioSource.clip.length - Time.realtimeSinceStartup;
                    nextPlayTime = Time.realtimeSinceStartup + loopDelay + delay;

                    if (delay <= 0f)
                    {
                        sfxAudioSource.Play();
                        nextPlayTime = Time.realtimeSinceStartup + loopDelay + sfxAudioSource.clip.length;
                    }
                }
                else
                {
                    sfxAudioSource.Play();
                    nextPlayTime = Time.realtimeSinceStartup + loopDelay + sfxAudioSource.clip.length;
                }
            }
        }

        // Schedules the next playback based on the loop delay and clip length
        private void ScheduleNextPlay()
        {
            if (sfxAudioSource != null && isLooping)
            {
                nextPlayTime = Time.realtimeSinceStartup + sfxAudioSource.clip.length + loopDelay;
            }
        }

        // Schedules the next looped playback, allowing for smooth looping
        private void ScheduleNextLoop()
        {
            if (isLooping && sfxAudioSource != null && sfxAudioSource.clip != null)
            {
                EditorApplication.delayCall += () =>
                {
                    if (isLooping)
                    {
                        PlayAudio();
                        ScheduleNextLoop(); // Schedule the next loop after the current clip finishes
                    }
                };
            }
        }

        // Starts the looping playback process by registering the looping method with the update callback
        private void StartLooping()
        {
            if (isLooping && sfxAudioSource != null && sfxAudioSource.clip != null)
            {
                EditorApplication.update += LoopWithDelay;
            }
        }

        // Updates the volume of the main audio source and all selected audio sources
        private void UpdateVolume()
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.volume = volume;
            }

            // Update volume for all selected audio sources
            foreach (var source in audioSourcesInScene)
            {
                if (source != null && audioSourceToggles[audioSourcesInScene.IndexOf(source)])
                {
                    source.volume = volume;
                }
            }
        }

        // Updates the pitch of the main audio source and all selected audio sources
        private void UpdatePitch()
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
                sfxAudioSource.pitch = pitch;
            }

            // Update pitch for all selected audio sources
            foreach (var source in audioSourcesInScene)
            {
                if (source != null && audioSourceToggles[audioSourcesInScene.IndexOf(source)])
                {
                    source.pitch = pitch;
                }
            }
        }

        #endregion

        private IEnumerator<AudioClip> CrossfadeToNextClip(AudioClip nextClip)
        {
            float currentTime = 0f;
            float startVolume = sfxAudioSource.volume;

            // Create a temporary GameObject to hold the new AudioSource
            GameObject tempObject = new GameObject("TempAudioSource");
            AudioSource nextSource = tempObject.AddComponent<AudioSource>();
            nextSource.clip = nextClip;
            nextSource.volume = 0f;
            nextSource.Play();

            while (currentTime < crossfadeDuration)
            {
                currentTime += Time.deltaTime;
                float t = currentTime / crossfadeDuration;

                sfxAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                nextSource.volume = Mathf.Lerp(0f, volume, t);

                yield return null;
            }

            sfxAudioSource.Stop();
            DestroyImmediate(sfxAudioSource.gameObject);
            sfxAudioSource = nextSource;
            DestroyImmediate(tempObject); // Clean up the temporary GameObject
        }

        #region State Management

        // Add these helper methods to the AudioAlchemyLab class
        public bool IsPlaying()
        {
            return sfxAudioSource != null && sfxAudioSource.isPlaying;
        }

        // Saves the current state of the editor to the ScriptableObject
        private void SaveState()
        {
            try
            {
                if (audioClipListData != null)
                {
                    audioClipListData.SelectedSortingOption = selectedSortingOption;
                    audioClipListData.IsFavoritesActive = (audioClips == favorites);
                    audioClipListData.ClipOrder.Clear();
                    audioClipListData.ClipOrder.AddRange(audioClips.ConvertAll(clip => clip.name));
                    audioClipListData.FavoriteClips.Clear();
                    audioClipListData.FavoriteClips.AddRange(favorites);

                    EditorUtility.SetDirty(audioClipListData);
                    AssetDatabase.SaveAssets();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving state: {ex.Message}");
            }
        }


        private void LoadState()
        {
            try
            {
                selectedSortingOption = EditorPrefs.GetInt("AudioAlchemy_SortingOption", 0);
                bool wasFavoritesActive = EditorPrefs.GetBool("AudioAlchemy_IsFavoritesActive", false);

                if (wasFavoritesActive)
                {
                    ToggleFavorites();  // Activate favorites view
                }

                string savedClipOrder = EditorPrefs.GetString("AudioAlchemy_ClipOrder", string.Empty);
                if (!string.IsNullOrEmpty(savedClipOrder))
                {
                    var orderedClipNames = savedClipOrder.Split(',');
                    audioClips = orderedClipNames
                        .Select(name => audioClips.FirstOrDefault(clip => clip != null && clip.name == name))
                        .Where(clip => clip != null)
                        .ToList();
                }

                SortAudioClips();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading state: {ex.Message}");
            }
        }
        private void SaveSelectedAudioSources()
        {
            if (audioClipListData != null)
            {
                List<string> selectedAudioSourceNames = audioSourcesInScene
                    .Where((source, index) => audioSourceToggles[index])
                    .Select(source => source.name)
                    .ToList();

                // Clear the existing list and add the new one
                audioClipListData.SelectedAudioSourceNames.Clear();
                audioClipListData.SelectedAudioSourceNames.AddRange(selectedAudioSourceNames);

                EditorUtility.SetDirty(audioClipListData);
                AssetDatabase.SaveAssets();
            }
        }


        private void LoadSelectedAudioSources()
        {
            if (audioClipListData != null && audioClipListData.SelectedAudioSourceNames != null)
            {
                for (int i = 0; i < audioSourcesInScene.Count; i++)
                {
                    // Set the toggle based on whether the name is in the saved list
                    audioSourceToggles[i] = audioClipListData.SelectedAudioSourceNames.Contains(audioSourcesInScene[i].name);
                }
            }
        }

        private void DeleteSelectedAudioSources()
        {
            if (audioSourcesInScene != null && audioSourcesInScene.Count > 0)
            {
                List<int> selectedIndices = new List<int>();

                // Collect the indices of the selected audio sources
                for (int i = 0; i < audioSourceToggles.Count; i++)
                {
                    if (audioSourceToggles[i])
                    {
                        selectedIndices.Add(i);
                    }
                }

                if (selectedIndices.Count > 0)
                {
                    // Display a simple confirmation dialog
                    bool shouldDelete = EditorUtility.DisplayDialog(
                        "Confirm Deletion",
                        $"Are you sure you want to delete {selectedIndices.Count} selected audio sources?",
                        "Delete",
                        "Cancel"
                    );

                    if (shouldDelete)
                    {
                        // Play the trash SFX on confirmation
                        PlaySFX(trashSFX);

                        // Proceed to delete the selected audio sources
                        for (int i = selectedIndices.Count - 1; i >= 0; i--)
                        {
                            int indexToDelete = selectedIndices[i];
                            DestroyImmediate(audioSourcesInScene[indexToDelete].gameObject);
                            audioSourcesInScene.RemoveAt(indexToDelete);
                            audioSourceToggles.RemoveAt(indexToDelete);
                        }

                        // Refresh the list after deletion
                        AutoDetectAudioSources();
                        Repaint();
                    }
                    else
                    {
                        // Play the cancel SFX if deletion is canceled
                        PlaySFX(cancelSFX);
                    }
                }
            }
        }

        private void SaveTogglesBeforeSorting()
        {
            togglesBeforeSorting = new List<bool>(audioSourceToggles);
        }

        private void RestoreTogglesAfterSorting()
        {
            if (togglesBeforeSorting != null)
            {
                for (int i = 0; i < audioSourceToggles.Count; i++)
                {
                    audioSourceToggles[i] = togglesBeforeSorting[i];
                }
            }
        }

        private void SortAudioSources()
        {
            SaveTogglesBeforeSorting();
            switch (selectedSortingOption)
            {
                case 0: // Alphabetical Ascending
                    audioSourcesInScene = audioSourcesInScene.OrderBy(source => source.name).ToList();
                    //Debug.Log("Audio Sources sorted Alphabetically Ascending.");
                    break;
                case 1: // Alphabetical Descending
                    audioSourcesInScene = audioSourcesInScene.OrderByDescending(source => source.name).ToList();
                    //Debug.Log("Audio Sources sorted Alphabetically Descending.");
                    break;
                case 2: // Numerical Ascending
                    audioSourcesInScene = audioSourcesInScene
                        .OrderBy(source => ExtractNumberFromName(source.name))
                        .ThenBy(source => source.name) // In case of a tie in numbers, sort alphabetically
                        .ToList();
                    //Debug.Log("Audio Sources sorted Numerically Ascending.");
                    break;
                case 3: // Numerical Descending
                    audioSourcesInScene = audioSourcesInScene
                        .OrderByDescending(source => ExtractNumberFromName(source.name))
                        .ThenByDescending(source => source.name) // In case of a tie in numbers, sort alphabetically
                        .ToList();
                    //Debug.Log("Audio Sources sorted Numerically Descending.");
                    break;
                default:
                    Debug.LogWarning("Invalid sorting option selected for audio sources.");
                    break;
            }

            // Update toggles to match the sorted list
            audioSourceToggles = new List<bool>(new bool[audioSourcesInScene.Count]);
            for (int i = 0; i < audioSourcesInScene.Count; i++)
            {
                int originalIndex = audioSourcesInScene.FindIndex(source => source.name == audioSourcesInScene[i].name);
                if (originalIndex >= 0 && originalIndex < togglesBeforeSorting.Count)
                {
                    audioSourceToggles[i] = togglesBeforeSorting[originalIndex];
                }
            }
            RestoreTogglesAfterSorting();
            Repaint(); // Refresh the UI after sorting
        }


        #endregion
    }
}