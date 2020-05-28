using System.Collections.Generic;


namespace DateLocNLP
{
    public class NlpProperties
    {
        private List<string> _annotators { get; set; }
        public string outputformat { get; set; }

        public string annotators
        {
            get => string.Join(", ", _annotators);
            set => _annotators = new List<string>(value.Split(", "));
        }

        public NlpProperties()
        {
            _annotators = new List<string>();
            outputformat = "json";
        }

        public void AddAnnotator(string annotator)
        {
            _annotators.Add(annotator);
        }
    }
}