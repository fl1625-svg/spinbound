using System;
using System.Collections.Generic;
using Spinbound.Core.Gameplay;

namespace Spinbound.Meta
{
    public sealed class PlayerProgress
    {
        private readonly Dictionary<string, StageProgressRecord> _records;

        public PlayerProgress()
        {
            _records = new Dictionary<string, StageProgressRecord>(StringComparer.Ordinal);
        }

        public PlayerProgress(IEnumerable<StageProgressRecord> records)
            : this()
        {
            if (records == null) return;
            foreach (StageProgressRecord record in records)
            {
                if (record != null)
                    _records[record.StageId] = record;
            }
        }

        public IReadOnlyCollection<StageProgressRecord> Records => _records.Values;

        public StageProgressRecord Get(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage id is required.", nameof(stageId));
            return _records.TryGetValue(stageId, out StageProgressRecord record)
                ? record
                : new StageProgressRecord(stageId);
        }

        public bool HasCleared(string stageId) => Get(stageId).HasCleared;

        public bool HasMastery(string stageId, StageMasteryFlags flag) =>
            (Get(stageId).MasteryFlags & flag) == flag;

        public StageProgressRecord Merge(
            RunResult result,
            IReadOnlyList<string> authoredOrbitCoreIds,
            float masterTimeSeconds)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            StageProgressRecord merged = Get(result.StageId).Merge(result, authoredOrbitCoreIds, masterTimeSeconds);
            _records[result.StageId] = merged;
            return merged;
        }

        public void Replace(StageProgressRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _records[record.StageId] = record;
        }
    }
}
