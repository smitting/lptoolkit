LPToolKit (open source Midi Symbiont)
=========

LPToolKit is the open source engine for the future commercial application MidiSymbiont, which allows multiple MIDI devices such as the Novation LaunchPad and LaunchControl to work together by using javascript to create "implants".  More details on MidiSymbiont are available at http://www.midisymbiont.com/ including programming reference for implants.

The entire engine for MidiSymbiont is intended to remain open source to encourage use of implants in other software.  My goal is to expand the use of implants as wide as possible, MidiSymbiont is only being released commercially so as to fund purchasing more gear to make more drivers.

CAUTION - THIS SOFTWARE IS CURRENTLY IN AN EARLY ALPHA STATE AND MAY BE SUBJECT TO API CHANGES.

If you still want to use the software, the ImplantConsole application is what you will want to build.  This is a simple console application that is controlled via an embedded web server.  It launches its settings web page in your default web browser upon launching.



Project Organization
=========

This solution is be developed with Visual Studio 2013 (and Xamarin Studio for OS X builds).  Below is a description of the projects within:

APPLICATIONS

ImplantConsole - This is an example console application hosting the complete LPToolKit engine, and is the project you should run.  The output from VS.Net runs natively on both Windows and Mac OS X, and is what my band uses at practice.

ImplantPlayer - This program will soon be obsolete, but is an example of embedding a web browser into a Windows GUI application to host LPToolKit.  Its use is not recommended.

LIBRARIES

ImplantApp - This contains base classes for implementing a full application using the LPToolKit engine.  It provides the majority of the default behavior such an application would need, but includes hooks for any situation-specific code that is necessary, like the need to display a specific web page or STDOUT coming from a javascript implant.

LPToolKit - This library contains the entire LPToolKit engine, including platform specific MIDI implementations for Windows and Mac OS X, the javascript implant engine, and a (semi-) real-time kernel for processing events and actions out-of-order based on priority (for example, a MIDI note should never not go out to a software instrument on-time just because we need to change some lights on a launchpad or respond to a web server request)

OTHER FOLDERS

Lib - Libraries used.

packages - NuGet's packaging management folder.

