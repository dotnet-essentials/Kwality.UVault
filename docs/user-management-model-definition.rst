Model definition
################

To perform user management operations with UVault, you need to define a model that represents a user in your system.
This model must inherit from the `UserModel<TKey>`_ class.

To ensure compatibility with the selected store (refer to :ref:`choosing a store <choosing-a-user-management-store>`),
the model being defined must be specified using a compatible key.

.. note::
    If you're using Auth0 as your store (see :ref:`using Auth0 <user-management-use-auth0>`) your model can also inherit
    from the `UserModel`_ class which hides certain mandatory fields of Auth0, such as password.

The following code snippet presents an instance of a user model that includes first and last names as properties, while
also utilizing the integer-based key that comes as a pre-existing feature within the project.

.. code-block:: csharp

    public sealed class UserModel : UserModel<IntKey>
    {
        public UserModel(IntKey key, string email, string firstName, string lastName)
            : base(key, email)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

.. _UserModel<TKey>: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Models/User.Model%7BTKey%7D.cs
.. _UserModel: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.Auth0/Users/Models/User.Model.cs
