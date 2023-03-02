using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Unit.Tests
{
    public class ScopedDepencencyTests
    {
        [Fact]
        public void aspnet_Extensions_issues_1301()
        {
            var services = new TestServiceCollection()
                .AddSingleton<Foo>();
            
            var provider = services.BuildServiceProvider();

            IServiceProvider scopedSp1 = null;
            IServiceProvider scopedSp2 = null;
            Foo foo1 = null;
            Foo foo2 = null;

            using (var scope1 = provider.CreateScope())
            {
                scopedSp1 = scope1.ServiceProvider;
                foo1 = scope1.ServiceProvider.GetRequiredService<Foo>();
            }

            using (var scope2 = provider.CreateScope())
            {
                scopedSp2 = scope2.ServiceProvider;
                foo2 = scope2.ServiceProvider.GetRequiredService<Foo>();
            }

            Assert.Equal(foo1.ServiceProvider, foo2.ServiceProvider);
            Assert.NotEqual(foo1.ServiceProvider, scopedSp1);
            Assert.NotEqual(foo2.ServiceProvider, scopedSp2);
        }

        [Fact]
        public void ScopedDependencyFromFactoryNotSharedAcrossScopes()
        {
            // Arrange
            var collection = new TestServiceCollection()
                                .AddTransient<ITransient>(CreateTransientFactory)
                                .AddScoped<IScoped, Scoped>();

            var provider = collection.BuildServiceProvider();

            // Act
            ITransient transient1a = null;
            ITransient transient1b = null;
            ITransient transient2b = null;

            using (var scope1 = provider.CreateScope())
            {
                transient1a = scope1.ServiceProvider.GetService<ITransient>();
            }

            using (var scope2 = provider.CreateScope())
            {
                transient1b = scope2.ServiceProvider.GetService<ITransient>();
                transient2b = scope2.ServiceProvider.GetService<ITransient>();
            }

            // Assert
            Assert.NotSame(transient1a, transient1b);
            Assert.NotSame(transient1b, transient2b);
            Assert.NotSame(transient1a.ScopedDependency, transient1b.ScopedDependency);
            Assert.Same(transient1b.ScopedDependency, transient2b.ScopedDependency);
        }

        [Fact]
        public void ScopedDependencyFromTransientNotSharedAcrossScopes()
        {
            // Arrange
            var collection = new TestServiceCollection()
                                .AddTransient<ITransient, Transient>()
                                .AddScoped<IScoped, Scoped>();

            var provider = collection.BuildServiceProvider();

            // Act
            ITransient transient1a = null;
            ITransient transient1b = null;
            ITransient transient2b = null;

            using (var scope1 = provider.CreateScope())
            {
                transient1a = scope1.ServiceProvider.GetService<ITransient>();
            }

            using (var scope2 = provider.CreateScope())
            {
                transient1b = scope2.ServiceProvider.GetService<ITransient>();
                transient2b = scope2.ServiceProvider.GetService<ITransient>();
            }

            // Assert
            Assert.NotSame(transient1a, transient1b);
            Assert.NotSame(transient1b, transient2b);
            Assert.NotSame(transient1a.ScopedDependency, transient1b.ScopedDependency);
            Assert.Same(transient1b.ScopedDependency, transient2b.ScopedDependency);
        }

        private ITransient CreateTransientFactory(IServiceProvider provider)
        {
            return provider.GetRequiredService<Transient>();
        }
    }

    public class Foo
    {
        public Foo(IServiceProvider sp)
        {
            ServiceProvider = sp;
        }

        public IServiceProvider ServiceProvider { get; }
    }

    public interface ITransient
    {
        IScoped ScopedDependency { get; }
    }

    public class Transient : ITransient
    {
        string ID { get; } = Guid.NewGuid().ToString();

        public Transient(IScoped scoped)
        {
            ScopedDependency = scoped;
        }

        public IScoped ScopedDependency { get; }
    }

    public interface IScoped { }

    public class Scoped : IScoped
    {
        string ID { get; } = Guid.NewGuid().ToString();
    }

    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}
