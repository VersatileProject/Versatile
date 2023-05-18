using System.Buffers;
using System.Diagnostics;
using SuperSocket.ProtoBase;

namespace Versatile.Networks.Services;

public class MessagePipelineFilter : FixedHeaderPipelineFilter<MessagePackageInfo>
{
    public MessagePipelineFilter() : base(8)
    {

    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        var magic = reader.ReadString(4);
        Debug.Assert(magic == "VMSG");
        //reader.Advance(4);
        reader.TryReadLittleEndian(out short len);
        reader.Advance(2);
        return len;
    }

    protected override MessagePackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
    {
        var package = MessagePackageInfo.FromBytes(ref buffer);
        return package;
    }

}
