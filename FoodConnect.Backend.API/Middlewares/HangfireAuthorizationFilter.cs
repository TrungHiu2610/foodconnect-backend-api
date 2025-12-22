using Hangfire.Dashboard;

namespace FoodConnect.Backend.API.Middlewares
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.Request.Host.Host == "localhost" || 
                   httpContext.Request.Host.Host == "127.0.0.1";
        }
    }
}
