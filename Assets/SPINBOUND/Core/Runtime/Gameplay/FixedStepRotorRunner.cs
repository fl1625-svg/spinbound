using System;
using Spinbound.Core.Collision;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    public sealed class FixedStepRotorRunner
    {
        private readonly CollisionWorld _collisionWorld;
        private readonly RunSession _session;

        public FixedStepRotorRunner(CollisionWorld collisionWorld, RunSession session)
        {
            _collisionWorld = collisionWorld ?? throw new ArgumentNullException(nameof(collisionWorld));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public float AccumulatorSeconds { get; private set; }
        public CollisionResult LastCollision { get; private set; } = CollisionResult.Clear(float.PositiveInfinity);

        public int Tick(float renderDeltaSeconds, in PlayerInputState input)
        {
            if (renderDeltaSeconds < 0f || float.IsNaN(renderDeltaSeconds) || float.IsInfinity(renderDeltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(renderDeltaSeconds));
            }

            // Avoid an unbounded catch-up spiral after a browser tab stall.
            AccumulatorSeconds = MathF.Min(AccumulatorSeconds + renderDeltaSeconds, 0.25f);
            var steps = 0;
            var intent = input.ToRotorIntent();

            while (AccumulatorSeconds + 1e-8f >= RotorTuning.FixedDeltaSeconds)
            {
                StepOnce(intent);
                AccumulatorSeconds -= RotorTuning.FixedDeltaSeconds;
                steps++;
            }

            return steps;
        }

        private void StepOnce(in RotorIntent intent)
        {
            var previous = _session.State;
            var candidate = RotorSimulation.Step(previous, intent, RotorTuning.FixedDeltaSeconds);
            var previousCapsule = ToCapsule(previous);
            var candidateCapsule = ToCapsule(candidate);
            LastCollision = _collisionWorld.SweepCapsule(previousCapsule, candidateCapsule);

            if (LastCollision.Hit)
            {
                _session.RegisterHit();
                // Keep the last valid authoritative state; presentation may play impact response separately.
                _session.AdvanceTo(previous, RotorTuning.FixedDeltaSeconds);
                return;
            }

            _session.AdvanceTo(candidate, RotorTuning.FixedDeltaSeconds);
        }

        private static Capsule2D ToCapsule(in RotorState state) =>
            new(state.Position, state.AngleDeg, state.HalfLengthMeters, state.RadiusMeters);
    }
}
