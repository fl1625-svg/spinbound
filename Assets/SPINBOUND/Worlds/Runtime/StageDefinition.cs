using System;
using System.Collections.Generic;
using System.Numerics;
using Spinbound.Core.Collision;
using Spinbound.Core.Simulation;

namespace Spinbound.Worlds
{
    public enum StageKind : byte
    {
        Normal,
        Trial,
        Boss
    }

    public sealed class StageDefinition
    {
        public StageDefinition(
            string id,
            string displayName,
            string hook,
            StageKind kind,
            RotorState startState,
            Vector2 finishCenter,
            float finishRadius,
            IReadOnlyList<CourseCollider> colliders,
            float masterTimeSeconds = 0f,
            int deterministicPhaseCount = 0)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Stage id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Stage display name is required.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(hook)) throw new ArgumentException("Stage hook is required.", nameof(hook));
            if (finishRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(finishRadius));
            if (colliders == null) throw new ArgumentNullException(nameof(colliders));
            if (masterTimeSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(masterTimeSeconds));
            if (deterministicPhaseCount < 0) throw new ArgumentOutOfRangeException(nameof(deterministicPhaseCount));

            Id = id;
            DisplayName = displayName;
            Hook = hook;
            Kind = kind;
            StartState = startState;
            FinishCenter = finishCenter;
            FinishRadius = finishRadius;
            Colliders = colliders;
            MasterTimeSeconds = masterTimeSeconds;
            DeterministicPhaseCount = deterministicPhaseCount;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Hook { get; }
        public StageKind Kind { get; }
        public RotorState StartState { get; }
        public Vector2 FinishCenter { get; }
        public float FinishRadius { get; }
        public IReadOnlyList<CourseCollider> Colliders { get; }
        public float MasterTimeSeconds { get; }
        public int DeterministicPhaseCount { get; }

        public RotorState StartFor(RotorMode mode) => StartState.With(mode: mode, bumpVelocity: Vector2.Zero);
    }
}
