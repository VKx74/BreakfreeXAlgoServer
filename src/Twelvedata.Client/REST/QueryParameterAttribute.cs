using System;

namespace Twelvedata.Client.REST
{
    public class QueryParameterAttribute : Attribute
    {
        public QueryParameterAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
