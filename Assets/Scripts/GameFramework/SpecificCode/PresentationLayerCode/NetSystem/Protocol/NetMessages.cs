using System;

//*****************************************
//功能说明：协议号 + 消息体（必须和服务器一致）
//*****************************************
public static class MsgId
{
    public const short LoginReq = 1;
    public const short LoginRes = 2;
    public const short RegisterReq = 3;
    public const short RegisterRes = 4;

    public const short CharacterListReq = 5;
    public const short CharacterListRes = 6;
    public const short CreateCharacterReq = 7;
    public const short CreateCharacterRes = 8;
    public const short DeleteCharacterReq = 9;
    public const short DeleteCharacterRes = 15;

    public const short EnterMapReq = 10;
    public const short EnterMapRes = 11;
    public const short EntityEnter = 12;
    public const short EntityLeave = 13;
    public const short EntityState = 14;

    public const short FriendListReq = 20;
    public const short FriendListRes = 21;
    public const short FriendRequestListReq = 22;
    public const short FriendRequestListRes = 23;
    public const short AddFriendReq = 24;
    public const short AddFriendRes = 25;
    public const short RespondFriendReq = 26;
    public const short RespondFriendRes = 27;

    public const short ChatHistoryReq = 30;
    public const short ChatHistoryRes = 31;
    public const short ChatSendReq = 32;
    public const short ChatSendRes = 33;
    public const short ChatPush = 34;

    public const short Heartbeat = 100;
}

[Serializable]
public class LoginReqMsg
{
    public string username;
    public string password;
}

[Serializable]
public class LoginResMsg
{
    public bool ok;
    public string message;
    public int userId;
}

[Serializable]
public class RegisterReqMsg
{
    public string username;
    public string password;
}

[Serializable]
public class RegisterResMsg
{
    public bool ok;
    public string message;
}

[Serializable]
public class CharacterDataMsg
{
    public int characterId;
    public string name;
    public int level;
    public int hp;
    public int mp;
    public int coin;
    public int role;
    public int gender;
    public int faceTypeId;
    public int hairStyleId;
    public int hairColorId;
    public int skinId;
    public int eyeColorId;
    public int faceEmotionId;
    public int mapId;
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class CharacterListResMsg
{
    public bool ok;
    public string message;
    public CharacterDataMsg[] characters;
}

[Serializable]
public class CreateCharacterReqMsg
{
    public string name;
    public int role;
    public int gender;
    public int faceTypeId;
    public int hairStyleId;
    public int hairColorId;
    public int skinId;
    public int eyeColorId;
    public int faceEmotionId;
}

[Serializable]
public class CreateCharacterResMsg
{
    public bool ok;
    public string message;
    public CharacterDataMsg character;
}

[Serializable]
public class DeleteCharacterReqMsg
{
    public int characterId;
}

[Serializable]
public class DeleteCharacterResMsg
{
    public bool ok;
    public string message;
    public int characterId;
}

[Serializable]
public class EnterMapReqMsg
{
    public int mapId;
    public int characterId;
    public string name;
    public float x;
    public float y;
    public float z;
    public float yaw;
    public int role;
    public int gender;
    public int faceTypeId;
    public int hairStyleId;
    public int hairColorId;
    public int skinId;
    public int eyeColorId;
    public int faceEmotionId;
    public int level;
}

[Serializable]
public class EnterMapResMsg
{
    public bool ok;
    public string message;
    public int entityId;
}

[Serializable]
public class EntityEnterMsg
{
    public int entityId;
    public int characterId;
    public string name;
    public float x;
    public float y;
    public float z;
    public float yaw;
    public int role;
    public int gender;
    public int faceTypeId;
    public int hairStyleId;
    public int hairColorId;
    public int skinId;
    public int eyeColorId;
    public int faceEmotionId;
    public int level;
}

[Serializable]
public class EntityLeaveMsg
{
    public int entityId;
}

[Serializable]
public class EntityStateMsg
{
    public int entityId;
    public float x;
    public float y;
    public float z;
    public float yaw;
    public float animSpeed;
}

[Serializable]
public class FriendEntryMsg
{
    public int characterId;
    public string name;
    public int level;
    public bool online;
}

[Serializable]
public class FriendRequestEntryMsg
{
    public int requestId;
    public int fromCharacterId;
    public string name;
    public int level;
    public bool online;
}

[Serializable]
public class FriendListResMsg
{
    public bool ok;
    public string message;
    public FriendEntryMsg[] friends;
}

[Serializable]
public class FriendRequestListResMsg
{
    public bool ok;
    public string message;
    public FriendRequestEntryMsg[] requests;
}

[Serializable]
public class AddFriendReqMsg
{
    public int targetCharacterId;
}

[Serializable]
public class AddFriendResMsg
{
    public bool ok;
    public string message;
}

[Serializable]
public class RespondFriendReqMsg
{
    public int requestId;
    public bool accept;
}

[Serializable]
public class RespondFriendResMsg
{
    public bool ok;
    public string message;
}

[Serializable]
public class ChatMessageItemMsg
{
    public int id;
    public int fromCharacterId;
    public int toCharacterId;
    public string content;
    public string createdAt;
}

[Serializable]
public class ChatHistoryReqMsg
{
    public int peerCharacterId;
    public int limit;
}

[Serializable]
public class ChatHistoryResMsg
{
    public bool ok;
    public string message;
    public int peerCharacterId;
    public ChatMessageItemMsg[] messages;
}

[Serializable]
public class ChatSendReqMsg
{
    public int toCharacterId;
    public string content;
}

[Serializable]
public class ChatSendResMsg
{
    public bool ok;
    public string message;
    public ChatMessageItemMsg msg;
}

[Serializable]
public class ChatPushMsg
{
    public ChatMessageItemMsg msg;
}

public class LoginResultEvent { }
public class RegisterResultEvent { }
public class CharacterListResultEvent { }
public class CreateCharacterResultEvent { }
public class DeleteCharacterResultEvent { }
public class EnterMapResultEvent { }
public class EntityEnterEvent { }
public class EntityLeaveEvent { }
public class EntityStateEvent { }
public class FriendListResultEvent { }
public class FriendRequestListResultEvent { }
public class AddFriendResultEvent { }
public class RespondFriendResultEvent { }
public class ChatHistoryResultEvent { }
public class ChatSendResultEvent { }
public class ChatPushEvent { }

/// <summary>服务器角色数据 → 客户端 PlayerData</summary>
public static class CharacterDataConverter
{
    public static PlayerData ToPlayerData(CharacterDataMsg msg)
    {
        if (msg == null)
        {
            return null;
        }

        PlayerData pd = new PlayerData();
        pd.characterId = msg.characterId;
        pd.id = msg.name;
        pd.level = msg.level;
        pd.hp = msg.hp;
        pd.mp = msg.mp;
        pd.coin = msg.coin;
        pd.role = (ROLE)msg.role;
        pd.gender = (GENDER)msg.gender;
        pd.faceTypeId = msg.faceTypeId;
        pd.hairStyleId = msg.hairStyleId;
        pd.hairColorId = msg.hairColorId;
        pd.skinId = msg.skinId;
        pd.eyeColorId = msg.eyeColorId;
        pd.faceEmotionId = msg.faceEmotionId;
        pd.x = msg.x;
        pd.y = msg.y;
        pd.z = msg.z;
        return pd;
    }
}

