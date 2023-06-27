Contributing
############

Contributions to open-source projects play a crucial role in their success, and UVault is no exception. We highly
appreciate and value all contributions, regardless of their scale or scope. Whether it's a small bug fix, a feature
enhancement, documentation improvements, or any other form of contribution, they all contribute to making UVault better
and more robust.

If you are interested in contributing to UVault, we encourage you to get involved! You can start by exploring the
UVault repository on GitHub, where you will find the project's source code, issue tracker, and other resources. You can
review the existing issues, propose new ideas, submit bug reports, or even submit pull requests with your own code
contributions.

UVault follows common open-source contribution practices, including the use of version control, code review, and
continuous integration. By adhering to these practices, we strive to maintain a collaborative and inclusive environment
for contributors.

We understand that contributing to open-source projects can be a rewarding experience that allows you to sharpen your
skills, gain exposure to new technologies, and make a positive impact on the community. We value the time and effort you
put into making UVault better, and we are grateful for your contributions.

So, whether you're a seasoned developer or just getting started, we welcome your contributions to UVault. Together, we
can continue to improve and enhance UVault's capabilities, making it an even more powerful tool for user management in
.NET applications.

Finding existing issues
************************

Before submitting a new issue, we kindly request that you first check our `issue tracker`_ to ensure that a similar
issue has not already been reported.

By reviewing the existing issues, you can avoid duplicating efforts and contribute to a more organized and efficient
issue tracking system. It also helps us focus on addressing new and unique problems or feature requests that have not
yet been addressed.

If you find a similar issue in the tracker, you can add any relevant information or additional context to the existing
issue. This helps consolidate related discussions and provides a more comprehensive understanding of the problem.

However, if you cannot find a similar issue, we encourage you to create a new one. When doing so, please provide a clear
and concise description of the problem, including any necessary steps to reproduce it. This allows us to better
understand and investigate the issue, ultimately leading to a quicker resolution.

Writing a Good Bug Report
=========================

Bug reports play a crucial role in helping maintainers verify and identify the root cause of an issue. The quality of a
bug report directly affects the speed at which a problem can be resolved. To ensure an effective bug report, please
include the following details:

* A brief overview of the problem: Start by providing a concise description of the issue you are experiencing. This
  helps set the context for the problem.
* Minimal reproduction scenario: It is essential to provide a minimal, self-contained code or configuration example
  that reproduces the undesired behavior. The goal is to isolate the issue and make it easier for maintainers to
  understand and debug the problem. By minimizing the reproduction scenario, you help focus the investigation on the
  specific area of concern.
* Comparison of expected versus actual behavior: Clearly state what you expected to happen and describe the actual
  behavior observed. This contrast allows maintainers to quickly identify any deviations and determine the root cause.
* Environment information: Include relevant information about your environment, such as the version of UVault, the
  version of .NET framework being used, and any other pertinent system details. This information helps in reproducing
  and investigating the issue in the same or similar environment.
* Additional information: If applicable, provide any additional information that could assist in understanding the
  issue. This might include details about any known workarounds, whether the issue is a regression from previous
  versions, or any related observations or insights you have.

To submit a bug report, we kindly request that you use the provided `bug report`_ issue template. This template helps
ensure that all the necessary information is included, making it easier for maintainers to review and address the
reported issue.

To submit a bug report, please use the `bug report`_ issue template.

Contributing Changes
====================

The UVault core library follows a strict policy of minimizing dependencies, and therefore, changes that introduce
additional dependencies are rarely accepted. If there is a need to add an extra dependency, we encourage you to consider
designing a solution where it can be incorporated into a separate project rather than the core library itself.

In order to maintain a consistent and cohesive experience throughout the library, all new APIs, modifications, and
deletions must undergo a thorough review process. Before submitting a pull request, it is important to propose, discuss,
and seek approval for changes to the API. This can be done by opening a separate issue and using the `API approved`
label to indicate that the proposed changes have been reviewed and approved.

