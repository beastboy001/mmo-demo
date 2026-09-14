using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

//*****************************************
//功能说明：封包读写（与服务器 PacketIO 格式一致）
// 格式：int32 长度 + int16 协议号 + UTF8 JSON
//*****************************************
public static class NetPacket
{
    public static byte[] Build(short msgId, object body)
    {
        string json = JsonUtility.ToJson(body);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        int len = 2 + jsonBytes.Length;
        byte[] packet = new byte[4 + len];
        BitConverter.GetBytes(len).CopyTo(packet, 0);
        BitConverter.GetBytes(msgId).CopyTo(packet, 4);
        Buffer.BlockCopy(jsonBytes, 0, packet, 6, jsonBytes.Length);
        return packet;
    }

    /// <summary>从流里读一个完整包；连接断开返回 false</summary>
    public static bool TryRead(NetworkStream stream, out short msgId, out string json)
    {
        msgId = 0;
        json = null;

        byte[] lenBuf = ReadExact(stream, 4);
        if (lenBuf == null)
        {
            return false;
        }

        int len = BitConverter.ToInt32(lenBuf, 0);
        if (len < 2 || len > 1024 * 1024)
        {
            throw new Exception("非法包长度: " + len);
        }

        byte[] body = ReadExact(stream, len);
        if (body == null)
        {
            return false;
        }

        msgId = BitConverter.ToInt16(body, 0);
        json = Encoding.UTF8.GetString(body, 2, len - 2);
        return true;
    }

    private static byte[] ReadExact(NetworkStream stream, int size)
    {
        byte[] buffer = new byte[size];
        int offset = 0;
        while (offset < size)
        {
            int n;
            try
            {
                n = stream.Read(buffer, offset, size - offset);
            }
            catch
            {
                return null;
            }

            if (n <= 0)
            {
                return null;
            }

            offset += n;
        }

        return buffer;
    }
}
