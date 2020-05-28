using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace DateLocNLP
{
    public class NlpResult
    {
        public List<Sentence> sentences { get; set; }
        public Dictionary<string, Coref[]> corefs { get; set; }
    }

    public class Sentence
    {
        public int index { get; set; }
        public List<Dependency> basicDependencies { get; set; }
        public List<Dependency> enhancedDependencies { get; set; }
        public List<Dependency> enhancedPlusPlusDependencies { get; set; }
        public List<OpenIe> openie { get; set; }
        public List<EntityMention> entitymentions { get; set; }
        public List<Token> tokens { get; set; }
        public override string ToString()
        {
            return string.Join(' ', tokens);
        }
    }

    public class Dependency
    {
        public string dep { get; set; }
        public int governor { get; set; }
        public string governorGloss { get; set; }
        public int dependent { get; set; }
        public string dependentGloss { get; set; }
    }

    public class OpenIe
    {
        public string subject { get; set; }
        public int[] subjectSpan { get; set; }
        public string relation { get; set; }
        public int[] relationSpan { get; set; }
        [JsonPropertyName("object")] 
        public string object_ { get; set; }
        public int[] objectSpan { get; set; }
    }

    public class EntityMention
    {
        public int docTokenBegin { get; set; }
        public int docTokenEnd { get; set; }
        public int tokenBegin { get; set; }
        public int tokenEnd { get; set; }
        public string text { get; set; }
        public int characterOffsetBegin { get; set; }
        public int characterOffsetEnd { get; set; }
        public string ner { get; set; }
        public Dictionary<string, float> nerConfidence { get; set; }
    }

    public class Token
    {
        public int index { get; set; }
        public string word { get; set; }
        public string originalText { get; set; }
        public string lemma { get; set; }
        public int characterOffsetBegin { get; set; }
        public int characterOffsetEnd { get; set; }
        public string pos { get; set; }
        public string ner { get; set; }
        public string speaker { get; set; }
        public string before { get; set; }
        public string after { get; set; }
        public override string ToString()
        {
            return originalText;
        }
    }

    public class Coref
    {
        public int id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public string number { get; set; }
        public string gender { get; set; }
        public string animacy { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public int headIndex { get; set; }
        public int sentNum { get; set; }
        public int[] position { get; set; }
        public bool isRepresentativeMention { get; set; }
    }
}