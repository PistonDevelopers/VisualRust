Visual Studio extension for Rust
================================

[![Build status](https://ci.appveyor.com/api/projects/status/5nw5no10jj0y4p3f?svg=true)]
(https://ci.appveyor.com/project/vosen/visualrust)

![Screenshot](http://i.imgur.com/63IYU6b.png)

Currently in development, and not feature complete. Stable versions are
available [on the Visual Studio extension gallery](https://visualstudiogallery.msdn.microsoft.com/c6075d2f-8864-47c0-8333-92f183d3e640).

Unstable, but more recent builds are downloadable from [AppVeyor]
(https://ci.appveyor.com/project/vosen/visualrust) (choose "Configuration:
Release" and "Artifacts").

### Debugging
Starting with Visual Studio 2015, the IDE is capable of using GDB as a debugging backend.  
Visual Rust now supports this feature!

#### Pre-requisites
- You will need to install GDB for Windows from one of the [MinGW-w64](http://mingw-w64.org) distributions **(original [MinGW](http://www.mingw.org/) won't work)**.
- When installing Visual Studio 2015, be sure to put check mark next to 
'Tools for Visual C++ Mobile Development' component.  This will install the GDB front-end package.

#### Configuration
If GDB installation directory is on your PATH, no further configuration should be required. 
Otherwise, open `Tools/Options...` dialog in the IDE, navigate to `Visual Rust/Debugging` and 
enter the full path to gdb.exe there.

#### Usage
Most of the usual VS debugger features are supported.  
You can also issue GDB-specific commands in the VS Command Window: `Debug.GDBExec <command>`. 
For convenience, `Debug.GDBExec` has been aliased to `gdb`.

#### Known issues
- The 64-bit GDB fails to start 32-bit executables. This appears to be a GDB problem. 
Meanwhile, you can use the 32-bt version of GDB for 32-bit debugging.

Contributing
============

## How to contribute?

### Issues

Feel free to open issues, post comments about progress, questions, new ideas,
brain storming etc. You can remove and edit comments as a way of refining ideas
in the issue tracker. This is very helpful because many concerns in this
project are very complex. Many issues needs to be broken down into new
issues before they can be implemented.

Issues marked *Ready* represent tasks that have a clear design and
deliverables. They are recommended starting points if you don't
want to spend time discussing and evaluating implementation.

Issues marked *Information* require some wider perspective and discussion.
They are perfect if you want to have an impact on the project but don't have
the time to spend coding.

### Code

1. Fork the main repository
2. Work on a feature in your own private branch
3. Once you are finished with you work or want someone to you, open a pull
   request
4. Someone will review your code and merge it. Some fixes might be required on
   your side.

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
