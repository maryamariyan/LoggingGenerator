// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Extensions.Logging.Internal
{
    // legit use of magic numbers for indices
#pragma warning disable S109 // Magic numbers should not be used

    // This file contains internal types exposed for use by generated code
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented
#pragma warning disable SA1402 // File may only contain a single type

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues, Exception?, string> _formatFunc;
        private readonly string _originalFormat;

        public LogValues(Func<LogValues, Exception?, string> formatFunc, string originalFormat)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 1;

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string _name;

        public LogValues(Func<LogValues<T>, Exception?, string> formatFunc, string originalFormat, string name, T value)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _name = name;
            Value = value;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return this[0];
            yield return new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 2;
        public T Value { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_name, Value),
            1 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T1, T2> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T1, T2>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string[] _names;

        public LogValues(Func<LogValues<T1, T2>, Exception?, string> formatFunc, string originalFormat, string[] names, T1 value1, T2 value2)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _names = names;
            Value1 = value1;
            Value2 = value2;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < 3; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 3;
        public T1 Value1 { get; }
        public T2 Value2 { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_names[0], Value1),
            1 => new KeyValuePair<string, object?>(_names[1], Value2),
            2 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T1, T2, T3> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T1, T2, T3>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string[] _names;

        public LogValues(Func<LogValues<T1, T2, T3>, Exception?, string> formatFunc, string originalFormat, string[] names, T1 value1, T2 value2, T3 value3)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _names = names;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < 4; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 4;
        public T1 Value1 { get; }
        public T2 Value2 { get; }
        public T3 Value3 { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_names[0], Value1),
            1 => new KeyValuePair<string, object?>(_names[1], Value2),
            2 => new KeyValuePair<string, object?>(_names[2], Value3),
            3 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T1, T2, T3, T4> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T1, T2, T3, T4>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string[] _names;

        public LogValues(Func<LogValues<T1, T2, T3, T4>, Exception?, string> formatFunc, string originalFormat, string[] names, T1 value1, T2 value2, T3 value3, T4 value4)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _names = names;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 5;
        public T1 Value1 { get; }
        public T2 Value2 { get; }
        public T3 Value3 { get; }
        public T4 Value4 { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_names[0], Value1),
            1 => new KeyValuePair<string, object?>(_names[1], Value2),
            2 => new KeyValuePair<string, object?>(_names[2], Value3),
            3 => new KeyValuePair<string, object?>(_names[3], Value4),
            4 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

#pragma warning disable S107 // Methods should not have too many parameters

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T1, T2, T3, T4, T5> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T1, T2, T3, T4, T5>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string[] _names;

        public LogValues(Func<LogValues<T1, T2, T3, T4, T5>, Exception?, string> formatFunc, string originalFormat, string[] names, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _names = names;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < 6; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 6;
        public T1 Value1 { get; }
        public T2 Value2 { get; }
        public T3 Value3 { get; }
        public T4 Value4 { get; }
        public T5 Value5 { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_names[0], Value1),
            1 => new KeyValuePair<string, object?>(_names[1], Value2),
            2 => new KeyValuePair<string, object?>(_names[2], Value3),
            3 => new KeyValuePair<string, object?>(_names[3], Value4),
            4 => new KeyValuePair<string, object?>(_names[4], Value5),
            5 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValues<T1, T2, T3, T4, T5, T6> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValues<T1, T2, T3, T4, T5, T6>, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly string[] _names;

        public LogValues(
            Func<LogValues<T1, T2, T3, T4, T5, T6>, Exception?, string> formatFunc,
            string originalFormat,
            string[] names,
            T1 value1,
            T2 value2,
            T3 value3,
            T4 value4,
            T5 value5,
            T6 value6)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _names = names;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < 7; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => 7;
        public T1 Value1 { get; }
        public T2 Value2 { get; }
        public T3 Value3 { get; }
        public T4 Value4 { get; }
        public T5 Value5 { get; }
        public T6 Value6 { get; }

        public KeyValuePair<string, object?> this[int index] => index switch
        {
            0 => new KeyValuePair<string, object?>(_names[0], Value1),
            1 => new KeyValuePair<string, object?>(_names[1], Value2),
            2 => new KeyValuePair<string, object?>(_names[2], Value3),
            3 => new KeyValuePair<string, object?>(_names[3], Value4),
            4 => new KeyValuePair<string, object?>(_names[4], Value5),
            5 => new KeyValuePair<string, object?>(_names[5], Value6),
            6 => new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    /// <summary>
    /// Implementation detail of the logging source generator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LogValuesN : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly Func<LogValuesN, Exception?, string> _formatFunc;
        private readonly string _originalFormat;
        private readonly KeyValuePair<string, object?>[] _kvp;

        public LogValuesN(Func<LogValuesN, Exception?, string> formatFunc, string originalFormat, KeyValuePair<string, object?>[] kvp)
        {
            _formatFunc = formatFunc;
            _originalFormat = originalFormat;
            _kvp = kvp;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (int i = 0; i < _kvp.Length + 1; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => _formatFunc(this, null);
        public int Count => _kvp.Length + 1;

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                if (index >= 0 && index < _kvp.Length)
                {
                    return _kvp[index];
                }

                if (index == _kvp.Length)
                {
                    return new KeyValuePair<string, object?>("{OriginalFormat}", _originalFormat);
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
