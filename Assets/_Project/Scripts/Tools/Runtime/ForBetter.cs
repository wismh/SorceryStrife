using UnityEngine;

namespace Game
{
    public class ForBetter : MonoBehaviour
    {
        private void Start()
        {
        #if !UNITY_WEBGL
            gameObject.SetActive(false);
        #endif
        }
    }
}