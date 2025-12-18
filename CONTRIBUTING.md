# Contributing

If you want to contribute to ScoopUtilities then please do the following before sending pull requests:

* Please read (and sign if required) the [contributor license agreement](Licenses/CLA.txt).
* Follow the coding guidelines.
* Document new code.
* Create tests that cover new functionality.
* Run all tests and make sure they pass before submitting.

## Coding guidelines

The solution includes a .editorconfig file which defines basic editor rules specifying basic settings such as
indentation, naming and other coding conventions. Please use an editor which supports these settings or otherwise
ensure that the code that respects the settings it defines.

## Documentation

Each public unit of code should be sufficiently documented so that a user unfamiliar with the project can figure out what
the unit does, when and how to use it. Code examples demonstrating usage of the unit is highly desirable. In private
members, the standard can be relaxed some, but should still be sufficient for other contributors to understand the code
well enough to be able to work with it.

## Testing

Unit tests should cover each unit of code. Each project has its own test project where these unit tests
should be added. The tests should ideally execute in a small fraction of a second so that all tests can be run in a
reasonable amount of time. The tests should also ideally cover all functionality. It is much easier to root out
bugs early in the development cycle through unit testing than trying to isolate and reproduce the problem later on.

Tests should be deterministic and non-flaky. Single threaded tests are preferable when multi-threading is not 
essential. Use seeds when using randomization and avoid Thread.Sleep as it often results in unpredictable order of
execution. If a test contains elements which behave stochastic, then run the test a large number of times before
submitting the code, to ensure it is not flaky.

Tests should be hermetic, i.e. not rely on some outside service or resources which may cause non-deterministic
behaviour. Use mocking to provide the services needed to run the test.