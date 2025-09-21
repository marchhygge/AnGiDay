using AGD.Repositories.ConfigurationModels;
using AGD.Service.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Implement
{
    public class EmailService : IEmailService
    {    
        private readonly SmtpSettings _smtpSetting;

        public EmailService(IOptionsMonitor<SmtpSettings> optionsMonitor)
        {
            _smtpSetting = optionsMonitor.CurrentValue;
        }

        public async Task SendMailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_smtpSetting.Host)
            {
                Port = _smtpSetting.Port,
                Credentials = new NetworkCredential(_smtpSetting.UserName, _smtpSetting.Password),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSetting.UserName, _smtpSetting.SenderName),
                Subject = subject,
                Body = Body(subject, "Mr/Ms", body, "AnGiDay", ""),
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string Body(string subject, string name, string content, string senderName, string buttonUrl)
        {
            string body = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{subject}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #fafafa;
            color: #333333;
        }}
        .container {{
            max-width: 650px;
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 6px 15px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #E7479E, #BB3DC6, #9534EA);
            padding: 35px;
            text-align: center;
            color: white;
        }}
        .header h1 {{
            margin: 0;
            font-size: 26px;
            font-weight: bold;
            letter-spacing: 1px;
        }}
        .body {{
            padding: 30px 40px;
            font-size: 16px;
            line-height: 1.7;
        }}
        .body p {{
            margin: 15px 0;
        }}
        .cta {{
            display: inline-block;
            background: linear-gradient(135deg, #E7479E, #BB3DC6, #9534EA);
            color: white !important;
            text-decoration: none;
            padding: 12px 25px;
            border-radius: 6px;
            font-weight: bold;
            margin-top: 20px;
            transition: opacity 0.3s;
        }}
        .cta:hover {{
            opacity: 0.9;
        }}
        .footer {{
            background-color: #f3f3f3;
            padding: 25px;
            text-align: center;
            font-size: 14px;
            color: #777777;
        }}
        .footer a {{
            color: #BB3DC6;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <!-- Header -->
        <div class=""header"">
            <h1>AnGiDay</h1>
            <p>{subject}</p>
        </div>

        <!-- Body -->
        <div class=""body"">
            <p>Xin chào <b>{name}</b>,</p>
            <p>{content}</p>
            <p>Cảm ơn bạn đã đồng hành cùng <b>AnGiDay</b>. Chúc bạn có trải nghiệm tuyệt vời!</p>
            <div style=""text-align: center;"">
                <a href=""{buttonUrl}"" target=""_blank"" class=""cta"">Khám phá ngay</a>
            </div>
        </div>

        <!-- Footer -->
        <div class=""footer"">
            <p>&copy; 2024 AnGiDay | Designed with AnGiDay | {senderName}</p>
            <p>
                <a href=""#"">Trang chủ</a> | 
                <a href=""#"">Liên hệ</a>
            </p>
        </div>
    </div>
</body>
</html>";
            return body;
        }
    }
}
