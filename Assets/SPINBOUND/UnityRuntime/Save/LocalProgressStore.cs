using System;
using System.Collections.Generic;
using UnityEngine;
using Spinbound.Meta;

namespace Spinbound.UnityRuntime.Save
{
    public sealed class LocalProgressStore
    {
        public const int SchemaVersion = 1;
        private const string PlayerPrefsKey = "spinbound.progress.v1";

        public PlayerProgress Load()
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new PlayerProgress();

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.schemaVersion != SchemaVersion || data.stages == null)
                    return new PlayerProgress();

                var records = new List<StageProgressRecord>(data.stages.Count);
                for (int i = 0; i < data.stages.Count; i++)
                {
                    StageData stage = data.stages[i];
                    if (stage == null || string.IsNullOrWhiteSpace(stage.stageId))
                        continue;

                    records.Add(new StageProgressRecord(
                        stage.stageId,
                        stage.hasCleared,
                        Mathf.Max(0f, stage.bestValidTimeSeconds),
                        Mathf.Max(-1, stage.bestDamageCount),
                        (StageMasteryFlags)stage.masteryFlags,
                        stage.orbitCoreIds ?? Array.Empty<string>()));
                }

                return new PlayerProgress(records);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SPINBOUND progress save could not be read; starting fresh. {exception.Message}");
                return new PlayerProgress();
            }
        }

        public void Save(PlayerProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            var data = new SaveData
            {
                schemaVersion = SchemaVersion,
                stages = new List<StageData>(),
            };

            foreach (StageProgressRecord record in progress.Records)
            {
                var cores = new string[record.OrbitCoreIds.Count];
                for (int i = 0; i < cores.Length; i++)
                    cores[i] = record.OrbitCoreIds[i];

                data.stages.Add(new StageData
                {
                    stageId = record.StageId,
                    hasCleared = record.HasCleared,
                    bestValidTimeSeconds = record.BestValidTimeSeconds,
                    bestDamageCount = record.BestDamageCount,
                    masteryFlags = (int)record.MasteryFlags,
                    orbitCoreIds = cores,
                });
            }

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class SaveData
        {
            public int schemaVersion;
            public List<StageData> stages;
        }

        [Serializable]
        private sealed class StageData
        {
            public string stageId;
            public bool hasCleared;
            public float bestValidTimeSeconds;
            public int bestDamageCount = -1;
            public int masteryFlags;
            public string[] orbitCoreIds;
        }
    }
}
