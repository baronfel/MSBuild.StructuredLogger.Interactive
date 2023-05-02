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
    private IReadOnlyList<(string key, string value)> Environment;
    private IReadOnlyList<(Project, ProjectEvaluation)> Projects;

    public StructuredLogKernel(string name, FileInfo binlogFile) : base(name)
    {
        var build = Serialization.Read(binlogFile.FullName);
        Environment = build.EnvironmentFolder.FindImmediateChildrenOfType<Property>().Select(p => (p.Name, p.Value)).ToArray();
        Projects = build.FindChildrenRecursive<Project>().Select(p => (p, build.FindEvaluation(p.EvaluationId))).ToArray();
    }

    Task IKernelCommandHandler<RequestValue>.HandleAsync(RequestValue command, KernelInvocationContext context)
    {
        if (command.Name == "Environment")
        {
            context.PublishValueProduced(command, Environment);
        }
        else if (command.Name == "Projects")
        { 
            context.PublishValueProduced(command, Projects); 
        }
        else 
        { 
            context.Fail(command, message: $"Unknown value '{command.Name}'"); 
        }

        return Task.CompletedTask;
    }

    Task IKernelCommandHandler<RequestValueInfos>.HandleAsync(RequestValueInfos command, KernelInvocationContext context)
    {
        var values = new KernelValueInfo[] {
            new KernelValueInfo("Environment", FormattedValue.FromObject(Environment, "application/json")[0]),
            new KernelValueInfo("Projects", FormattedValue.FromObject(Projects, "application/json")[0]),
         };
        context.Publish(new ValueInfosProduced(values, command));
        return Task.CompletedTask;
    }
}