using System.Reflection;

[assembly: AssemblyTitle("copyNBTlib")]
[assembly: AssemblyDescription("Library for handling NamedBinaryTag data")]
[assembly: AssemblyVersion("1.0.*")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
