using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using PixelwareSite.Models;
using PixelwareSite.Extensions;

namespace PixelwareSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(ContatoModel model)
        {
            try
            {
                await DoSendMail(model.Nome, model.Email, model.OndeConheceu, model.Mensagem, model.Telefone);
                return Ok(new { Success = true });
            }
            catch(Exception ex)
            {
                return BadRequest(new { Success = false, Message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message) });
            }
        }

        private async Task DoSendMail(string name, string email, string ondeConheceu, string body, string telefone)
        {

            string SmtpServer = _configuration.GetValue<string>("Smtp:Host");
            string SmtpPort = _configuration.GetValue<string>("Smtp:Port");
            string SmtpUsername = _configuration.GetValue<string>("Smtp:Username");
            string SmtpPasswd = _configuration.GetValue<string>("Smtp:Password");


            var t = _configuration.GetSection("MailTo").GetChildren().Select(x => new { Name = x.GetValue<string>("Name"), Email = x.GetValue<string>("Email") });


            var From = _configuration.GetListValue<To>("MailFrom");
            var To = _configuration.GetListValue<To>("MailTo");
            var Cc = _configuration.GetListValue<Cc>("MailCc");
            var Bcc = _configuration.GetListValue<Bcc>("MailBcc");

            // Mime message
            var message = new MimeMessage();

            foreach (var from in From)
            {
                message.From.Add(new MailboxAddress(from.Name, from.Email));
            }

            foreach (var to in To)
            {
                message.To.Add(new MailboxAddress(to.Name, to.Email));
            }

            foreach (var cc in Cc)
            {
                message.Cc.Add(new MailboxAddress(cc.Name, cc.Email));
            }

            foreach (var bcc in Bcc)
            {
                message.Bcc.Add(new MailboxAddress(bcc.Name, bcc.Email));
            }

            message.Subject = "Pixelware - Contato através do Site";
            message.Body = new TextPart("html")
            {
                Text = $"Nome: {name}<br/>E-mail: {email}<br/>Telefone: {telefone}<br/>Onde conheceu: {ondeConheceu}<br/>Mensagem: {body}"
            };

            // Dispara o e-mail.
            using (var client = new SmtpClient())
            {

                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) =>  true;

                await client.ConnectAsync(SmtpServer, Convert.ToInt32(SmtpPort), MailKit.Security.SecureSocketOptions.Auto);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(SmtpUsername, SmtpPasswd);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);

            }

        }

    }
}
