using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    /// <summary>
    /// Lives only in BootstrapScene (Build Settings index 0) - immediately hands off to
    /// MainMenu. Kept scene-content-free so index 0 never needs game logic of its own,
    /// matching the Lumenwake template's Bootstrap -> MainScene handoff.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(SceneInBuild.MainMenu);
        }
    }
}
