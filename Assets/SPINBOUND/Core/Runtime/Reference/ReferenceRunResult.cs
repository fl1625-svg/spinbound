namespace Spinbound.Core.Reference
{
    public readonly struct ReferenceRunResult
    {
        public ReferenceRunResult(bool cleared, int hits, float elapsedSeconds, float minimumClearanceMeters, string failure)
        {
            Cleared = cleared;
            Hits = hits;
            ElapsedSeconds = elapsedSeconds;
            MinimumClearanceMeters = minimumClearanceMeters;
            Failure = failure;
        }

        public bool Cleared { get; }
        public int Hits { get; }
        public float ElapsedSeconds { get; }
        public float MinimumClearanceMeters { get; }
        public string Failure { get; }
    }
}
