using System;
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
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.UnityRuntime
{
    public sealed class UnityRotorGameHost : MonoBehaviour
    {
        [SerializeField] private RotorPresenter _presenter;
        [SerializeField] private AdventureHud _hud;
        [SerializeField] private RotorFxDirector _fx;
        [SerializeField] private World1PlaytestFlow _flow;
        [SerializeField] private bool _assistMode;
        [SerializeField] private string _stageId = W01_01_FirstSpin.Id;

        private StageDefinition _stage;
        private RunSession _session;
        private FixedStepRotorRunner _runner;
        private IPlatformBridge _platform;
        private RotorMode _mode;
        private bool _paused;
        private bool _completed;
        private bool _gameplayReported;
        private bool _flowEventsBound;
        private int _presentedHitCount;

        public void Configure(RotorPresenter presenter, AdventureHud hud = null, RotorFxDirector fx = null, World1PlaytestFlow flow = null)
        {
            _presenter = presenter;
            _hud = hud;
            _fx = fx;
            _flow = flow;
        }

        public void ConfigureStageId(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new ArgumentException("Stage id is required.", nameof(stageId));

            _stage = W01ReferenceRoutes.Get(stageId).Stage;
            _stageId = stageId;
        }

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
            _stage ??= W01ReferenceRoutes.Get(_stageId).Stage;
            _mode = _assistMode ? RotorMode.Assist : RotorMode.Standard;
            _session = new RunSession(_stage.StartFor(_mode));
            _runner = new FixedStepRotorRunner(new CollisionWorld(_stage.Colliders), _session);
            _platform = new CrazyGamesPlatformBridge();
            _presenter?.Apply(_session.State);
            _hud?.SetCourse(_stage.Id, _stage.DisplayName);
            _hud?.SetHearts(3);
            _hud?.SetTime(0f);
            _hud?.SetOrbitCores(0, 3);
            _hud?.SetSpeedTier(SpeedTier.Speed1);
            _presentedHitCount = _session.Hits;
        }

        private void Start()
        {
            BindFlowEvents();
            ApplySettings(_flow != null ? _flow.Settings : AccessibilitySettings.Load());
            ReportGameplayStart();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;

            bool escapePressed =
                (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
                Input.GetKeyDown(KeyCode.Escape);
            if (escapePressed)
                SetPaused(!_paused);
            if (_paused || _completed) return;

            var direction = NumericsVector2.Zero;

            bool left =
                (keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)) ||
                Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool right =
                (keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)) ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            bool down =
                (keyboard != null && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)) ||
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            bool up =
                (keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)) ||
                Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

            if (left) direction.X -= 1f;
            if (right) direction.X += 1f;
            if (down) direction.Y -= 1f;
            if (up) direction.Y += 1f;

            bool buttonA =
                (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)) ||
                Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool buttonB =
                (keyboard != null && keyboard.spaceKey.isPressed) ||
                Input.GetKey(KeyCode.Space);

            bool restartPressed =
                (keyboard != null && keyboard.rKey.wasPressedThisFrame) ||
                Input.GetKeyDown(KeyCode.R);
            if (restartPressed)
                _session.RestartFromCheckpoint();

            var input = new PlayerInputState(direction, buttonA, buttonB);
            var tier = SpeedTierResolver.Resolve(buttonA, buttonB);
            _runner.Tick(Time.unscaledDeltaTime, input);
            _presenter?.Apply(_session.State);
            _presenter?.SetSpeedTier(tier);
            if (_session.Hits > _presentedHitCount)
            {
                _presenter?.PlayHitRecoil();
                _presentedHitCount = _session.Hits;
            }
            _fx?.SetSpeedTier(tier);
            _hud?.SetTime(_session.ElapsedSeconds);
            _hud?.SetHearts(Mathf.Max(0, 3 - _session.Hits));
            _hud?.SetSpeedTier(tier);

            if (HasReachedFinish())
                CompleteStage();
        }

        private bool HasReachedFinish()
        {
            float dx = _session.State.Position.X - _stage.FinishCenter.X;
            float dy = _session.State.Position.Y - _stage.FinishCenter.Y;
            float radius = _stage.FinishRadius;
            return dx * dx + dy * dy <= radius * radius;
        }

        private void CompleteStage()
        {
            if (_completed) return;
            _completed = true;
            ReportGameplayStop();
            _flow?.CompleteStage(_stage, _session.ElapsedSeconds, _session.Hits, _mode);
        }

        private void OnDisable()
        {
            UnbindFlowEvents();
            if (_gameplayReported)
            {
                _platform?.GameplayStop();
                _gameplayReported = false;
            }
        }

        private void BindFlowEvents()
        {
            if (_flow == null || _flowEventsBound) return;
            _flow.ModalPauseChanged += OnModalPauseChanged;
            _flow.SettingsChanged += ApplySettings;
            _flowEventsBound = true;
        }

        private void UnbindFlowEvents()
        {
            if (_flow == null || !_flowEventsBound) return;
            _flow.ModalPauseChanged -= OnModalPauseChanged;
            _flow.SettingsChanged -= ApplySettings;
            _flowEventsBound = false;
        }

        private void OnModalPauseChanged(bool paused)
        {
            SetPaused(paused);
        }

        private void ApplySettings(AccessibilitySettings settings)
        {
            if (settings == null) return;
            _fx?.ApplyAccessibility(settings);
        }

        private void SetPaused(bool paused)
        {
            if (_paused == paused || _completed) return;
            _paused = paused;
            if (_paused) ReportGameplayStop(); else ReportGameplayStart();
        }

        private void ReportGameplayStart()
        {
            if (_gameplayReported || _completed) return;
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
