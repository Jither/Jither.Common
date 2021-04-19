﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jither.Reflection
{
    public static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly)
        {
            return
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                assembly.GetName().Version.ToString(); // Fall back to assembly version if informational isn't defined.
        }
    }
}
