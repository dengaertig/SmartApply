using SmartApply.Models;

namespace SmartApply.Services.Crawling;

public interface IJobSource
{
    Task<List<JobPosting>> SearchJobsAsync(string keyword, string location, bool internshipsOnly);
}