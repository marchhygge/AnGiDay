using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string toEmail, string subject, string body);
    }
}
