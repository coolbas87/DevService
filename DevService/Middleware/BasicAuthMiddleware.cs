using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DevService.RefData;
using DevService.Models;
using Microsoft.Extensions.Logging;

namespace DevService.Middleware
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _realm;
        private readonly ILogger _logger;

        public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                // Get the encoded username and password
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                // Decode from Base64 to string
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];
                // Check if login is correct
                if (IsAuthorized(username, password))
                {
                    await _next.Invoke(context);
                    return;
                }
            }
            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            // Return unauthorized
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }

        // Make your own implementation of this
        public bool IsAuthorized(string username, string password)
        {
            _logger.LogInformation($"Спроба аутентифікації користувачем \"{username}\"");
            Users userObj = RefBook.Users.SingleOrDefault(usr => usr.uLogin == username);

            if (userObj == null)
            {
                throw new UnauthorizedAccessException($"Користувач \"{username}\" не знайдений");
            }
            else if (!password.Equals(userObj.uPass, StringComparison.InvariantCulture))
            {
                throw new UnauthorizedAccessException($"Невірний пароль для користувача \"{username}\"");
            }
            else
            {
                _logger.LogInformation($"Користувач \"{username}\" пройшов аутентифікацію");
                return true;
            }
        }
    }
}
