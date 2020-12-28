using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;


class Message
{
    public const int buffer_size = 102400;
    public byte[] buffer = new byte[buffer_size];

    private int _start = 0;
    private int _remain = buffer_size;
    public int Start
    {
        set
        {
            this._start = value;
            this._remain = buffer_size - this._start;
        }
        get
        {
            return this._start;
        }
    }

    public int Remain
    {
        get { return this._remain; }
    }

    public void UnpackMessage(int dataLen, Action<byte, byte[]> OperationRequest)
    {
        //message type: len(4) requestcode(1) parameters(x)
        //message: <length> 1 username=123|password=123
        Start += dataLen;
        //Console.WriteLine("len: " + Start);
        //Console.WriteLine("remain: " + Remain);
        while (true)
        {
            if (Start <= 4)
            {
                Console.WriteLine("end----------");
                return;
            }

            int count = BitConverter.ToInt32(buffer, 0);
            //Console.WriteLine("count: " + count);
            if ((Start - 4) >= count)
            {
                //0-3 length; 4 requestCode; 5-x requestParameter
                byte requestCode = buffer[4];
                byte[] parameters = new byte[count - 1];
                Array.Copy(buffer, 5, parameters, 0, count - 1);
                OperationRequest(requestCode, parameters);

                Array.Copy(buffer, 4 + count, buffer, 0, Start - 4 - count);
                this.Start -= (count + 4);
                //Console.WriteLine("remain: " + Remain);
            }
            else
            {
                break;
            }
        }
    }

    public static byte[] PackMessage<T>(PB_RequestCode requestCode, T parameter)
    {
        byte opCode_byte = (byte)requestCode;
        byte[] respCode_bytes = new byte[1] { opCode_byte };
        byte[] parameter_bytes = Serialize<T>(parameter);
        byte[] data = respCode_bytes.Concat(parameter_bytes).ToArray();

        int length = data.Length;
        byte[] len_bytes = BitConverter.GetBytes(length);
        byte[] res = len_bytes.Concat(data).ToArray();
        return res;
    }

    public static byte[] Serialize<T>(T t)
    {
        return (t as IMessage).ToByteArray();
    }

    public static T Deserialize<T>(byte[] bytes) where T : IMessage<T>, new()
    {
        MessageParser<T> messageParser = new MessageParser<T>(() => { return new T(); });
        return messageParser.ParseFrom(bytes);
    }


    public static string DictionaryToString(Dictionary<byte, object> dict)
    {
        string str = "";
        foreach (byte key in dict.Keys)
        {
            str += (key + "=" + dict[key]);
            str += "|";
        }
        str = str.Substring(0, str.Length - 1);
        return str;
    }
    public static string DictionaryToString(Dictionary<string, object> dict)
    {
        string str = "";
        foreach (string key in dict.Keys)
        {
            str += (key + "=" + dict[key]);
            str += "|";
        }
        str = str.Substring(0, str.Length - 1);
        return str;
    }

    public static Dictionary<string, object> StringToDictionary(string value)
    {
        if (value.Length < 1)
        {
            return null;
        }
        Dictionary<string, object> dic = new Dictionary<string, object>();

        string[] dicStrs = value.Split('|');
        foreach (string str in dicStrs)
        {
            string[] strs = str.Split('=');
            dic.Add(strs[0], strs[1]);
        }
        return dic;
    }
}

