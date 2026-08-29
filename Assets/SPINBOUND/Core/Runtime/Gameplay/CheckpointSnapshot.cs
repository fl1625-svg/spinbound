using Spinbound.Core.Simulation;

namespace Spinbound.Core.Gameplay
{
    public readonly struct CheckpointSnapshot
    {
        public CheckpointSnapshot(string id, RotorState respawnState)
        {
            Id = id;
            RespawnState = respawnState;
        }

        public string Id { get; }
        public RotorState RespawnState { get; }
    }
}
