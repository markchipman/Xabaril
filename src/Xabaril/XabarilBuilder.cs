using Microsoft.Extensions.DependencyInjection;
using System;

namespace Xabaril
{
    internal sealed class XabarilBuilder
        : IXabarilBuilder
    {
        public IServiceCollection Services
        {
            get;
        }

        public XabarilBuilder(IServiceCollection serviceCollection)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }
    }
}
