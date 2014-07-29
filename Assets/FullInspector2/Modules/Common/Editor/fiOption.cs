using System;

namespace FullInspector.Modules.Common {
    public static class fiOption {
        public static fiOption<T> Just<T>(T value) {
            return new fiOption<T>(value);
        }
    }

    /// <summary>
    /// A simple monad that can either contain or not contain a value.
    /// </summary>
    public struct fiOption<T> {

        // remark: We ensure that the default constructor state makes the option empty.


        private bool _hasValue;
        private T _value;

        public fiOption(T value) {
            _hasValue = true;
            _value = value;
        }

        public static fiOption<T> Empty = new fiOption<T>() {
            _hasValue = false,
            _value = default(T)
        };

        public bool HasValue {
            get {
                return _hasValue;
            }
        }

        public T Value {
            get {
                if (HasValue == false) {
                    throw new InvalidOperationException("There is no value inside the option");
                }

                return _value;
            }
        }
    }
}