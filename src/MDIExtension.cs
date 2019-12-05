using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Builder;
using Unity.Extension;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity.Microsoft.DependencyInjection
{
    internal class MdiExtension : UnityContainerExtension
    {
        #region Fields

        private static readonly MethodInfo _resolveMethod =
            typeof(UnityContainer).GetTypeInfo().GetDeclaredMethod(nameof(UnityContainer.ResolveArray));

        private static readonly MethodInfo _resolveGenericMethod =
            typeof(UnityContainer).GetTypeInfo().GetDeclaredMethod(nameof(UnityContainer.ResolveGenericArray));

        #endregion


        protected override void Initialize()
        {
            Context.Policies.Set(typeof(IEnumerable<>), UnityContainer.All, typeof(ResolveDelegateFactory), Factory);

        }

        public ILifetimeContainer Lifetime => Context.Lifetime;


        #region ResolveDelegateFactory

        public static ResolveDelegateFactory Factory = (ref BuilderContext context) =>
        {
#if NETSTANDARD1_0 || NETCOREAPP1_0 || NET40
            var typeArgument = context.Type.GetTypeInfo().GenericTypeArguments.First();
#else
            var typeArgument = context.Type.GenericTypeArguments.First();
#endif
            var type = ((UnityContainer)context.Container).GetFinalType(typeArgument);

            if (type != typeArgument)
            {
                var method = (ResolveArrayDelegate)_resolveGenericMethod
                    .MakeGenericMethod(typeArgument)
                    .CreateDelegate(typeof(ResolveArrayDelegate));
                return (ref BuilderContext c) => method(ref c, type);
            }
            else
            {
                return (ResolveDelegate<BuilderContext>)_resolveMethod
                    .MakeGenericMethod(typeArgument)
                    .CreateDelegate(typeof(ResolveDelegate<BuilderContext>));
            }
        };

        #endregion


        #region Implementation

        private static object Resolver<TElement>(ref BuilderContext context)
        {
            return ((UnityContainer)context.Container).ResolveEnumerable<TElement>(context.Resolve,
                                                                                   context.Name);
        }

        private static ResolveDelegate<BuilderContext> ResolverFactory<TElement>()
        {
            Type type = typeof(TElement).GetGenericTypeDefinition();
            return (ref BuilderContext c) => ((UnityContainer)c.Container).ResolveEnumerable<TElement>(c.Resolve, type, c.Name);
        }

        #endregion


        #region Nested Types

        private delegate object ResolveArrayDelegate(ref BuilderContext context, Type type);

        #endregion
    }
}
