using Microsoft.Extensions.Localization;

namespace CSharp.Net.Core.MVC.Northwind
{
    public class SharedResource
    {
        private readonly IStringLocalizer _localizer;

        public SharedResource(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }
    }
}
