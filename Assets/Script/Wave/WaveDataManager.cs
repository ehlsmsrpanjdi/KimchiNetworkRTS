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
                instance.LoadTestData();
            }
            return instance;
        }
    }

    private Dictionary<int, WaveData> dataDict = new Dictionary<int, WaveData>();

    void LoadTestData()
    {
        // Wave 1: 좀비 10마리
        var wave1 = new WaveData
        {
            waveID = 1,
            waveNumber = 1,
            spawnInfos = new WaveSpawnInfo[]
            {
                new WaveSpawnInfo
                {
                    monsterID = 1,          // 좀비
                    baseSpawnCount = 10,    // 기본 10마리
                    perPlayerSpawnCount = 2 // 플레이어당 +2마리
                }
            },
            waveDuration = 60f, // 60초
            bossMonsterID = -1  // 보스 없음
        };

        // Wave 2: 좀비 10마리 + 궁수 20마리
        var wave2 = new WaveData
        {
            waveID = 2,
            waveNumber = 2,
            spawnInfos = new WaveSpawnInfo[]
            {
                new WaveSpawnInfo
                {
                    monsterID = 1,
                    baseSpawnCount = 10,
                    perPlayerSpawnCount = 3
                },
                new WaveSpawnInfo
                {
                    monsterID = 2,          // 궁수
                    baseSpawnCount = 20,
                    perPlayerSpawnCount = 5
                }
            },
            waveDuration = 60f,
            bossMonsterID = -1
        };

        // Wave 3: 좀비 10마리 + 보스 1마리
        var wave3 = new WaveData
        {
            waveID = 3,
            waveNumber = 3,
            spawnInfos = new WaveSpawnInfo[]
            {
                new WaveSpawnInfo
                {
                    monsterID = 1,
                    baseSpawnCount = 10,
                    perPlayerSpawnCount = 3
                }
            },
            waveDuration = 60f,
            bossMonsterID = 6 // 좀비 왕
        };

        dataDict[1] = wave1;
        dataDict[2] = wave2;
        dataDict[3] = wave3;
    }

    public WaveData GetData(int waveID)
    {
        if (dataDict.TryGetValue(waveID, out WaveData data))
        {
            return data;
        }
        LogHelper.LogError($"WaveData not found: {waveID}");
        return null;
    }
}