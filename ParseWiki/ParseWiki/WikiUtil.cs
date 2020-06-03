using System.Text.RegularExpressions;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public static class WikiUtil
    {
        public static string NormalizeTemplateName(string argumentName)
        {
            // MwParserUtility.NormalizeTemplateArgumentName does not handle HTML comments
            return Regex.Replace(argumentName, "<!--.*?-->", "").Trim();
        }

        public static string NormalizeTemplateName(Node argumentName)
        {
            return NormalizeTemplateName(argumentName.ToString());
        }
    }
}