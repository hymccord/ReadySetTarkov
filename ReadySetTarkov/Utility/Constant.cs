using System;
using System.IO;
using System.Reflection;

namespace ReadySetTarkov.Utility
{
    static class Constant
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public static readonly string ProgramDirectory = Directory.GetParent(Assembly.Location)!.ToString();
    }
}