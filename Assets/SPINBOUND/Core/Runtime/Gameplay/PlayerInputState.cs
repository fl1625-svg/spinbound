using System.Numerics;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    public readonly struct PlayerInputState
    {
        public PlayerInputState(Vector2 moveDirection, bool buttonA, bool buttonB)
        {
            MoveDirection = moveDirection;
            ButtonA = buttonA;
            ButtonB = buttonB;
        }

        public Vector2 MoveDirection { get; }
        public bool ButtonA { get; }
        public bool ButtonB { get; }

        public RotorIntent ToRotorIntent() =>
            new(MoveDirection, SpeedTierResolver.Resolve(buttonA: ButtonA, buttonB: ButtonB));
    }
}
