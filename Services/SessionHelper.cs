namespace Raceday.API.Services
{
    public static class SessionHelper
    {
        // Returns the UserID from session, or null if not logged in
        public static int? GetUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("UserID");
        }

        // Returns the Role from session, or null if not logged in
        public static string? GetRole(HttpContext httpContext)
        {
            return httpContext.Session.GetString("Role");
        }

        // Returns true if logged in and role matches
        public static bool IsInRole(HttpContext httpContext, string role)
        {
            var userRole = GetRole(httpContext);
            return userRole != null && userRole == role;
        }
    }
}