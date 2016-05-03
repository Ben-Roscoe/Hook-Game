using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Nixin
{
    public class LocalisationLanguage
    {


        // Public:


        public const string         Extension   = ".xml";

        public const string         English     = "English";
        public const string         Japanese    = "Japanese";


        public LocalisationLanguage( string language, List<string> xmls )
        {
            this.language = language;
            ParseLocalisationFiles( xmls );
        }


        public string GetLocalString( string id )
        {
            string outStr;
            if( !strings.TryGetValue( id, out outStr ) )
            {
                outStr = invalidString;
            }
            return outStr;
        }


        public string Language
        {
            get
            {
                return language;
            }
        }


        // Private:


        private const string    stringItemKey   = "String";
        private const string    idItemKey       = "StringId";
        private const string    invalidString   = "INVALID_STRING_ID";

        private Dictionary<string, string>      strings     = new Dictionary<string, string>();
        private string                          language    = null;



        private void ParseLocalisationFiles( List<string> xmls )
        {
            for( int i = 0; i < xmls.Count; ++i )
            {
                var    doc  = XDocument.Parse( xmls[i] );

                // Try and find the text id.
                var elements = doc.Root.Elements( stringItemKey );
                for( int e = 0; e < elements.Count(); ++e )
                {
                    var element         = elements.ElementAt( e );
                    var id              = element.Element( idItemKey ).Value;
                    var str             = element.Element( language ).Value;

                    strings[id] = str;
                }
            }
        }
    }


    public class LocalTextLanguageNotFoundException : Exception
    {
        public LocalTextLanguageNotFoundException() : base()
        {
        }


        public LocalTextLanguageNotFoundException( string msg ) : base( msg )
        {
        }


        public LocalTextLanguageNotFoundException( string msg, Exception inner ) : base( msg, inner )
        {
        }
    }


    public class LocalTextIdNotFoundException : Exception
    {
        public LocalTextIdNotFoundException() : base()
        {
        }


        public LocalTextIdNotFoundException( string msg ) : base( msg )
        {
        }


        public LocalTextIdNotFoundException( string msg, Exception inner ) : base( msg, inner )
        {
        }
    }
}
