using UnityEngine;

namespace AudioAlchemy.AudioTools
{
    public class AudioClipKeyboardHandler
    {
        private AudioAlchemyLab audioAlchemyLab;

        public AudioClipKeyboardHandler(AudioAlchemyLab audioAlchemyLabInstance)
        {
            audioAlchemyLab = audioAlchemyLabInstance;
        }

        public void HandleKeyboardShortcuts()
        {
            Event e = Event.current;

            // Only process keyboard input if the search field isn't focused
            if (GUI.GetNameOfFocusedControl() != "SearchField")
            {
                if (e.type == EventType.KeyDown)
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.LeftArrow:
                        case KeyCode.UpArrow:
                            audioAlchemyLab.SkipBackAudio();
                            e.Use(); // Mark the event as used so it doesn't propagate further
                            break;

                        case KeyCode.RightArrow:
                        case KeyCode.DownArrow:
                            audioAlchemyLab.SkipForwardAudio();
                            e.Use(); // Mark the event as used so it doesn't propagate further
                            break;

                        case KeyCode.Space:
                            if (audioAlchemyLab.IsPlaying())
                            {
                                audioAlchemyLab.PauseAudio();
                            }
                            else
                            {
                                audioAlchemyLab.PlayAudio();
                            }
                            e.Use(); // Mark the event as used so it doesn't propagate further
                            break;
                    }
                }
            }
        }
    }
}
