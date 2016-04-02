using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codit.Blog.Cache
{
    public class Maybe<T>
    {
        private T _value;

        public bool IsPresent
        {
            get;
            private set;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    throw new InvalidOperationException();
                }

                return _value;
            }
        }

        public Maybe()
        {
            IsPresent = false;
        }

        public Maybe(T value)
        {
            _value = value;

            // If we have a value type, it's always present.
            if (typeof(T).IsValueType)
            {
                IsPresent = true;
            }
            // If it's a reference type, it's present if the value is not null
            else
            {
                IsPresent = (value != null);
            }
        }
    }
}

