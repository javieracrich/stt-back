using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public static class Extensions
    {

        /// <summary>
        /// Returns true if the List is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool Empty<T>(this IEnumerable<T> list)
        {
            return !list.Any();
        }
    }

    public static class ExceptionExtensions
    {
        public static string InnermostMsg(this Exception e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e.Message;
        }


        public static bool DoesNotHaveValue(this int? nullable)
        {
            return !nullable.HasValue;
        }

    }
}
