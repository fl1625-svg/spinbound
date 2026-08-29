using System;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    public sealed class RunSession
    {
        private readonly RotorState _initialState;
        private CheckpointSnapshot? _checkpoint;

        public RunSession(RotorState initialState)
        {
            _initialState = initialState;
            State = initialState;
        }

        public RotorState State { get; private set; }
        public int Hits { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public string LatestCheckpointId => _checkpoint?.Id ?? string.Empty;

        public void AdvanceTo(RotorState state, float fixedDeltaSeconds)
        {
            if (fixedDeltaSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(fixedDeltaSeconds));
            State = state;
            ElapsedSeconds += fixedDeltaSeconds;
        }

        public void RegisterHit() => Hits++;

        public void SetCheckpoint(CheckpointSnapshot checkpoint) => _checkpoint = checkpoint;

        public void RestartFromCheckpoint()
        {
            State = _checkpoint?.RespawnState ?? _initialState;
        }

        public void RestartRun()
        {
            State = _initialState;
            Hits = 0;
            ElapsedSeconds = 0f;
            _checkpoint = null;
        }
    }
}
