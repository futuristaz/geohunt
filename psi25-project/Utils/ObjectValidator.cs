using System;
using System.Linq.Expressions;

namespace psi25_project.Utils
{
    public class ObjectValidator<T> where T : class, new()
    {
        public T CreateDefault()
        {
            return new T();
        }
        public bool ValidatePropertyNotNull(T obj, Expression<Func<T, object?>> selector)
        {
            var compiled = selector.Compile();
            return compiled(obj) != null;
        }
    }
}
