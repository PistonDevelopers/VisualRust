Visual Studio extension for Rust
================================

[![Build status](https://ci.appveyor.com/api/projects/status/5nw5no10jj0y4p3f?svg=true)]
(https://ci.appveyor.com/project/vosen/visualrust)

![Screenshot](http://i.imgur.com/T06C5wH.png)

Currently in development, and not feature complete. Stable versions are
available [on the Visual Studio extension gallery](http://visualstudiogallery.msdn.microsoft.com/dcd9075c-46da-4164-be4a-4d09589efcad).

Unstable, but more recent builds are downloadable from [AppVeyor]
(https://ci.appveyor.com/project/vosen/visualrust) (choose "Configuration:
Release" and "Artifacts").

Contributing
============

## How to contribute?

We use the same model as the main Piston project. Read their [How to contribute](https://github.com/PistonDevelopers/piston/blob/master/CONTRIBUTING.md) for 
an explanation and some pointers.

## Prerequisites

To build this, you'll need the [Visual Studio
SDK](http://msdn.microsoft.com/en-us/vstudio/vextend.aspx) for the VS plugin,
[WiX Toolset 3.9](http://wixtoolset.org/) for the setup project and [Java RE]
(https://www.java.com/en/download/manual.jsp).
Nuget will take care of the rest.

## Build configuration

If you plan to hask on Visual Rust you should understand difference between
our two build configurations. 
* For the `Release` configuration the main project is `VisualRust.Setup`,
  its output is a .msi file that consists of two parts:
  * MSBuild integration: this lets you build Rust projects (.rsproj) from
    the command line and Visual Studio.
  * Visual Studio plugin(s): this adds support for Rust projects (.rsproj)
   inside Visual Studio. Syntax highlighting, project system, item templates.
   Everything except building.
   
* For the `Debug` build main project is called simply `VisualRust` and it builds
 `VisualRust.vsix` which is a VS plugin in a format that is suitable for
 local installation and debugging. **It doesn't contain MSBuild integration**
 
 Consequently, for the debug build you'll want to either install just MSBuild
 integration from the .msi file or build it yourself (`VisualRust.Build`) and
 copy to `%ProgramFiles(x86)%\MSBuild\VisualRust`.
 
 Also you'll want to modify `VisualRust` project with location of your
 Visual Studio installation to [launch it automatically when debugging]
 (http://stackoverflow.com/a/9281921).


Contact
=======

This project is currently maintained by [vosen](https://github.com/vosen/).

Feel free to mail him or ask around in `#rust-gamedev` on irc.mozilla.org.

License
=======

Same as Rust, dual MIT/ASL2. Any contributions made are under this license.
