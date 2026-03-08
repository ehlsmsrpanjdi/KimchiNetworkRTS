using System.Collections.Generic;

public class WaveDataManager
{
    static WaveDataManager instance;
    public static WaveDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WaveDataManager();
            }
            return instance;
        }
    }

    // waveNumber → WaveData
    private Dictionary<int, WaveData> dataDict = new Dictionary<int, WaveData>();

    // ========== 엑셀 Waves 시트 전체 반영 ==========
    /// <summary>
    /// 엑셀 1행을 버퍼에 추가. 동일 waveNumber면 spawnInfo만 추가
    /// </summary>
    public void AddRow(
        Dictionary<int, WaveData> buffer,
        int waveNumber, string enemyID,
        int countBase, int perPlayer,
        float duration, float interval,
        bool isBoss, int cardChoices)
    {
        if (!buffer.TryGetValue(waveNumber, out var waveData))
        {
            waveData = new WaveData
            {
                waveNumber = waveNumber,
                isBossWave = isBoss,
                cardChoices = cardChoices,
                spawnInfos = new WaveSpawnInfo[0]
            };
            buffer[waveNumber] = waveData;
        }

        // isBossWave는 한 행이라도 true면 전체 true
        if (isBoss) waveData.isBossWave = true;

        // spawnInfos 배열에 추가
        var newInfo = new WaveSpawnInfo
        {
            monsterID = enemyID,
            countBase = countBase,
            countPerPlayer = perPlayer,
            spawnDurationSec = duration,
            spawnIntervalSec = interval
        };

        var oldList = new List<WaveSpawnInfo>(waveData.spawnInfos);
        oldList.Add(newInfo);
        waveData.spawnInfos = oldList.ToArray();
    }

    public int Count => dataDict.Count;

    public void Clear() => dataDict.Clear();
    public void LoadFromBuffer(Dictionary<int, WaveData> buffer)
    {
        foreach (var kvp in buffer)
            dataDict[kvp.Key] = kvp.Value;
    }

    // ========== 조회 ==========

    public WaveData GetData(int waveNumber)
    {
        if (dataDict.TryGetValue(waveNumber, out var data))
            return data;

        LogHelper.LogError($"WaveData not found: wave {waveNumber}");
        return null;
    }

    public bool HasWave(int waveNumber) => dataDict.ContainsKey(waveNumber);
    public int TotalWaveCount => dataDict.Count;
}
