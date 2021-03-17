// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Microsoft.Extensions.Logging.Internal
{
    // This file contains internal types exposed for use by generated code
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

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

                if (e == null)
                {
                    _ = sb.Append("(null)");
                }
                else
                {
                    if (e is IFormattable fmt)
                    {
                        _ = sb.Append(fmt.ToString(null, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        _ = sb.Append(e);
                    }
                }

                first = false;
            }

            _ = sb.Append(']');

            return sb.ToString();
        }
    }
}
