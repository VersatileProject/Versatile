using System;
using System.Text.Json;
using Versatile.Networks.Services;

namespace Versatile.Plays.Networks;

public abstract class MessageCommand<T> where T : IMessageCommand
{
    protected abstract MessageType MessageType { get; }

    public MessagePackageInfo ToMessagePackageInfo()
    {
        var type = GetType();
        var json = JsonSerializer.Serialize(this, type, new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        });

        var msg = new MessagePackageInfo()
        {
            Key = MessageType,
            Timestamp = DateTime.UtcNow,
        };
        msg.Add("type", type.Name);
        msg.Add("json", json);
        return msg;
    }

    public static T FromMessage(MessagePackageInfo package)
    {
        var baseType = typeof(T);
        var typename = package.Get<string>("type");
        var cmdType = baseType.Assembly.GetType(baseType.Namespace + '.' + typename);
        var json = package.Get<string>("json");
        var obj = JsonSerializer.Deserialize(json, cmdType, new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        });
        if (obj is T cmd)
        {
            return cmd;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}

