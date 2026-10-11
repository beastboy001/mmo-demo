//*****************************************
//功能说明：网络系统接口
//*****************************************
public interface INetSystem : ISystem
{
    bool IsConnected { get; }

    int UserId { get; }

    int EntityId { get; }

    bool Connect();

    void Disconnect();

    void Send(short msgId, object body);

    void Update();

    void SendLogin(string username, string password);

    void SendRegister(string username, string password);

    void SendCharacterList();

    void SendCreateCharacter(PlayerData pd);

    void SendDeleteCharacter(int characterId);

    void SendEnterMap(PlayerData pd, int mapId, UnityEngine.Vector3 worldPos, float yaw);

    void SendEntityState(UnityEngine.Vector3 worldPos, float yaw, float animSpeed);

    void SendFriendList();

    void SendFriendRequestList();

    void SendAddFriend(int targetCharacterId);

    void SendRespondFriend(int requestId, bool accept);

    void SendChatHistory(int peerCharacterId, int limit = 50);

    void SendChatMessage(int toCharacterId, string content);
}
