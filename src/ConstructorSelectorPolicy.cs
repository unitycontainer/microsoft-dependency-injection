using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Policy;
using Unity.ObjectBuilder.BuildPlan.Selection;
using Unity.Builder;
using Unity.Builder.Selection;
using Unity.Attributes;
using Unity.ResolverPolicy;

namespace Unity.Microsoft.DependencyInjection
{
    public class ConstructorSelectorPolicy : IConstructorSelectorPolicy
    {
        DefaultUnityConstructorSelectorPolicy dependency = new DefaultUnityConstructorSelectorPolicy();
        
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
                return CreateSelectedConstructor(ctor);
            return dependency.SelectConstructor(context, resolverPolicyDestination);
        }

        private ConstructorInfo FindDependencyConstructor<T>(IBuilderContext context)
        {
            Type typeOfAttribute = typeof(T);

            IEnumerable<ConstructorInfo> constructors = context.BuildKey.Type.GetTypeInfo()
                .DeclaredConstructors.Where(c => (!c.IsStatic) && c.IsPublic);

            ConstructorInfo[] injectionConstructors = constructors
                .Where(ctor => ctor.IsDefined(typeOfAttribute, true))
                .ToArray();
            switch (injectionConstructors.Length)
            {
                case 0: return FindSingleConstructor(constructors) ?? Other(constructors.ToArray(), context);
                case 1: return injectionConstructors[0];
                default:
                    throw new InvalidOperationException(
               $"Existem multiplos construtores decorados com Inject para a classe " +
               $"{context.BuildKey.Type.GetTypeInfo().Name}");
            }
        }

        private static ConstructorInfo FindSingleConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            if (constructors.Count() == 1)
                return constructors.First();
            return null;
        }

        private SelectedConstructor CreateSelectedConstructor(ConstructorInfo ctor)
        {
            var result = new SelectedConstructor(ctor);
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                result.AddParameterResolver(ResolveParameter(param));
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
                    return b.GetParameters().Sum(p => p.ParameterType.IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.IsInterface ? 1 : 0));
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
                            if (bestConstructorParameterTypes.All(p => p.IsInterface)
                                && !parameters.All(p => p.ParameterType.IsInterface))
                                return bestConstructor;

                            var msg = $"Falha ao procurar um construtor para {context.BuildKey.Type.FullName}\n" +
                                $"Há uma abiquidade entre os construtores";
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
                    $"Construtor não encontrado para {context.BuildKey.Type.FullName}");
            }
            else
            {
                return bestConstructor;
            }
        }

        private bool CanBuildUp(ParameterInfo[] parameters, IBuilderContext context)
        {
            return parameters.All(p => context.Container.CanResolve(p.ParameterType));
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
