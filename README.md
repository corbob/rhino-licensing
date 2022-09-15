# Rhino-Licensing

A fork of the original Rhino.Licensing software licensing framework used for [Chocolatey](https://github.com/chocolatey/choco).

[https://www.hibernatingrhinos.com/oss/rhino-licensing](https://www.hibernatingrhinos.com/oss/rhino-licensing) [http://ayende.com](http://ayende.com)

## Building the Project

Visual Studio or VS Code can be used to build the solution.

A full build, test, and pack run can be started using the `build.ps1` PowerShell script.

## Testing

Running `build.ps1 -Target Test` will run unit tests for the project.

### Test License files

The test license files are stored in the `test/Rhino.Licensing.Tests/LicenseFiles` directory, and automatically embedded in the Tests project.
This allows us to test the generator and validator separately.
If new license types or signing algorithms are added, new license files will need to be made.

To generate new license files, run the following PowerShell: `$env:GENERATE_TEST_LICENSES = Resolve-Path .\test\Rhino.Licensing.Tests\LicenseFiles\ ; ./build.ps1 -Target Test`.
You will then need to commit the changed files to the repository.


## Acknowledgements

Rhino Licensing is making use of the excellent log4net logging framework.
