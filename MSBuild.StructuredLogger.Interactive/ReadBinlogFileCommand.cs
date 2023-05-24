namespace MSBuild.StructuredLogger.Interactive;

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;

public class ReadBinlogFileCommand : ConnectKernelCommand
{
    public ReadBinlogFileCommand() : base("msbuild", "Reads a MSBuild binlog file")
    {
        Add(BinlogFileArgument);
    }

    public Argument<FileInfo> BinlogFileArgument { get;} = 
        new("binlog", "The MSBuild binlog file to read");

    public override async Task<IEnumerable<Microsoft.DotNet.Interactive.Kernel>> ConnectKernelsAsync(KernelInvocationContext context, InvocationContext commandLineContext)
    {
        var binlogFile = commandLineContext.ParseResult.GetValueForArgument(BinlogFileArgument);
        IKernelConnector connector = new StructuredLogKernelConnector(binlogFile);
        var localName = commandLineContext.ParseResult.GetValueForOption(KernelNameOption);
        var kernel = await connector.CreateKernelAsync(localName!);
        return new [] { kernel };
    }
}