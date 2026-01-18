using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AudioAlchemy.AudioTools
{
    public class InteractiveLogo : MonoBehaviour
    {
        public GameObject logo; // Reference to the logo GameObject
        public GameObject documentationPanel; // The UI panel containing the documentation
        public Button exitButton; // The exit play mode button
        public AudioClip clickSound; // Sound to play on logo click
        public AudioClip exitSound; // Sound to play on exit button click
        public TextMeshProUGUI textMeshPro; // The TextMeshPro component for displaying the text
        public ScrollRect scrollRect; // The ScrollRect component to make the text scrollable
        public GameObject hoverText; // Reference to the hover text GameObject

        // Animation settings
        public float shrinkDuration = 0.1f; // Duration to shrink the logo
        public float growDuration = 0.1f; // Duration to grow the logo slightly larger than original
        public float restoreDuration = 0.1f; // Duration to restore the logo to original size
        public float waitAfterAnimation = 0.1f; // Time to wait after the animation before switching panels
        public float smoothness = 1.0f; // Controls the smoothness of the animation
        public float scaleMultiplier = 1.1f; // How much to scale beyond the original size

        private AudioSource audioSource;
        private bool isAnimating = false;
        private Vector3 originalScale;

        void Start()
        {
            // Get the AudioSource component on the parent object
            audioSource = GetComponent<AudioSource>();

            // Store the original scale of the logo
            originalScale = logo.transform.localScale;

            // Initially hide the documentation panel, exit button, and hover text
            documentationPanel.SetActive(false);
            exitButton.gameObject.SetActive(false);
            hoverText.SetActive(false); // Hide hover text initially

            // Add listener to the exit button
            exitButton.onClick.AddListener(OnExitButtonClick);
        }

        void Update()
        {
            // Check if the mouse is over the logo
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == logo && !isAnimating)
                {
                    // Show the hover text
                    hoverText.SetActive(true);

                    // Position the hover text near the logo
                    Vector3 screenPosition = Camera.main.WorldToScreenPoint(logo.transform.position);
                    hoverText.transform.position = screenPosition + new Vector3(0, 30, 0); // Adjust the offset as needed

                    // Check if the user clicks the logo
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(OnLogoClick());
                    }
                }
                else
                {
                    // Hide the hover text when not hovering over the logo
                    hoverText.SetActive(false);
                }
            }
            else
            {
                // Hide the hover text when the mouse is not over the logo
                hoverText.SetActive(false);
            }
        }

        IEnumerator OnLogoClick()
        {
            isAnimating = true;

            // Play the click sound
            audioSource.PlayOneShot(clickSound);

            // Start the scale animation
            yield return StartCoroutine(ScaleLogo());

            // Wait for the specified delay after the animation
            yield return new WaitForSeconds(waitAfterAnimation);

            // Show the documentation panel after the animation completes
            ShowDocumentation();
        }

        IEnumerator ScaleLogo()
        {
            // Shrink the logo
            Vector3 shrinkScale = originalScale * 0.7f;
            yield return StartCoroutine(AnimateScale(logo.transform, originalScale, shrinkScale, shrinkDuration));

            // Scale it slightly larger than original size
            Vector3 growScale = originalScale * scaleMultiplier;
            yield return StartCoroutine(AnimateScale(logo.transform, shrinkScale, growScale, growDuration));

            // Restore the logo to its original size
            yield return StartCoroutine(AnimateScale(logo.transform, growScale, originalScale, restoreDuration));
        }

        IEnumerator AnimateScale(Transform target, Vector3 fromScale, Vector3 toScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime * smoothness; // Adjust smoothness here
                target.localScale = Vector3.Lerp(fromScale, toScale, elapsed / duration);
                yield return null;
            }
            target.localScale = toScale;
        }

        void ShowDocumentation()
        {
            // Hide the logo and show the documentation panel
            logo.SetActive(false);
            documentationPanel.SetActive(true);
            exitButton.gameObject.SetActive(true);

            // Reset the scroll position to the top
            scrollRect.verticalNormalizedPosition = 1f;

            isAnimating = false;
        }

        private void OnExitButtonClick()
        {
            StartCoroutine(ExitAfterDelay());
        }

        IEnumerator ExitAfterDelay()
        {
            // Play the exit sound
            audioSource.PlayOneShot(exitSound);

            // Wait for a short delay before exiting
            yield return new WaitForSeconds(0.5f);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else
            Application.Quit(); // Quit the application if built
#endif
        }

        // Optionally, a method to update the text dynamically
        public void UpdateText(string newText)
        {
            textMeshPro.text = newText;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
