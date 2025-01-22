using ARM_APIs.Interface;
using ARMCommon.Helpers;
using ARMCommon.Model;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ARMCommon.Services
{
    public class ARMCheckService : IARMCheckService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public ARMCheckService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task ProcessLogsAndSendEmails()
        {
            try
            {
                var logsToProcess = await _context.ARMServiceLogs
                    .Where(log => log.Status == "Started")
                    .ToListAsync();

                foreach (var log in logsToProcess)
                {
                    await SendEmail(log.ServiceName, log.Server, log.StartOnTime, log.Folder);
                }

                ScheduleNextCheck();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessLogsAndSendEmails: {ex.Message}");
                throw;
            }
        }

        public async Task CheckServiceLogsAndSendEmails()
        {
            try
            {
                var currentTime = DateTime.Now;

                var logs = await _context.ARMServiceLogs.ToListAsync();

                var logsToUpdate = new List<ARMServiceLogs>();
                var outdatedLogs = new List<ARMServiceLogs>();

                foreach (var log in logs)
                {
                    if (log.LastOnline.HasValue && log.IsMailSent == null)
                    {
                        var lastOnlineUtc = log.LastOnline.Value;
                        if ((currentTime - lastOnlineUtc).TotalMinutes <= 2)
                        {
                            if (!log.IsMailSent.HasValue)
                            {
                                log.IsMailSent = true;
                                logsToUpdate.Add(log);
                            }
                        }
                        else
                        {
                            log.Status = "Stopped";
                            log.IsMailSent = false;
                            outdatedLogs.Add(log);
                            Console.WriteLine($"Service {log.ServiceName} status updated to 'Stopped'.");
                        }
                    }
                }

                if (logsToUpdate.Any())
                {
                    _context.ARMServiceLogs.UpdateRange(logsToUpdate);
                }
                if (outdatedLogs.Any())
                {
                    _context.ARMServiceLogs.UpdateRange(outdatedLogs);
                }

                if (logsToUpdate.Any() || outdatedLogs.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckServiceLogsAndSendEmails: {ex.Message}");
            }
        }





        private async Task SendEmail(string serviceName, string server, DateTime? startOnTime, string folder)
        {
            var smtpConfig = _configuration.GetSection("EmailConfiguration");
            var smtpClient = new SmtpClient(smtpConfig["SmtpServer"], int.Parse(smtpConfig["Port"]))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"])
            };

            var mailMessage = new MailMessage(smtpConfig["Username"], smtpConfig["notifyto"])
            {
                Subject = $"Service Details for {serviceName}",
                Body = $"Hi,\n\nThe following service is started successfully.\n ServiceName: {serviceName} \n Server: {server}\n Folder: {folder}\n StartTime: {startOnTime} .\n\nRegards,\n {serviceName}",
                IsBodyHtml = false
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        private void ScheduleNextCheck()
        {
            var jobId = BackgroundJob.Schedule(() => CheckServiceLogsAndSendEmails(), TimeSpan.FromMinutes(2));
            Console.WriteLine($"JobId {jobId} scheduled for: {DateTime.Now.AddMinutes(2):MM/dd/yyyy - hh:mm tt}");
        }
    }
}
