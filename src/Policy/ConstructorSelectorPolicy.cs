using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder;
using Unity.Builder.Selection;
using Unity.ObjectBuilder.BuildPlan.Selection;
using Unity.Policy;
using Unity.ResolverPolicy;

namespace Unity.Microsoft.DependencyInjection.Policy
{
    public class ConstructorSelectorPolicy : IConstructorSelectorPolicy
    {
        private readonly DefaultUnityConstructorSelectorPolicy _dependency = new DefaultUnityConstructorSelectorPolicy();
        
        /// <summary>
        /// Choose the constructor to call for the given type.
        /// </summary>
        /// <param name="context">Current build context</param>
        /// <param name="resolverPolicyDestination">
        /// The Microsoft.Practices.ObjectBuilder2.IPolicyList to add any generated resolver objects into.
        /// </param>
        /// <returns>The chosen constructor.</returns>
        public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList resolverPolicyDestination)
        {
            ConstructorInfo ctor = FindDependencyConstructor<DependencyAttribute>(context);
            if (ctor != null)
                return CreateSelectedConstructor(ctor,context);
            return _dependency.SelectConstructor(context, resolverPolicyDestination);
        }

        private ConstructorInfo FindDependencyConstructor<T>(IBuilderContext context)
        {
            Type typeOfAttribute = typeof(T);

            IEnumerable<ConstructorInfo> constructors = context.BuildKey.Type.GetTypeInfo()
                .DeclaredConstructors.Where(c => (!c.IsStatic) && c.IsPublic);

            var constructorInfos = constructors as ConstructorInfo[] ?? constructors.ToArray();
            ConstructorInfo[] injectionConstructors = constructorInfos
                .Where(ctor => ctor.IsDefined(typeOfAttribute, true))
                .ToArray();
            switch (injectionConstructors.Length)
            {
                case 0: return FindSingleConstructor(constructorInfos) ?? Other(constructorInfos.ToArray(), context);
                case 1: return injectionConstructors[0];
                default:
                    throw new InvalidOperationException(
               $"There are multiple constructors decorated with Inject attribute for the class {context.BuildKey.Type.GetTypeInfo().Name}");
            }
        }

        private static ConstructorInfo FindSingleConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            if (constructors.Count() == 1)
                return constructors.First();

            return null;
        }

        private SelectedConstructor CreateSelectedConstructor(ConstructorInfo ctor, IBuilderContext context)
        {
            var result = new SelectedConstructor(ctor);
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                result.AddParameterResolver(param.HasDefaultValue ? context.Container.CanResolve(param.ParameterType)? ResolveParameter(param): new LiteralValueDependencyResolverPolicy(null) : ResolveParameter(param));
            }
            return result;
        }

        private ConstructorInfo Other(ConstructorInfo[] constructors, IBuilderContext context)
        {
            Array.Sort(constructors, (a, b) =>
            {
                var qtd = b.GetParameters().Length.CompareTo(a.GetParameters().Length);
                if (qtd == 0)
                {
                    return b.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0));
                }
                return qtd;
            });

            ConstructorInfo bestConstructor = null;
            HashSet<Type> bestConstructorParameterTypes = null;
            for (var i = 0; i < constructors.Length; i++)
            {
                var parameters = constructors[i].GetParameters();

                var can = CanBuildUp(parameters, context);

                if (can)
                {
                    if (bestConstructor == null)
                    {
                        bestConstructor = constructors[i];
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>(
                                bestConstructor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestConstructorParameterTypes.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                        {
                            if (bestConstructorParameterTypes.All(p => p.GetTypeInfo().IsInterface)
                                && !parameters.All(p => p.ParameterType.GetTypeInfo().IsInterface))
                                return bestConstructor;

                            var msg = $"Failed to search for a constructor for {context.BuildKey.Type.FullName}{Environment.NewLine}There is an abnormality between the constructors";
                            throw new InvalidOperationException(msg);
                        }
                        else
                        {
                            return bestConstructor;
                        }
                    }
                }
            }

            if (bestConstructor == null)
            {
                //return null;
                throw new InvalidOperationException(
                    $"Constructor not found for {context.BuildKey.Type.FullName}");
            }
            else
            {
                return bestConstructor;
            }
        }

        private bool CanBuildUp(ParameterInfo[] parameters, IBuilderContext context)
        {
            return parameters.All(p => context.Container.CanResolve(p.ParameterType) || p.HasDefaultValue);
        }

        /// <summary>
        /// <para>
        /// Create a Policy to inject a parameter.
        /// </para>
        /// <lang name="pt-br">
        /// Cria uma política para injeção de um parâmetro.
        /// </lang>
        /// </summary>
        /// <param name="parameter">Parameter to be injeted.</param>
        /// <returns>The Resolver Policy.</returns>
        public IResolverPolicy ResolveParameter(ParameterInfo parameter)
        {
            var optional = parameter.GetCustomAttribute<OptionalDependencyAttribute>(false) != null;
            // parametros do construtor com attribute Dependency
            var attrs2 = parameter.GetCustomAttributes(false).OfType<DependencyResolutionAttribute>().ToList();
            if (attrs2.Count > 0)
            {
                return attrs2[0].CreateResolver(parameter.ParameterType);
            }

            // No attribute, just go back to the container for the default for that type.
            if (optional)
                return new OptionalDependencyResolverPolicy(parameter.ParameterType, null);
            else
                return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }
    }
}
