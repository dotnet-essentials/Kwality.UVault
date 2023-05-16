Dependency inection
###################

Each user management task is made accessible through a manager object. For further clarification on this concept, please
refer to the :ref:`manager <manager-concept>` section.

After UVault has been properly configured (see :ref:`choosing a management store <choosing-a-user-management-store>`),
this manager can be injected into your HTTP handlers.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        context.Response.StatusCode = 200;

        return Task.CompletedTask;
    });
