namespace MSBuild.StructuredLogger.Interactive;

using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.Build.Logging.StructuredLogger;
using System.Collections.Generic;
using Task = Task;

public class StructuredLogKernel : Kernel, IKernelCommandHandler<RequestValue>
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
}