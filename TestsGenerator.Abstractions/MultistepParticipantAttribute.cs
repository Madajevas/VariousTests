using System;

namespace TestsGenerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class MultistepParticipantAttribute : Attribute
    {
    }
}
