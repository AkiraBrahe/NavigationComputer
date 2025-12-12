using System.Text;
using System.Text.RegularExpressions;

namespace NavigationComputer.Utils
{
    public static class RichTextWrapper
    {
        private static readonly Regex tagRegex = new("<.*?>", RegexOptions.Compiled);
        private static readonly Regex nameDetailRegex = new(@"(.*)(\s+\(.*\)$)", RegexOptions.Compiled);
        private const int tabWidth = 4;
        private const int defaultWrapWidth = 42;

        /// <summary>
        /// Gets the visible display length of a string after stripping rich text tags.
        /// </summary>
        private static int GetVisibleLength(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            string expandedTabs = text.Replace("\t", new string(' ', tabWidth));
            return tagRegex.Replace(expandedTabs, "").Length;
        }

        /// <summary>
        /// Wraps text with a pattern of "Name (details)", prioritizing wrapping before the details.
        /// </summary>
        public static string WrapLine(string text)
        {
            if (string.IsNullOrEmpty(text) || GetVisibleLength(text) <= defaultWrapWidth)
            {
                return text;
            }

            var match = nameDetailRegex.Match(text);
            if (match.Success)
            {
                string namePart = match.Groups[1].Value;
                string detailPart = match.Groups[2].Value;

                if (GetVisibleLength(namePart) <= defaultWrapWidth)
                {
                    return $"{namePart}\n\t{detailPart.TrimStart()}";
                }
            }

            return WrapLine(text, defaultWrapWidth);
        }

        /// <summary>
        /// Wraps a single line of text containing rich text tags to a max width, indenting wrapped lines with a tab.
        /// </summary>
        public static string WrapLine(string text, int maxLineLength)
        {
            if (string.IsNullOrEmpty(text) || GetVisibleLength(text) <= maxLineLength)
            {
                return text;
            }

            string[] words = text.Split(' ');
            var wrappedText = new StringBuilder();
            int currentLineLength = 0;

            foreach (string word in words)
            {
                int wordVisibleLength = GetVisibleLength(word);

                if (currentLineLength == 0 && wordVisibleLength > maxLineLength)
                {
                    wrappedText.Append(word);
                    currentLineLength = wordVisibleLength;
                    continue;
                }

                if (currentLineLength == 0)
                {
                    wrappedText.Append(word);
                    currentLineLength = wordVisibleLength;
                }
                else if (currentLineLength + 1 + wordVisibleLength <= maxLineLength)
                {
                    wrappedText.Append(" ");
                    wrappedText.Append(word);
                    currentLineLength += 1 + wordVisibleLength;
                }
                else
                {
                    wrappedText.Append("\n\t");
                    wrappedText.Append(word);
                    currentLineLength = tabWidth + wordVisibleLength;
                }
            }
            return wrappedText.ToString();
        }
    }
}