using AuthService.Models;
using Azure.Core;

namespace AuthService.Services
{

    public class GenerateEmail
    {
        public EmailRequestModel GenerateVerificationEmail(string email)
        {
            var verificationEmail = new EmailRequestModel
            {
                To = email,
                Subject = "Your verification code from Ventixe",
                HtmlBody = @$"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Email Verification</title>
                    </head>
                    <body style=""margin:0; padding:0; background-color:#ffffff; font-family: Inter, Arial, sans-serif;"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; padding:2rem;"">
                            <tr>
                                <td>
                                    <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ededed; padding:1rem; margin:1rem auto;"">
                                        <tr>
                                            <td>
                                                <h2 style=""color:#F26CF9; margin:0 0 1rem;"">Hello  - Welcome to Ventixe</h2>
                                                <p style=""color:#434345; margin:0;"">Before you can start managing events, we need to verify your email address.</p>
                                            </td>
                                        </tr>
                                    </table>

                                    <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ededed; padding:1rem; margin:1rem auto;"">
                                        <tr>
                                            <td>
                                                <p style=""color:#434345; margin:0;"">
                                                    Follow this <a href=""https://jolly-ocean-090980503.6.azurestaticapps.net/verify?email={email}"" style=""color:#5562A2;"">link</a> and enter the code 
                                                    <span style=""color:#5562A2; font-size:24px; font-weight:bold;""></span> to verify your email address.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>",
                PlainText = "Hello"
            };
            return verificationEmail;
        }
    }
}
