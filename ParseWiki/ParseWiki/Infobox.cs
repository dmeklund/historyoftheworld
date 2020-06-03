using System;
using System.Collections.Generic;
using System.Linq;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;

namespace ParseWiki
{
    public class Infobox
    {
        private static readonly Dictionary<string, string> TemplateNameToCoord = new Dictionary<string, string>()
        {
            {"infobox settlement", "coordinates"},
            {"infobox venue", "coordinates"},
            {"infobox building", "coordinates"},
            {"infobox university", "coordinates"},
            {"infobox body of water", "coords"},
            {"infobox mountain", "coordinates"},
            {"infobox military conflict", "coordinates"},
            {"infobox civil conflict", "coordinates"},
            {"infobox ancient site", "coordinates"},
        };
        public static Infobox FromWiki(WikiBlock block, Template template)
        {
            var templateName = WikiUtil.NormalizeTemplateName(template.Name);
            TemplateArgument arg;
            Infobox box = null;
            if (TemplateNameToCoord.TryGetValue(templateName.ToLowerInvariant(), out var coordArgName))
            {
                box = FromWiki(block, template, coordArgName);
            }
            else if ((arg = FindTemplateArgument(template, "coordinates")) != null ||
                (arg = FindTemplateArgument(template, "coords")) != null)
            {
                Console.WriteLine(
                    "Found a '{0}' infobox with a '{1}' argument in '{2}' - looks like a coordinate? Using",
                    templateName,
                    arg.Name,
                    block.Title
                );
                box = FromWiki(block, arg);
            }
            return box;
        }

        public static Infobox FromWiki(WikiBlock block, Template template, string coordArgName)
        {
            var coordArg = FindTemplateArgument(template, coordArgName);
            return coordArg == null ? null : FromWiki(block, coordArg);
        }

        public static Infobox FromWiki(WikiBlock block, TemplateArgument arg)
        {
            return new Infobox(block.Id, block.Title, Coord.FromWikitext(arg.Value.ToString()));
        }

        private static TemplateArgument FindTemplateArgument(Template template, string argName)
        {
            return template.Arguments.FirstOrDefault(
                arg => MwParserUtility.NormalizeTemplateArgumentName(arg.Name) == argName);
        }

        public Infobox(int id, string title, Coord location)
        {
            Id = id;
            Title = title;
            Location = location;
        }

        public int Id { get; }
        public string Title { get; }
        public Coord Location { get; }

        public WikiLocation ToLocation()
        {
            if (Location == null)
            {
                return null;
            }

            return new WikiLocation(Id, Title, Location);
        }
    }
}