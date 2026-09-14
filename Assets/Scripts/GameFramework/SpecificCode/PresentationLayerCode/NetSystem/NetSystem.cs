using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

//*****************************************
//功能说明：网络系统
//*****************************************
public class NetSystem : INetSystem
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread recvThread;
    private volatile bool running;

    private readonly Queue<PendingPacket> recvQueue = new Queue<PendingPacket>();
    private readonly object queueLock = new object();

    public bool IsConnected => client != null && client.Connected;
    public int UserId { get; private set; }
    public int EntityId { get; private set; }

    private struct PendingPacket
    {
        public short msgId;
        public string json;
    }

    public void Init()
    {
        Debug.Log("[NetSystem] Init");
    }

    public bool Connect()
    {
        if (IsConnected)
        {
            return true;
        }

        try
        {
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(NetConfig.Host, NetConfig.Port);
            stream = client.GetStream();
            running = true;
            recvThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = "NetRecv"
            };
            recvThread.Start();
            Debug.Log("[NetSystem] 已连接 " + NetConfig.Host + ":" + NetConfig.Port);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetSystem] 连接失败: " + ex.Message);
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        running = false;
        try { stream?.Close(); } catch { /* ignore */ }
        try { client?.Close(); } catch { /* ignore */ }
        stream = null;
        client = null;
        UserId = 0;
        EntityId = 0;
    }

    public void Send(short msgId, object body)
    {
        if (!IsConnected || stream == null)
        {
            Debug.LogWarning("[NetSystem] 未连接，无法发送 msgId=" + msgId);
            return;
        }

        try
        {
            byte[] packet = NetPacket.Build(msgId, body);
            stream.Write(packet, 0, packet.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetSystem] 发送失败: " + ex.Message);
            Disconnect();
        }
    }

    public void SendLogin(string username, string password)
    {
        if (!Connect())
        {
            this.SendEvent<LoginResultEvent>(new LoginResMsg
            {
                ok = false,
                message = "无法连接服务器",
                userId = 0
            });
            return;
        }

        Send(MsgId.LoginReq, new LoginReqMsg { username = username, password = password });
    }

    public void SendRegister(string username, string password)
    {
        if (!Connect())
        {
            this.SendEvent<RegisterResultEvent>(new RegisterResMsg
            {
                ok = false,
                message = "无法连接服务器"
            });
            return;
        }

        Send(MsgId.RegisterReq, new RegisterReqMsg { username = username, password = password });
    }

    public void SendCharacterList()
    {
        if (!IsConnected || UserId <= 0)
        {
            this.SendEvent<CharacterListResultEvent>(new CharacterListResMsg
            {
                ok = false,
                message = "未登录",
                characters = new CharacterDataMsg[0]
            });
            return;
        }

        Send(MsgId.CharacterListReq, new EmptyMsg());
    }

    public void SendCreateCharacter(PlayerData pd)
    {
        if (!IsConnected || UserId <= 0 || pd == null)
        {
            this.SendEvent<CreateCharacterResultEvent>(new CreateCharacterResMsg
            {
                ok = false,
                message = "未登录或数据无效"
            });
            return;
        }

        Send(MsgId.CreateCharacterReq, new CreateCharacterReqMsg
        {
            name = pd.id,
            role = (int)pd.role,
            gender = (int)pd.gender,
            faceTypeId = pd.faceTypeId,
            hairStyleId = pd.hairStyleId,
            hairColorId = pd.hairColorId,
            skinId = pd.skinId,
            eyeColorId = pd.eyeColorId,
            faceEmotionId = pd.faceEmotionId
        });
    }

    public void SendDeleteCharacter(int characterId)
    {
        if (!IsConnected || UserId <= 0)
        {
            this.SendEvent<DeleteCharacterResultEvent>(new DeleteCharacterResMsg
            {
                ok = false,
                message = "未登录",
                characterId = characterId
            });
            return;
        }

        Send(MsgId.DeleteCharacterReq, new DeleteCharacterReqMsg { characterId = characterId });
    }

    public void SendEnterMap(PlayerData pd, int mapId, Vector3 worldPos, float yaw)
    {
        if (!IsConnected || UserId <= 0 || pd == null)
        {
            Debug.LogWarning("[NetSystem] 未登录，无法进图");
            return;
        }

        Send(MsgId.EnterMapReq, new EnterMapReqMsg
        {
            mapId = mapId,
            characterId = pd.characterId,
            name = pd.id,
            x = worldPos.x,
            y = worldPos.y,
            z = worldPos.z,
            yaw = yaw,
            role = (int)pd.role,
            gender = (int)pd.gender,
            faceTypeId = pd.faceTypeId,
            hairStyleId = pd.hairStyleId,
            hairColorId = pd.hairColorId,
            skinId = pd.skinId,
            eyeColorId = pd.eyeColorId,
            faceEmotionId = pd.faceEmotionId,
            level = pd.level
        });
    }

    public void SendEntityState(Vector3 worldPos, float yaw, float animSpeed)
    {
        if (!IsConnected || EntityId <= 0)
        {
            return;
        }

        Send(MsgId.EntityState, new EntityStateMsg
        {
            entityId = EntityId,
            x = worldPos.x,
            y = worldPos.y,
            z = worldPos.z,
            yaw = yaw,
            animSpeed = animSpeed
        });
    }

    public void SendFriendList()
    {
        if (!IsConnected)
        {
            this.SendEvent<FriendListResultEvent>(new FriendListResMsg
            {
                ok = false,
                message = "未连接",
                friends = new FriendEntryMsg[0]
            });
            return;
        }

        Send(MsgId.FriendListReq, new EmptyMsg());
    }

    public void SendFriendRequestList()
    {
        if (!IsConnected)
        {
            this.SendEvent<FriendRequestListResultEvent>(new FriendRequestListResMsg
            {
                ok = false,
                message = "未连接",
                requests = new FriendRequestEntryMsg[0]
            });
            return;
        }

        Send(MsgId.FriendRequestListReq, new EmptyMsg());
    }

    public void SendAddFriend(int targetCharacterId)
    {
        if (!IsConnected || targetCharacterId <= 0)
        {
            this.SendEvent<AddFriendResultEvent>(new AddFriendResMsg
            {
                ok = false,
                message = "未连接或目标无效"
            });
            return;
        }

        Send(MsgId.AddFriendReq, new AddFriendReqMsg { targetCharacterId = targetCharacterId });
    }

    public void SendRespondFriend(int requestId, bool accept)
    {
        if (!IsConnected || requestId <= 0)
        {
            this.SendEvent<RespondFriendResultEvent>(new RespondFriendResMsg
            {
                ok = false,
                message = "未连接或请求无效"
            });
            return;
        }

        Send(MsgId.RespondFriendReq, new RespondFriendReqMsg
        {
            requestId = requestId,
            accept = accept
        });
    }

    public void Update()
    {
        while (true)
        {
            PendingPacket packet;
            lock (queueLock)
            {
                if (recvQueue.Count == 0)
                {
                    break;
                }

                packet = recvQueue.Dequeue();
            }

            Dispatch(packet.msgId, packet.json);
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            while (running && stream != null)
            {
                if (!NetPacket.TryRead(stream, out short msgId, out string json))
                {
                    break;
                }

                lock (queueLock)
                {
                    recvQueue.Enqueue(new PendingPacket { msgId = msgId, json = json });
                }
            }
        }
        catch (Exception ex)
        {
            if (running)
            {
                Debug.LogWarning("[NetSystem] 收包线程结束: " + ex.Message);
            }
        }
        finally
        {
            running = false;
        }
    }

    private void Dispatch(short msgId, string json)
    {
        switch (msgId)
        {
            case MsgId.LoginRes:
            {
                LoginResMsg res = JsonUtility.FromJson<LoginResMsg>(json) ??
                                  new LoginResMsg { ok = false, message = "解析登录回包失败" };
                if (res.ok)
                {
                    UserId = res.userId;
                }

                this.SendEvent<LoginResultEvent>(res);
                break;
            }

            case MsgId.RegisterRes:
            {
                RegisterResMsg res = JsonUtility.FromJson<RegisterResMsg>(json) ??
                                     new RegisterResMsg { ok = false, message = "解析注册回包失败" };
                this.SendEvent<RegisterResultEvent>(res);
                break;
            }

            case MsgId.CharacterListRes:
            {
                CharacterListResMsg res = JsonUtility.FromJson<CharacterListResMsg>(json) ??
                                          new CharacterListResMsg { ok = false, message = "解析角色列表失败" };
                if (res.characters == null)
                {
                    res.characters = new CharacterDataMsg[0];
                }

                this.SendEvent<CharacterListResultEvent>(res);
                break;
            }

            case MsgId.CreateCharacterRes:
            {
                CreateCharacterResMsg res = JsonUtility.FromJson<CreateCharacterResMsg>(json) ??
                                            new CreateCharacterResMsg { ok = false, message = "解析创建角色回包失败" };
                this.SendEvent<CreateCharacterResultEvent>(res);
                break;
            }

            case MsgId.DeleteCharacterRes:
            {
                DeleteCharacterResMsg res = JsonUtility.FromJson<DeleteCharacterResMsg>(json) ??
                                            new DeleteCharacterResMsg { ok = false, message = "解析删除角色回包失败" };
                this.SendEvent<DeleteCharacterResultEvent>(res);
                break;
            }

            case MsgId.EnterMapRes:
            {
                EnterMapResMsg res = JsonUtility.FromJson<EnterMapResMsg>(json) ??
                                     new EnterMapResMsg { ok = false, message = "解析进图回包失败" };
                if (res.ok)
                {
                    EntityId = res.entityId;
                }

                this.SendEvent<EnterMapResultEvent>(res);
                break;
            }

            case MsgId.EntityEnter:
            {
                EntityEnterMsg msg = JsonUtility.FromJson<EntityEnterMsg>(json);
                if (msg != null)
                {
                    this.SendEvent<EntityEnterEvent>(msg);
                }

                break;
            }

            case MsgId.EntityLeave:
            {
                EntityLeaveMsg msg = JsonUtility.FromJson<EntityLeaveMsg>(json);
                if (msg != null)
                {
                    this.SendEvent<EntityLeaveEvent>(msg);
                }

                break;
            }

            case MsgId.EntityState:
            {
                EntityStateMsg msg = JsonUtility.FromJson<EntityStateMsg>(json);
                if (msg != null)
                {
                    this.SendEvent<EntityStateEvent>(msg);
                }

                break;
            }

            case MsgId.FriendListRes:
            {
                FriendListResMsg res = JsonUtility.FromJson<FriendListResMsg>(json) ??
                                       new FriendListResMsg { ok = false, message = "解析好友列表失败" };
                if (res.friends == null)
                {
                    res.friends = new FriendEntryMsg[0];
                }

                this.SendEvent<FriendListResultEvent>(res);
                break;
            }

            case MsgId.FriendRequestListRes:
            {
                FriendRequestListResMsg res = JsonUtility.FromJson<FriendRequestListResMsg>(json) ??
                                              new FriendRequestListResMsg
                                              {
                                                  ok = false,
                                                  message = "解析好友申请失败"
                                              };
                if (res.requests == null)
                {
                    res.requests = new FriendRequestEntryMsg[0];
                }

                this.SendEvent<FriendRequestListResultEvent>(res);
                break;
            }

            case MsgId.AddFriendRes:
            {
                AddFriendResMsg res = JsonUtility.FromJson<AddFriendResMsg>(json) ??
                                      new AddFriendResMsg { ok = false, message = "解析加好友回包失败" };
                this.SendEvent<AddFriendResultEvent>(res);
                break;
            }

            case MsgId.RespondFriendRes:
            {
                RespondFriendResMsg res = JsonUtility.FromJson<RespondFriendResMsg>(json) ??
                                          new RespondFriendResMsg { ok = false, message = "解析回应好友失败" };
                this.SendEvent<RespondFriendResultEvent>(res);
                break;
            }

            case MsgId.Heartbeat:
                break;

            default:
                Debug.LogWarning("[NetSystem] 未处理协议号: " + msgId);
                break;
        }
    }
}

[Serializable]
public class EmptyMsg
{
}
