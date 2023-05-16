.. _user-management-key:

Keys
#####

A user in UVault is identified by a unique key.
For more information on this concept, please refer to the section on :ref:`keys <key-concept>`.

To define a custom key for a user in UVault, you can implement the `IEquatable<T>`_ interface.
The code snippet below demonstrates how the built-in integer-based key has been implemented.

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
