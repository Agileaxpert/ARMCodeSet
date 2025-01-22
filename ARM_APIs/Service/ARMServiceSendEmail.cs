using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.Storage;
using ARM_APIs.Interface;

public class ARMServiceSendEmail : ISendEmailService
{
    public async Task<string> SendEmailJob(string senderEmail, string username, string JobId, int interval, bool DeletePendingJobs)
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            string smtpServer = configuration["EmailConfiguration:SmtpServer"];
            int port = int.Parse(configuration["EmailConfiguration:Port"]);
            string senderAddress = configuration["EmailConfiguration:Username"];
            string password = configuration["EmailConfiguration:Password"];
            string subject = "test message";
            string body = $"Hi {username}, this is a sample test mail";

            using (SmtpClient smtpClient = new SmtpClient(smtpServer, port))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderAddress, password);

                using (MailMessage mailMessage = new MailMessage(senderAddress, senderEmail))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            DateTime nextExecutionTime = DateTime.Now.AddSeconds(interval);
            var jobId = BackgroundJob.Schedule(() => SendEmailJob(senderEmail, username, JobId, interval, DeletePendingJobs), nextExecutionTime);
            Console.WriteLine($"Job scheduled successfully: {jobId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message);
            throw;
        }
        return "";
    }

    public async Task ScheduleSendEmailJob(string senderEmail, string username, string JobId, int interval, bool DeletePendingJobs)
    {
        await SendEmailJob(senderEmail, username, JobId, interval, DeletePendingJobs);
    }

    public async Task<string> DeleteExistingJobs(string jobId)
    {
        var monitor = JobStorage.Current.GetMonitoringApi();
        int deletedJobsCount = 0;

        var jobsProcessing = monitor.ScheduledJobs(0, int.MaxValue)
            .Where(x => x.Value.Job.Method.Name == nameof(ScheduleSendEmailJob) && x.Value.Job.Args.Count > 3 && x.Value.Job.Args[2]?.ToString() == jobId);

        foreach (var job in jobsProcessing)
        {
            BackgroundJob.Delete(job.Key);
            deletedJobsCount++;
        }



        var scheduledJobs = monitor.ScheduledJobs(0, int.MaxValue);
        var jobs = scheduledJobs.Where(o => o.Value.Job.Method.Name == "ScheduleSendEmailJob").ToList();
        if (jobs is not null)
        {
            jobs.ForEach(x => BackgroundJob.Delete(x.Key));
        }
        //var jobsScheduled = monitor.ScheduledJobs(0, int.MaxValue)
        //    .Where(x => x.Value.Job.Method.Name == nameof(ScheduleSendEmailJob) && x.Value.Job.Args.Count > 3 && x.Value.Job.Args[2]?.ToString() == jobId);

        //foreach (var job in jobsScheduled)
        //{
        //    BackgroundJob.Delete(job.Key);
        //    deletedJobsCount++;
        //}

        return $"Deleted {deletedJobsCount} job(s) with JobId: {jobId}";
    }
}
