Installation
============

To utilize UVault's User Management component, you must first include a reference to Kwality.UVault.User.Management.dll
in your project. The most straightforward approach to do this is through either the NuGet package manager or the
dotnet CLI.

To accomplish this through the NuGet package manager console within Visual Studio, execute the following command:

.. code-block:: console

    Install-Package Kwality.UVault.User.Management

Alternatively, you can use the .NET Core CLI from a terminal window:

.. code-block:: console

    dotnet add package Kwality.UVault.User.Management

.. note::
  If you want to use `Auth0` as a store, a reference to the package ``Kwality.UVault.User.Management.Auth0`` must be
  added.
