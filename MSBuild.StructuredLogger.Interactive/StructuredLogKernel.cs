namespace MSBuild.StructuredLogger.Interactive;

using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Utility;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.Build.Logging.StructuredLogger;
using System.Collections.Generic;
using Task = Task;
using Microsoft.DotNet.Interactive.ValueSharing;

public class StructuredLogKernel : Kernel, IKernelCommandHandler<RequestValue>, IKernelCommandHandler<RequestValueInfos>
{
    private readonly Dictionary<string, string[]> DoubleWrites;
    private readonly Dictionary<string, string> LongestTasks;
    private readonly Dictionary<string, string> Environment;
    // private IReadOnlyList<(Project, ProjectEvaluation)> Projects;

    public StructuredLogKernel(string name, FileInfo binlogFile) : base(name)
    {
        var build = Serialization.Read(binlogFile.FullName);
        BuildAnalyzer.AnalyzeBuild(build);
        DoubleWrites = 
            build.FindChild<Folder>("DoubleWrites")?
                .FindImmediateChildrenOfType<Item>()
                .ToDictionary(b => b.Text, b => b.FindImmediateChildrenOfType<Item>().Select(item => item.Text).ToArray())
            ?? new();

        LongestTasks = 
            build.FindImmediateChildrenOfType<Folder>().First(
                f => f.Name.EndsWith("most expensive tasks")
            )
            .FindImmediateChildrenOfType<Item>()
            .ToDictionary(i => i.Name, i => i.Text);
            
        Environment = build.EnvironmentFolder.FindImmediateChildrenOfType<Property>().ToDictionary(p => p.Name, p => p.Value);
        // Projects = build.FindChildrenRecursive<Project>().Select(p => (p, build.FindEvaluation(p.EvaluationId))).ToArray();
    }

    Task IKernelCommandHandler<RequestValue>.HandleAsync(RequestValue command, KernelInvocationContext context)
    {
        if (command.Name == "BuildEnvironment")
        {
            context.PublishValueProduced(command, Environment);
        } else if (command.Name == "DoubleWrites")
        {
            context.PublishValueProduced(command, DoubleWrites);
        }
        else if (command.Name == "LongestTasks")
        {
            context.PublishValueProduced(command, LongestTasks);
        }
        // else if (command.Name == "Projects")
        // { 
        //     context.PublishValueProduced(command, Projects); 
        // }
        else 
        { 
            context.Fail(command, message: $"Unknown value '{command.Name}'"); 
        }

        return Task.CompletedTask;
    }

    Task IKernelCommandHandler<RequestValueInfos>.HandleAsync(RequestValueInfos command, KernelInvocationContext context)
    {
        var values = new KernelValueInfo[] {
            new KernelValueInfo("BuildEnvironment", FormattedValue.CreateSingleFromObject(Environment, "application/json")),
            new KernelValueInfo("DoubleWrites", FormattedValue.CreateSingleFromObject(DoubleWrites, "application/json")),
            new KernelValueInfo("LongestTasks", FormattedValue.CreateSingleFromObject(LongestTasks, "application/json")),
            // new KernelValueInfo("Projects", FormattedValue.FromObject(Projects, "application/json")[0]),
        };
        context.Publish(new ValueInfosProduced(values, command));
        return Task.CompletedTask;
    }
}