Dependency inection
###################

The accessibility of each management task related to API's, grants, users and more is facilitated by means of a manager
object. For a more comprehensive understanding of this concept, please consult the corresponding
:ref:`section <manager-concept>` dedicated to managers.

Once UVault has been suitably configured (as outlined in the section
":ref:`choosing a management store <choosing-a-user-management-store>`"), this manager can be injected into your HTTP
handlers.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        context.Response.StatusCode = 200;

        return Task.CompletedTask;
    });
