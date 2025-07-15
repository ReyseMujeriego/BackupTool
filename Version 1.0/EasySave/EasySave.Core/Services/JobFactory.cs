using EasySave.Core.Models;

namespace EasySave.Core.Services;

public abstract class JobFactory
{
    public abstract Job CreateJob(string name, string source, string destination);
}

public class CompleteJobFactory : JobFactory
{
    public override Job CreateJob(string name, string source, string destination)
    {
        return new Job
        {
            Name = name,
            SourcePath = source,
            DestinationPath = destination,
            SaveType = SaveTypes.Complete
        };
    }
}

public class DifferentialJobFactory : JobFactory
{
    public override Job CreateJob(string name, string source, string destination)
    {
        return new Job
        {
            Name = name,
            SourcePath = source,
            DestinationPath = destination,
            SaveType = SaveTypes.Differential
        };
    }
}