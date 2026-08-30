using System;
using System.Collections.Generic;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    /// <summary>
    /// Immutable authoritative summary of a completed or abandoned run.
    /// Meta/UI systems may consume this record but never mutate simulation state through it.
    /// </summary>
    public sealed class RunResult
    {
        private readonly string[] _orbitCoreIds;

        public RunResult(
            string stageId,
            float rawTimeSeconds,
            float penaltySeconds,
            int damageCount,
            IReadOnlyList<string> orbitCoreIds,
            RotorMode mode,
            bool practice,
            bool completed)
        {
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage id is required.", nameof(stageId));
            if (rawTimeSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(rawTimeSeconds));
            if (penaltySeconds < 0f) throw new ArgumentOutOfRangeException(nameof(penaltySeconds));
            if (damageCount < 0) throw new ArgumentOutOfRangeException(nameof(damageCount));

            StageId = stageId;
            RawTimeSeconds = rawTimeSeconds;
            PenaltySeconds = penaltySeconds;
            DamageCount = damageCount;
            Mode = mode;
            Practice = practice;
            Completed = completed;

            if (orbitCoreIds == null || orbitCoreIds.Count == 0)
            {
                _orbitCoreIds = Array.Empty<string>();
            }
            else
            {
                _orbitCoreIds = new string[orbitCoreIds.Count];
                for (int i = 0; i < orbitCoreIds.Count; i++)
                    _orbitCoreIds[i] = orbitCoreIds[i] ?? string.Empty;
            }
        }

        public string StageId { get; }
        public float RawTimeSeconds { get; }
        public float PenaltySeconds { get; }
        public float DisplayTimeSeconds => RawTimeSeconds + PenaltySeconds;
        public int DamageCount { get; }
        public IReadOnlyList<string> OrbitCoreIds => _orbitCoreIds;
        public RotorMode Mode { get; }
        public bool Practice { get; }
        public bool Completed { get; }
        public bool Perfect => Completed && DamageCount == 0;
        public bool EligibleForStandardLeaderboard => Completed && Mode == RotorMode.Standard && !Practice;
    }
}
