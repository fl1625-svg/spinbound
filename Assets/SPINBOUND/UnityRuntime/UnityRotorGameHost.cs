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
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) SetPaused(!_paused);
            if (_paused) return;

            var direction = NumericsVector2.Zero;
            var buttonA = false;
            var buttonB = false;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.X -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.X += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.Y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.Y += 1f;
                buttonA = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                buttonB = keyboard.spaceKey.isPressed;
                if (keyboard.rKey.wasPressedThisFrame) _session.RestartFromCheckpoint();
            }

            var input = new PlayerInputState(direction, buttonA, buttonB);
            var tier = SpeedTierResolver.Resolve(buttonA, buttonB);
            _runner.Tick(Time.unscaledDeltaTime, input);
            _presenter?.Apply(_session.State);
            _fx?.SetSpeedTier(tier);
            _hud?.SetTime(_session.ElapsedSeconds);
            _hud?.SetHearts(Mathf.Max(0, 3 - _session.Hits));
        }

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
