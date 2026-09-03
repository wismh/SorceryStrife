using UnityEngine;
using Zenject;

namespace Game
{
    /// <summary>
    /// Plays looping menu music via IAudioSystem, which is itself the persistent ProjectContext
    /// singleton - no DontDestroyOnLoad/duplicate-guard needed here anymore.
    /// </summary>
    public class BackgroundMusic : MonoBehaviour
    {
        [SerializeField] private SoundData _music;

        private IAudioSystem _audioSystem;

        [Inject]
        public void Construct(IAudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
        }

        private void Start()
        {
            _audioSystem.PlayMusic(_music);
        }
    }
}
