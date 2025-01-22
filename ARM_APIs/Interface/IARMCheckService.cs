namespace ARM_APIs.Interface
{
    public interface IARMCheckService
    {
        Task ProcessLogsAndSendEmails();
        Task CheckServiceLogsAndSendEmails();
    }
}
