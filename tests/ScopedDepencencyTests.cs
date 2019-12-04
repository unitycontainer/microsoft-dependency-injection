﻿using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace Unity.Microsoft.DependencyInjection.Tests
{
    public class ScopedDepencencyTests
    {
        [Fact]
        public void ScopedDependencyFromTransientFactoryNotSharedAcrossScopes()
        {
            // Arrange
            var collection = new TestServiceCollection()
                                .AddTransient<ITransient>(CreateTransient)
                                .AddScoped<IScoped, Scoped>();

            var provider = collection.BuildServiceProvider();

            // Act
            //var rootTransient = provider.GetService<Transient>();
            
            ITransient transient1 = null;
            ITransient transient2a = null;
            ITransient transient2b = null;

            using (var scope1 = provider.CreateScope())
            {
                transient1 = scope1.ServiceProvider.GetService<ITransient>();
            }

            using (var scope2 = provider.CreateScope())
            {
                transient2a = scope2.ServiceProvider.GetService<ITransient>();
                transient2b = scope2.ServiceProvider.GetService<ITransient>();
            }

            // Assert
            Assert.NotSame(transient1, transient2a);
            Assert.NotSame(transient2a, transient2b);
            Assert.NotSame(transient1.ScopedDependency, transient2a.ScopedDependency);
            Assert.Same(transient2a.ScopedDependency, transient2b.ScopedDependency);
        }

        private ITransient CreateTransient(System.IServiceProvider provider)
        {
            return provider.GetRequiredService<Transient>();
        }

        public interface ITransient {
            IScoped ScopedDependency { get; }
        }

        public class Transient : ITransient
        {
            public Transient(IScoped scoped)
            {
                ScopedDependency = scoped;
            }

            public IScoped ScopedDependency { get; }
        }

        public interface IScoped { }

        public class Scoped : IScoped
        {
        }

    }

    internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}
