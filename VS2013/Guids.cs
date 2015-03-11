// Guids.cs
// MUST match guids.h
using System;

namespace Company.VSWorkingSetPkg
{
    static class GuidList
    {
        public const string guidVSWorkingSetPkgPkgString = "350af5ed-1d03-468a-9b7e-90719854eac6";
        public const string guidVSWorkingSetPkgCmdSetString = "4c24442b-57d5-422f-a7ef-730befd8d2c8";
        public const string guidToolWindowPersistanceString = "daa557e0-f63f-443b-acfa-c51976b62449";
        public const string guidDynamicVSWorkingSetMenuString = "9d9046da-94f8-4fd0-8a00-92bf4f6defa8";

        public static readonly Guid guidVSWorkingSetPkgCmdSet = new Guid(guidVSWorkingSetPkgCmdSetString);
        public static readonly Guid guidDynamicVSWorkingSetMenu = new Guid(guidDynamicVSWorkingSetMenuString);
    };
}