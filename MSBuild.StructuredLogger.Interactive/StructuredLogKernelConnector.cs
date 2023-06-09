﻿namespace MSBuild.StructuredLogger.Interactive;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Connection;

public class StructuredLogKernelConnector : IKernelConnector
{
    private readonly FileInfo _binlogFile;

    public StructuredLogKernelConnector(FileInfo binlogFile)
    {
        _binlogFile = binlogFile;
    }

    Task<Kernel> IKernelConnector.CreateKernelAsync(string kernelName)
    {
        var kernel = new StructuredLogKernel($"msbuild-binlog-{kernelName}", _binlogFile);
        return Task.FromResult<Kernel>(kernel);
    }

    public static void AddStructuredLogKernelToCurrentRoot()
    {
        if (KernelInvocationContext.Current is { } context &&
            context.HandlingKernel.RootKernel is CompositeKernel root)
        {
            AddStructuredLogKernelKernelConnectorTo(root);
        }
    }

    public static void AddStructuredLogKernelKernelConnectorTo(CompositeKernel kernel)
    {
            kernel.AddKernelConnector(new ReadBinlogFileCommand());

            KernelInvocationContext.Current?.Display(
                new HtmlString(@"<details><summary>Read and query MSBuild Structured Log files.</summary>
    <p>This extension adds support for reading MSbuild Structured Log files using the <code>#!connect msbuild %binlogPath%</code> magic command.</p>
    </details>"),
                "text/html");

        }
}