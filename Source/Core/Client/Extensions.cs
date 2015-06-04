using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Client specific extension methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Checks to see if any of the object(s) in the collection are equal to the source value.
        /// </summary>
        /// <typeparam name="T">The type of object used.</typeparam><param name="source">The source object.</param><param name="list">The possible objects to equal.</param>
        /// <returns>
        /// True of any object(s) are equal to the source, false otherwise.
        /// </returns>
        public static bool EqualsAny<T>(this T source, params T[] list)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return list.Contains(source);
        }
    }
}
