namespace JobScheduler.Interfaces;

public interface ITimeTriggeredJob
{
    string Name { get; }
    string NextOccurrence { get; set; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
