# Dark Notes

## Description

Dark Notes is a library that uses the Java Native Interface (JNI) to load the Java
VM into a .NET process on Windows, and exposes the Java classes and methods in an easy-to-call
way using the .NET Dynamic Language Runtime (DLR). This is meant for situations where
you have some Java functionality/classes that you really want to invoke within your
.NET programs, but would rather avoid shelling out to a separate Java program just
to do that. It's been optimized for usability rather than speed.

## Quick usage

Important: You need to have Java installed for this to work. Specifically Dark Notes
needs the jvm.dll library, and looks for it under "$JAVA_HOME\bin\server\jvm.dll". Also
typically this DLL is 64-bit, so you need to run this in a 64-bit process.

    using (dynamic java = DarkJava.CreateVm())
    {
        java.ImportPackage("java.lang");
        dynamic s = java.String.@new("Hello world");
        Assert.AreEqual("hello world", (string)s.toLowerCase());
    }

This "Hello world" example shows a few things:
1. How to create the entry point: DarkJava.CreateVm(). You typically want just one invocation
of that in your application (JNI doesn't respond kindly to attempting to host the JVM multiple
times in a process). It has overloads that lets you specify additional options to the JVM or
explicitly give the path to the jvm.dll library instead of trying to infer it.
2. Importing packages: this is Dark Notes' equivalent of "using" statements.
3. How to call constructors: You can call the constructor of any type by navigating to that type
(java.String in the above example, which thanks to the package import resolves to java.lang.String),
then invoking the "new" method on it. You can then pass in any parameters you want to that constructor.
4. How to call instance methods (this should just look familiar to you). Dark Notes does the magic
under the covers to call the proper corresponding Java method on that instance.
5. How to convert to .NET types: the result of s.toLowerCase() is a Java String object, but
when we cast it to string Dark Notes does the requisite magic to give you back a .NET string.