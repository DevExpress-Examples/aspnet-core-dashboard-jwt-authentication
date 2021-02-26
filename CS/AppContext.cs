using Microsoft.AspNetCore.Http;

// https://www.quickdevnotes.com/better-approach-to-use-httpcontext-outside-a-controller-in-net-core-2-1/
namespace ASPNETCore30Dashboard {
    public static class AppContext {
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }
        public static HttpContext Current => _httpContextAccessor.HttpContext;
    }
}