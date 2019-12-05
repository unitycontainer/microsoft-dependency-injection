using System;
using System.Collections.Generic;
using System.Text;
using Unity.Registration;

namespace Unity.Microsoft.DependencyInjection
{
    public class DescriptorRegistration 
    {
        private ContainerRegistration _registration;

        public DescriptorRegistration(ContainerRegistration registration)
        {
            _registration = registration;
        }


    }
}
