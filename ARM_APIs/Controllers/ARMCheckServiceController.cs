//using ARMCommon.Helpers;
//using ARMCommon.Model;
//using Hangfire;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Net;
//using System.Net.Mail;


//namespace ARM_APIs.Controllers
//{
//    [Route("api/v{version:apiVersion}")]
//    [ApiVersion("1")]
//    [ApiController]
//    public class ARMCheckServiceController : ControllerBase
//    {
//        private readonly DataContext _context;
//        private readonly IConfiguration _configuration;

//        public ARMCheckServiceController(DataContext context, IConfiguration configuration)
//        {
//            _context = context;
//            _configuration = configuration;
//        }

//        [HttpGet("ARMcheckService")]
//        public async Task<ActionResult> ARMcheckService()
//        {
//            try
//            {
//                await ProcessLogsAndSendEmails();
//                return Ok("Process completed.");
//            }
//            catch (Exception ex)
//            {
//                return BadRequest("No jobs running");
//            }
//        }

//        public async Task ProcessLogsAndSendEmails()
//        {
//            try
//            {
//                var logs = await _context.ARMServiceLogs.ToListAsync();
//                var currentTime = DateTime.Now;

//                foreach (var log in logs)
//                {
//                    bool shouldSendEmail = false;
//                    if (log.Status == "Started" || log.Status == "Stopped")
//                    {
//                        shouldSendEmail = true;
//                    }
//                    if (shouldSendEmail)
//                    {
//                        await SendEmail(log.ServiceName, log.Server, log.StartOnTime, log.Folder);
//                    }
//                    CheckServiceLogsAndSendEmails();
//                }
//                await _context.SaveChangesAsync();
//                //CheckServiceLogsAndSendEmails();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                throw;
//            }
//        }

//        private async Task SendEmail(string serviceName, string server, DateTime? startOnTime, string folder)
//        {
//            string smtpServer = _configuration["EmailConfiguration:SmtpServer"];
//            int port = int.Parse(_configuration["EmailConfiguration:Port"]);
//            string senderAddress = _configuration["EmailConfiguration:Username"];
//            string password = _configuration["EmailConfiguration:Password"];
//            string senderEmail = _configuration["EmailConfiguration:notifyto"];

//            string subject = "Service Details for " + serviceName;
//            string body = $"Hi,\n\nThe following service is started successfully.\n ServiceName: {serviceName} \n Server: {server}\n Folder: {folder}\n StartTime: {startOnTime} .\n\nRegards,\n {serviceName}";

//            using (SmtpClient smtpClient = new SmtpClient(smtpServer, port))
//            {
//                smtpClient.EnableSsl = true;
//                smtpClient.UseDefaultCredentials = false;
//                smtpClient.Credentials = new NetworkCredential(senderAddress, password);

//                using (MailMessage mailMessage = new MailMessage(senderAddress, senderEmail))
//                {
//                    mailMessage.Subject = subject;
//                    mailMessage.Body = body;
//                    mailMessage.IsBodyHtml = false;
//                    await smtpClient.SendMailAsync(mailMessage);
//                }
//            }
//        }


//        public async Task CheckServiceLogsAndSendEmails()
//        {
//            var currentTime = DateTime.Now;

//            try
//            {
//                var checklogs = await _context.ARMServiceLogs.ToListAsync();

//                foreach (var log in checklogs)
//                {
//                    if (log.LastOnline.HasValue)
//                    {
//                        var lastOnlineTimeSpan = currentTime - log.LastOnline.Value;
//                        string mailSentStatus = log.ismailsent ?? "";

//                        if (lastOnlineTimeSpan.TotalMinutes <= 2 && mailSentStatus == "")
//                        {
//                            log.ismailsent = "true";
//                        }
//                        else if (lastOnlineTimeSpan.TotalMinutes > 2)
//                        {
//                            Console.WriteLine($"Service {log.ServiceName} has not been online for more than 2 minutes.");
//                        }
//                        await _context.SaveChangesAsync();
//                    }
//                }

//                 BackgroundJob.Schedule(() => CheckServiceLogsAndSendEmails(), TimeSpan.FromMinutes(2));
//                //var jobid = BackgroundJob.Reschedule(() => CheckServiceLogsAndSendEmails(), TimeSpan.FromMinutes(2));
//                Console.WriteLine($" Jobid  Next scheduled for: {DateTime.Now.AddMinutes(2):MM/dd/yyyy - hh:mm tt}");

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                throw;
//            }
//        }

//    }
//}





using ARMCommon.Helpers;
using ARMCommon.Model;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    public class ARMCheckServiceController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public ARMCheckServiceController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("ARMcheckService")]
        public async Task<ActionResult> ARMcheckService()
        {
            try
            {
                // Start the background job manually for immediate processing
                BackgroundJob.Enqueue(() => CheckServiceLogsAndSendEmails());
                return Ok("Process initiated.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public async Task ProcessLogsAndSendEmails()
        {
            try
            {
                var logs = await _context.ARMServiceLogs.ToListAsync();
                var currentTime = DateTime.Now;

                foreach (var log in logs)
                {
                    if (log.Status == "Started" || log.Status == "Stopped")
                    {
                        await SendEmail(log.ServiceName, log.Server, log.StartOnTime, log.Folder);
                    }
                }
                // Save changes if necessary
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task SendEmail(string serviceName, string server, DateTime? startOnTime, string folder)
        {
            string smtpServer = _configuration["EmailConfiguration:SmtpServer"];
            int port = int.Parse(_configuration["EmailConfiguration:Port"]);
            string senderAddress = _configuration["EmailConfiguration:Username"];
            string password = _configuration["EmailConfiguration:Password"];
            string senderEmail = _configuration["EmailConfiguration:notifyto"];

            string subject = "Service Details for " + serviceName;
            string body = $"Hi,\n\nThe following service is started successfully.\n ServiceName: {serviceName} \n Server: {server}\n Folder: {folder}\n StartTime: {startOnTime} .\n\nRegards,\n {serviceName}";

            using (SmtpClient smtpClient = new SmtpClient(smtpServer, port))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderAddress, password);

                using (MailMessage mailMessage = new MailMessage(senderAddress, senderEmail))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = false;
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }

        public async Task CheckServiceLogsAndSendEmails()
        {
            var currentTime = DateTime.Now;

            try
            {
                // Fetch logs that need processing
                var logs = await _context.ARMServiceLogs
                    .Where(log => log.LastOnline.HasValue)
                    .ToListAsync();

                foreach (var log in logs)
                {
                    var lastOnlineTimeSpan = currentTime - log.LastOnline.Value;
                    var isMailSent = log.IsMailSent ?? false;

                    if (lastOnlineTimeSpan.TotalMinutes <= 2 && !isMailSent)
                    {
                        log.IsMailSent = true;
                        Console.WriteLine($"Email sent for service: {log.ServiceName}");
                    }
                    else if (lastOnlineTimeSpan.TotalMinutes > 2)
                    {
                        Console.WriteLine($"Service {log.ServiceName} has not been online for more than 2 minutes.");
                    }
                }

                // Save changes after processing all logs
                await _context.SaveChangesAsync();

                // Ensure the recurring job is registered once, e.g., in application startup
                RecurringJob.AddOrUpdate(() => CheckServiceLogsAndSendEmails(), Cron.MinuteInterval(2));
                Console.WriteLine($"Job scheduled for: {DateTime.Now.AddMinutes(2):MM/dd/yyyy - hh:mm tt}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckServiceLogsAndSendEmails: {ex.Message}");
                throw; // Rethrow to ensure proper error handling at higher levels
            }
        }


    }
}

