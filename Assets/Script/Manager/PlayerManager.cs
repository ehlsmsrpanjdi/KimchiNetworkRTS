using System.Collections.Generic;

public class PlayerManager
{
    static PlayerManager instance;
    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }
    }

    private Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

    // ✅ 로컬 플레이어 캐싱
    private Player localPlayer;

    public void AddPlayer(ulong playerID, Player player)
    {
        if (!players.ContainsKey(playerID))
        {
            players[playerID] = player;
            LogHelper.Log($"✅ Player registered: {playerID}");
        }
    }

    // ✅ 로컬 플레이어 설정
    public void SetLocalPlayer(Player player)
    {
        localPlayer = player;
        LogHelper.Log($"✅ Local player set: {player.OwnerClientId}");
    }

    public void RemovePlayer(ulong playerID)
    {
        if (players.Remove(playerID))
        {
            LogHelper.Log($"✅ Player unregistered: {playerID}");
        }
    }

    public Player GetPlayer(ulong playerID)
    {
        if (players.TryGetValue(playerID, out Player player))
        {
            return player;
        }
        LogHelper.LogWarrning($"Player not found: {playerID}");
        return null;
    }

    // ✅ 로컬 플레이어 가져오기
    public Player GetLocalPlayer()
    {
        return localPlayer;
    }
}