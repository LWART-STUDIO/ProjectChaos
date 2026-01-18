using UnityEngine;
using System.Collections;

namespace AudioAlchemy.AudioTools
{
    public class OpenWebsiteButton : MonoBehaviour
    {
        [Tooltip("Enter the website it will open when the button is clicked")]
        public string url; // The URL to open

        [Tooltip("Delay before opening the website (in seconds)")]
        public float delayBeforeOpening = 0.5f; // The delay in seconds

        public void OpenWebsite()
        {
            if (!string.IsNullOrEmpty(url))
            {
                StartCoroutine(OpenWebsiteWithDelay());
            }
            else
            {
                Debug.LogWarning("URL is not set or is empty.");
            }
        }

        private IEnumerator OpenWebsiteWithDelay()
        {
            // Wait for the specified delay before opening the website
            yield return new WaitForSeconds(delayBeforeOpening);

            // Open the website
            Application.OpenURL(url);
        }
    }
}
