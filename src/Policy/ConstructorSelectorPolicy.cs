using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build;
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
        /// <returns>The chosen constructor.</returns>
        public object SelectConstructor<TContext>(ref TContext context)
            where TContext : IBuildContext
        {
            ConstructorInfo ctor = FindDependencyConstructor<TContext, DependencyAttribute>(ref context);
            if (ctor != null)
                return CreateSelectedConstructor(ctor, ref context);
            return _dependency.SelectConstructor(ref context);
        }

        private ConstructorInfo FindDependencyConstructor<TContext, T>(ref TContext context)
            where TContext : IBuildContext
        {
            Type typeOfAttribute = typeof(T);

            IEnumerable<ConstructorInfo> constructors = context.Type.GetTypeInfo()
                .DeclaredConstructors.Where(c => (!c.IsStatic) && c.IsPublic);

            var constructorInfos = constructors as ConstructorInfo[] ?? constructors.ToArray();
            ConstructorInfo[] injectionConstructors = constructorInfos
                .Where(ctor => ctor.IsDefined(typeOfAttribute, true))
                .ToArray();
            switch (injectionConstructors.Length)
            {
                case 0: return FindSingleConstructor(constructorInfos) ?? Other(constructorInfos.ToArray(), ref context);
                case 1: return injectionConstructors[0];
                default:
                    throw new InvalidOperationException(
               $"Existem multiplos construtores decorados com Inject para a classe " +
               $"{context.Type.GetTypeInfo().Name}");
            }
        }

        private static ConstructorInfo FindSingleConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            if (constructors.Count() == 1)
                return constructors.First();

            return null;
        }

        private SelectedConstructor CreateSelectedConstructor<TContext>(ConstructorInfo ctor, ref TContext context)
            where TContext : IBuildContext
        {
            var result = new SelectedConstructor(ctor);
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                result.AddParameterResolver(param.HasDefaultValue ? context.Container.CanResolve(param.ParameterType)? ResolveParameter(param): new LiteralValueDependencyResolverPolicy(null) : ResolveParameter(param));
            }
            return result;
        }

        private ConstructorInfo Other<TContext>(ConstructorInfo[] constructors, ref TContext context)
            where TContext : IBuildContext
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

                var can = CanBuildUp(parameters, ref context);

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

                            var msg = $"Falha ao procurar um construtor para {context.Type.FullName}\n" +
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
                    $"Construtor não encontrado para {context.Type.FullName}");
            }
            else
            {
                return bestConstructor;
            }
        }

        private bool CanBuildUp<TContext>(ParameterInfo[] parameters, ref TContext context)
            where TContext : IBuildContext
        {
            var container = context.Container;
            return parameters.All(p => container.CanResolve(p.ParameterType) || p.HasDefaultValue);
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
