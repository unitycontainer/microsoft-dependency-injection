using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Tests
{
    public class IssuesOnGitHub
    {
        public interface IScopedService
        {
            IServiceProvider ServiceProvider { get; }
        }

        public class ScopedService : IScopedService
        {
            public ScopedService(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }
            public IServiceProvider ServiceProvider { get; }
        }

        private interface ITransientService
        {
            IScopedService ScopedService { get; }
        }

        public class TransientService : ITransientService
        {
            public TransientService(IScopedService scopedService)
            {
                ScopedService = scopedService;
            }
            public IScopedService ScopedService { get; }
        }

        [Fact]
        public void Issue_29_Type()
        {
            var sc = new ServiceCollection();

            sc.AddTransient<ITransientService, TransientService>();
            sc.AddScoped<IScopedService, ScopedService>();
            var sp = sc.BuildServiceProvider(new UnityContainer());

            Assert.Same(sp, sp.GetRequiredService<IScopedService>().ServiceProvider);
            Assert.Same(sp, sp.GetRequiredService<ITransientService>().ScopedService.ServiceProvider);
            var ssf = sp.GetRequiredService<IServiceScopeFactory>();
            using (var scope = ssf.CreateScope())
            {
                var scopedSp = scope.ServiceProvider;
                var scoped = scopedSp.GetRequiredService<IScopedService>();
                var transient = scopedSp.GetRequiredService<ITransientService>();

                Assert.Same(scopedSp, scoped.ServiceProvider);
                Assert.Same(scopedSp, transient.ScopedService.ServiceProvider);
            }
        }


        [Fact]
        public void Issue_29_Factory()
        {
            var sc = new ServiceCollection();

            sc.AddTransient<ITransientService>(o => new TransientService(o.GetRequiredService<IScopedService>()));
            sc.AddScoped<IScopedService, ScopedService>();
            var sp = sc.BuildServiceProvider(new UnityContainer());

            Assert.Same(sp, sp.GetRequiredService<IScopedService>().ServiceProvider);
            Assert.Same(sp, sp.GetRequiredService<ITransientService>().ScopedService.ServiceProvider);
            var ssf = sp.GetRequiredService<IServiceScopeFactory>();
            using (var scope = ssf.CreateScope())
            {
                var scopedSp = scope.ServiceProvider;
                var scoped = scopedSp.GetRequiredService<IScopedService>();
                var transient = scopedSp.GetRequiredService<ITransientService>();

                Assert.Same(scopedSp, scoped.ServiceProvider);
                Assert.Same(scopedSp, transient.ScopedService.ServiceProvider);
            }
        }


        [Fact]
        public void Issue_28()
        {

            var serviceCollection = new ServiceCollection();


            serviceCollection.AddHttpClient();

            IUnityContainer container = new UnityContainer().CreateChildContainer();

            var factory = new ServiceProviderFactory(container);

            var sp = factory.CreateServiceProvider(serviceCollection);
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var httpFactory0 = sp.GetRequiredService<IHttpClientFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                var sp1 = httpFactory.GetType().GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(httpFactory) as IServiceProvider;
                var c1 = sp1.GetType().GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sp1);
                Assert.NotNull(c1); // "In scope"

            }

            {
                var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
                var sp1 = httpFactory.GetType().GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(httpFactory) as IServiceProvider;
                var c1 = sp1.GetType().GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sp1);
                Assert.NotNull(c1); // "after scope"
            }

        }
    }
}
