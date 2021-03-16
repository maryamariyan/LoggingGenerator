// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.ComponentModel;
using System.Text;

namespace Microsoft.Extensions.Logging.Internal
{
    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ArgumentFormatter
    {
        public static string Enumerate(IEnumerable? enumerable)
        {
            if (enumerable == null)
            {
                return "(null)";
            }

            var sb = new StringBuilder();
            _ = sb.Append('[');

            bool first = true;
            foreach (object e in enumerable)
            {
                if (!first)
                {
                    _ = sb.Append(", ");
                }

                _ = sb.Append(e != null ? e.ToString() : "(null)");
                first = false;
            }
            _ = sb.Append(']');

            return sb.ToString();
        }
    }
}
