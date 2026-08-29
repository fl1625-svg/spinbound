using NumericsVector2 = System.Numerics.Vector2;
using UnityEngine;
using UnityEngine.InputSystem;
using Spinbound.Core.Collision;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;
using Spinbound.Platform;
using Spinbound.Presentation;
using Spinbound.Presentation.UI;
using Spinbound.Presentation.Vfx;
using Spinbound.Worlds.W01.DaisyHighlands;

namespace Spinbound.UnityRuntime
{
    public sealed class UnityRotorGameHost : MonoBehaviour
    {
        [SerializeField] private RotorPresenter _presenter;
        [SerializeField] private AdventureHud _hud;
        [SerializeField] private RotorFxDirector _fx;
        [SerializeField] private bool _assistMode;

        private RunSession _session;
        private FixedStepRotorRunner _runner;
        private IPlatformBridge _platform;
        private bool _paused;
        private bool _gameplayReported;

        public void Configure(RotorPresenter presenter, AdventureHud hud = null, RotorFxDirector fx = null)
        {
            _presenter = presenter;
            _hud = hud;
            _fx = fx;
        }

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Keep keyboard gameplay independent from DOM focus on the browser checkpoint.
            WebGLInput.captureAllKeyboardInput = true;
#endif
            var mode = _assistMode ? RotorMode.Assist : RotorMode.Standard;
            _session = new RunSession(W01_01CourseDefinition.StartFor(mode));
            _runner = new FixedStepRotorRunner(new CollisionWorld(W01_01CourseDefinition.Colliders), _session);
            _platform = new CrazyGamesPlatformBridge();
            _presenter?.Apply(_session.State);
            _hud?.SetHearts(3);
            _hud?.SetTime(0f);
        }

        private void Start() => ReportGameplayStart();

        private void Update()
        {
            var keyboard = Keyboard.current;
            var webDown = BrowserKeyboard.DownMask;
            var webPressed = BrowserKeyboard.ConsumePressedMask();

            if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || Has(webPressed, BrowserKeyboard.Escape))
            {
                SetPaused(!_paused);
            }
            if (_paused) return;

            var direction = NumericsVector2.Zero;
            var buttonA = false;
            var buttonB = false;

            if ((keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)) ||
                Has(webDown, BrowserKeyboard.A) || Has(webDown, BrowserKeyboard.Left))
            {
                direction.X -= 1f;
            }
            if ((keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)) ||
                Has(webDown, BrowserKeyboard.D) || Has(webDown, BrowserKeyboard.Right))
            {
                direction.X += 1f;
            }
            if ((keyboard != null && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)) ||
                Has(webDown, BrowserKeyboard.S) || Has(webDown, BrowserKeyboard.Down))
            {
                direction.Y -= 1f;
            }
            if ((keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)) ||
                Has(webDown, BrowserKeyboard.W) || Has(webDown, BrowserKeyboard.Up))
            {
                direction.Y += 1f;
            }

            buttonA = (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)) ||
                      Has(webDown, BrowserKeyboard.Shift);
            buttonB = (keyboard != null && keyboard.spaceKey.isPressed) || Has(webDown, BrowserKeyboard.Space);

            if ((keyboard != null && keyboard.rKey.wasPressedThisFrame) || Has(webPressed, BrowserKeyboard.Restart))
            {
                _session.RestartFromCheckpoint();
            }

            var input = new PlayerInputState(direction, buttonA, buttonB);
            var tier = SpeedTierResolver.Resolve(buttonA, buttonB);
            _runner.Tick(Time.unscaledDeltaTime, input);
            _presenter?.Apply(_session.State);
            _fx?.SetSpeedTier(tier);
            _hud?.SetTime(_session.ElapsedSeconds);
            _hud?.SetHearts(Mathf.Max(0, 3 - _session.Hits));
        }

        private static bool Has(int mask, int bit) => (mask & bit) != 0;

        private void OnDisable()
        {
            if (_gameplayReported)
            {
                _platform?.GameplayStop();
                _gameplayReported = false;
            }
        }

        private void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            if (_paused) ReportGameplayStop(); else ReportGameplayStart();
        }

        private void ReportGameplayStart()
        {
            if (_gameplayReported) return;
            _platform?.GameplayStart();
            _gameplayReported = true;
        }

        private void ReportGameplayStop()
        {
            if (!_gameplayReported) return;
            _platform?.GameplayStop();
            _gameplayReported = false;
        }
    }
}
