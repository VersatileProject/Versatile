using Versatile.Networks.Services;

namespace Versatile.Plays.Networks;

public interface IMessageCommand
{
    public MessagePackageInfo ToMessagePackageInfo();
}

