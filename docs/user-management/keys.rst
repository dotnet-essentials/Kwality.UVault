.. _user-management-key:

Keys
#####


In UVault, a user is uniquely identified by a key that serves as a distinctive identifier. For detailed information
regarding this concept, please consult the section dedicated to :ref:`keys <key-concept>` in the UVault documentation.

To define a custom key for a user in UVault, the implementation of the `IEquatable<T>`_ interface is required.
The subsequent code snippet illustrates the implementation of the default integer-based key:

.. code-block:: csharp

    public sealed class IntKey : IEqualityComparer<IntKey>
    {
        private readonly int value;

        public IntKey(int value)
        {
            this.value = value;
        }

        public bool Equals(IntKey? x, IntKey? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.value == y.value;
        }

        public int GetHashCode(IntKey obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            return obj.value;
        }
    }

.. _IEquatable<T>: https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=net-7.0
