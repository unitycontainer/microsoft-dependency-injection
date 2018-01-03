using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Microsoft.DependencyInjection
{
    public class Aggregates
    {
        public List<Aggregate> Types { get; set; }

        public Aggregates(List<Aggregate> types)
        {
            Types = types;
        }

        public Aggregate Get(Type t)
        {
            return Types.Where(a => a.Type == t).FirstOrDefault();
        }

        public void Register()
        {
            foreach (var type in Types)
            {
                type.Register();
            }
        }
    }
}
