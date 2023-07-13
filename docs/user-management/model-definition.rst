Model definition
################

In order to carry out user management operations using UVault, it is necessary to define a model that represents a user
within your system. This user model should inherit from the `UserModel<TKey>`_ class provided by UVault.

When defining the user model, it is crucial to ensure compatibility with the chosen store, as described in the section
on ":ref:`choosing a store <choosing-a-user-management-store>`." The compatibility requirement pertains specifically to
the key used in the model.

The key used in the user model must be compatible with the selected store's requirements and capabilities. Different
stores may have varying expectations regarding the key type or format. For instance, a database-based store might
require an integer-based or GUID-based key, while an external service provider store may expect a string-based key.

By aligning the user model's key with the compatible key type of the selected store, UVault ensures seamless integration
and efficient user management operations within your application.

Therefore, when defining the user model for UVault, it is important to consider the store's requirements and choose a
compatible key type that matches the expectations of the selected store. This compatibility ensures smooth data
interaction and accurate representation of users within the UVault system.

.. note::
    If you're using Auth0 as your store see :ref:`using Auth0 <user-management-use-auth0>` your model can also inherit
    from the `UserModel`_ class which hides certain mandatory fields of Auth0, such as password.


The subsequent code snippet illustrates an instantiation of a user model that encompasses properties for first and last
names. It further leverages the project's pre-existing feature of an integer-based key.

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
