using System;
using System.Linq;

namespace Uzi.Core
{
    public static class ResolveName
    {
        /// <summary>
        /// Gets the Type.Name or the SourceInfo.Name
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string SourceName(this object target)
        {
            if (target is string)
                return target as string;

            var _tType = target as Type ?? target.GetType();
            if (_tType.GetCustomAttributes(typeof(SourceInfoAttribute), false).FirstOrDefault() is SourceInfoAttribute _srcName)
                return _srcName.Name;
            else
                return _tType.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SourceInfoAttribute : Attribute
    {
        public SourceInfoAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name associated with the class
        /// </summary>
        public string Name { get; private set; }
    }
}
