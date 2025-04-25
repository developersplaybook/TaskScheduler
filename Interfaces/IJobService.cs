using JobScheduler.Models;
using System.Threading.Tasks;

namespace JobScheduler.Interfaces
{
    public interface IJobService
    {
        Task TriggerJobToggle(JobNames jobType);
    }
}
