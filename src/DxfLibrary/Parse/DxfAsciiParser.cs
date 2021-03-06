// DxfAsciiParser.cs
// By: Adam Renaud
// Created: 2019-01-01

// System using statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// Internal using statements
using DxfLibrary.IO;
using DxfLibrary.DxfSpec;
using DxfLibrary.Parse.Entities;

namespace DxfLibrary.Parse
{
    /// <summary>
    /// The main Ascii parser. This function will be used as the main entry point for
    /// the parsing of Dxf Ascii Files
    /// </summary>
    public class DxfAsciiParser : IDxfParser<SectionsContainer, string, object>
    {
        public SectionsContainer Parse(IDxfReader<string, object> reader)
        {
            // Get the common Spec
            var commonSpec = SpecService.GetSpec<object>(SpecService.DxfCommonSpec);

            // Initialize the sections container
            var container = new SectionsContainer();
            
            // Section Strings
            var headerString = commonSpec.Get("Sections.HeaderString") as string;
            var entitiesString = commonSpec.Get("Sections.EntitiesString") as string;

            // Read through the file
            while(!reader.EndOfStream)
            {
                var firstItem = reader.GetNextPair();

                if (reader.EndOfStream)
                    break;
                
                var secondItem = reader.GetNextPair();

                var sectionCode = commonSpec.Get("Sections.StartCode") as string;
                var sectionString = commonSpec.Get("Sections.StartString") as string;

                // If the first item's code is a section code and its value is a section string then this is the start
                // of a section
                if ( String.CompareOrdinal(firstItem.GroupCode, sectionCode) == 0 && 
                     String.CompareOrdinal(firstItem.Value as string, sectionString as string) == 0)
                {
                    var sectionName = secondItem.Value as string;


                    if (sectionName == headerString)
                    {
                        var headerParser = new HeaderParser();
                        container.Header = headerParser.Parse(reader);
                    }
                    else if (sectionName == entitiesString)
                    {
                        var entityParser = new EntitySectionParser();
                        container.Entities = entityParser.Parse(reader);
                    }
                }
            }

            return container;
        }
    }
}