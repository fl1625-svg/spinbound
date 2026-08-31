using UnityEngine;
using UnityEngine.SceneManagement;
using Spinbound.Presentation.WorldMap;
using Spinbound.UnityRuntime.Save;

namespace Spinbound.UnityRuntime
{
    public sealed class WorldMapSceneHost : MonoBehaviour
    {
        [SerializeField] private WorldMapController _controller;
        private readonly LocalProgressStore _store = new();

        public void Configure(WorldMapController controller)
        {
            _controller = controller;
        }

        private void Awake()
        {
            _controller ??= FindFirstObjectByType<WorldMapController>();
            if (_controller == null)
            {
                Debug.LogError("SPINBOUND WorldMapSceneHost requires a WorldMapController.");
                enabled = false;
                return;
            }

            _controller.ConfigureProgress(_store.Load());
            _controller.StageRequested += LoadStage;
        }

        private void OnDestroy()
        {
            if (_controller != null)
                _controller.StageRequested -= LoadStage;
        }

        private static void LoadStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(stageId, LoadSceneMode.Single);
        }
    }
}
