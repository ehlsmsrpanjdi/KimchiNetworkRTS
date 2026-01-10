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
    private Player localPlayer;

    public void AddPlayer(ulong playerID, Player player)
    {
        if (!players.ContainsKey(playerID))
        {
            players[playerID] = player;
            LogHelper.Log($"✅ Player registered: {playerID}");
        }
    }

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

    public Player GetLocalPlayer()
    {
        return localPlayer;
    }

    // ✅ 모든 플레이어 가져오기
    public List<Player> GetAllPlayers()
    {
        return new List<Player>(players.Values);
    }

    // ✅ 살아있는 플레이어만 가져오기
    public List<Player> GetAlivePlayers()
    {
        List<Player> alivePlayers = new List<Player>();
        foreach (var player in players.Values)
        {
            if (player != null && player.gameObject.activeSelf)
            {
                alivePlayers.Add(player);
            }
        }
        return alivePlayers;
    }
}