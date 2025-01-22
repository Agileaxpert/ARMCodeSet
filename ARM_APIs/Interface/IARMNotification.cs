using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;

namespace ARM_APIs.Interface
{
    public interface IARMNotificationService
    {
      abstract Task<object> ProcessARMMobileNotification(MobileNotification notification);
       abstract Task<bool> DisableMobileNotificationForUserAsync(string ARMSessionId);
      abstract Task<IActionResult> SendEmailNotification(ARMNotify model);
    }
}
