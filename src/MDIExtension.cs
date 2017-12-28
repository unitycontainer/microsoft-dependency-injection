using System;
using System.Collections.Generic;
using System.Text;
using Unity.Extension;
using Unity.Policy;

namespace Unity.Microsoft.DependencyInjection
{
    internal class MDIExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Policies.SetDefault<IConstructorSelectorPolicy>(new ConstructorSelectorPolicy());
        }
    }
}