This review process ensures that any modifications to the API are carefully evaluated to maintain the library's design
principles, compatibility, and overall quality. It also provides an opportunity for collaboration and feedback from the
community, helping to ensure that the library continues to meet the needs of its users.

We appreciate your understanding and cooperation in adhering to these guidelines. By following this process, we can
collectively maintain the integrity and reliability of the UVault core library while also fostering a collaborative and
inclusive development environment.

DOs and DON'Ts
==============

When submitting a pull request, it is important to follow the guidelines outlined below to ensure a smooth and effective
review process:

* Target the `main` branch when creating your pull request.
* Adhere to the established coding style used in the existing project.
* Ensure that your code does not generate any issues flagged by ReSharper (CLI). Pull requests that contain
  ReSharper-detected issues may be rejected.
* Verify that your changes are accompanied by appropriate tests, either new or existing.
* Evaluate the quality of your tests by performing mutation testing using Stryker.
* Take care not to decrease code coverage unless it has been approved by the project's authors.
* If your contribution affects the documentation, please update the relevant documentation files accordingly.

To increase the likelihood of your pull request being well-received, please avoid the following:

* Avoid creating excessively large pull requests. Instead, initiate a discussion by opening an issue to propose and
  discuss the direction of your proposed changes. This is particularly important for modifications to the public API or
  substantial changes that may require significant review and discussion.

By following these guidelines, you contribute to the smooth collaboration and maintain the quality of the UVault
project. Your adherence to these best practices is greatly appreciated.

Validate your work locally
==========================

To ensure that your pull request (PR) does not introduce any issues or break existing functionality, it is highly
recommended to run the test suite locally before submitting it.

As UVault integrates closely with Auth0, it is necessary to have an Auth0 account (which is free) before proceeding.
To set up your Auth0 account, follow these steps:

1. Access your tenant's settings and change the value of the "Default Directory" to "Username-Password-Authentication".
2. Create a "Regular Web Application" in your tenant and enable the "Password" and "Client Credentials" grant types.
3. Create an API in your tenant or use the default "Auth0 Management API". Make sure that your application is authorized
   and has all the necessary permissions for this tenant. You can configure this under the "Machine To Machine
   Applications" section.
4. Create 2 more "Regular Web Applications" in your tenant and enable the "Password" and "Client Credentials" grant
   type. These application's will be referred to as "test" applications in this document.
5. Create a user in your tenant using the "Username-Password-Authentication" connection.
6. Create two "Database Connections" named "DEV-CNN-1" and "DEV-CNN-2" in your tenant and enable your application for
   them.

Once your Auth0 account is configured, set the following environment variables:

- ``AUTH0_AUDIENCE``: The API audience of the API in your tenant.
- ``AUTH0_CLIENT_ID``: The Client ID of the application in your tenant.
- ``AUTH0_CLIENT_SECRET``: The Client Secret of the application in your tenant.
- ``AUTH0_TOKEN_ENDPOINT``: The endpoint of your Auth0 tenant used to authenticate.
- ``AUTH0_USERNAME``: The username of the created Auth0 user.
- ``AUTH0_PASSWORD``: The password of the created Auth0 user.
- ``AUTH0_VALID_AUDIENCE``: The audience that's supported by your Auth0 tenant.
- ``AUTH0_VALID_ISSUER``: The issuer that's supported by your Auth0 tenant.
- ``AUTH0_TEST_APPLICATION_1_CLIENT_ID``: The Client ID of one the first "test" application.
- ``AUTH0_TEST_APPLICATION_2_CLIENT_ID``: The Client ID of one the second "test" application.

.. _issue tracker: https://github.com/dotnet-essentials/Kwality.UVault/issues
.. _bug report: https://github.com/dotnet-essentials/Kwality.UVault/issues/new/choose
.. _pull request: https://help.github.com/articles/using-pull-requests
.. _Stryker: https://stryker-mutator.io/docs/stryker-net/introduction/
.. _documentation: https://kwalityuvault.readthedocs.io/en/latest/
