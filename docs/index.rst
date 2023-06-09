Introduction
############

UVault is a collection of .NET libraries specifically designed to handle user and application management operations in
.NET applications. It is compatible with the following platforms:

- `Microsoft .NET 6 <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>`_
- `Microsoft .NET 7 <https://dotnet.microsoft.com/en-us/download/dotnet/7.0>`_

Supported features
******************

The following features are supported:

- :doc:`Authentication / authorization (JWT Bearer) <iam/introduction>`
- :doc:`User Management <user-management/introduction>`

Getting started
***************
.. toctree::
   :maxdepth: 1
   :caption: Getting started

   installation
   concepts/key-concepts
   quality
   benefits

.. toctree::
  :maxdepth: 1
  :caption: Identity & access management

  iam/introduction
  iam/getting-started
  iam/custom-jwt-validation

.. toctree::
  :maxdepth: 1
  :caption: User management

  user-management/introduction
  user-management/keys
  user-management/model-definition
  user-management/stores
  user-management/dependency-injection
  user-management/operations

.. toctree::
  :maxdepth: 1
  :caption: Contributing

  contributing/contribute
  contributing/documentation
