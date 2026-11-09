using System.Net;

namespace RaceDay.Web.Services
{
    public class CookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Get the cookie from the browser and attach it to the API request
            var cookie = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }

            // 2. Send the request to the API
            var response = await base.SendAsync(request, cancellationToken);

            // 3. If the API sends back a Set-Cookie, save it properly to the browser
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var setCookie in setCookieValues)
                {
                    // Split the cookie string by ';' to get the name, value, and attributes
                    var cookieParts = setCookie.Split(';');
                    var nameValue = cookieParts[0].Split('=');

                    if (nameValue.Length == 2)
                    {
                        var cookieName = nameValue[0].Trim();
                        var cookieValue = nameValue[1].Trim();

                        // Ignore ASP.NET's anti-forgery or other non-session cookies if needed
                        if (cookieName == ".AspNetCore.Session" || cookieName == ".RaceDay.Session" || cookieName.StartsWith(".AspNetCore"))
                        {
                            _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, cookieValue, new CookieOptions
                            {
                                HttpOnly = true, // Prevents JavaScript access
                                Secure = true,   // Required for HTTPS
                                SameSite = SameSiteMode.Lax, // CRITICAL: Allows the cookie to work across the MVC and API ports locally
                                Path = "/"
                            });
                        }
                    }
                }
            }

            return response;
        }
    }
}