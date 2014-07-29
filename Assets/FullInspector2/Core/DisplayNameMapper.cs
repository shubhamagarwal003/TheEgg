using System.Text;

namespace FullInspector.Internal {
    public static class DisplayNameMapper {
        /// <summary>
        /// Convert the given property name into a name that will be used for the Unity inspector.
        /// For example, Unity by default converts "fieldValue" into "Field Value".
        /// </summary>
        public static string Map(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                return "";
            }

            // strip initial _s
            int start = 0;
            while (start < propertyName.Length && propertyName[start] == '_') {
                ++start;
            }

            // the string is just "___"; don't modify it
            if (start >= propertyName.Length) {
                return propertyName;
            }

            var builder = new StringBuilder();
            bool forceCaptial = true;

            // insert spaces before capitals or _
            for (int i = start; i < propertyName.Length; ++i) {
                char c = propertyName[i];

                if (c == '_') {
                    forceCaptial = true;
                    continue;
                }

                if (forceCaptial) {
                    forceCaptial = false;
                    c = char.ToUpper(c);
                }

                if (char.IsUpper(c) && i != start) {
                    builder.Append(' ');
                }
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}