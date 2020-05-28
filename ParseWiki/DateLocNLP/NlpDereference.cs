using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DateLocNLP
{
    public static class NlpDereference
    {
        public static string dereference(int sentenceNum, int startIndex, int endIndex, Dictionary<string, Coref[]> corefDict)
        {
            string result = null;
            Coref[] corefs = (
                from val in corefDict.Values 
                from coref in val 
                where coref.sentNum == sentenceNum && coref.startIndex == startIndex && coref.endIndex == endIndex 
                select val).FirstOrDefault();
            return corefs == null ? null : FindBest(corefs);
        }
        
        public static string FindBest(Coref[] corefs)
        {
            var result = "";
            foreach (var coref in corefs)
            {
                if (coref.type != "PROPER") continue;
                if (coref.text.Length > result.Length)
                {
                    result = coref.text;
                }
            }
            return result;
        }
    }
}