using UnityEngine;
using System.Collections;
using System.Xml;


namespace Nixin
{
    public static class SupportedLanguage
    {
        public static readonly string   English = "'en'";
    }

    public class GameResources
    {


        // Public:


        public  string           Language           { get; set; }


        public GameResources()
        {
            // Will have to implement a config file. For now just set default to english.
            Language = SupportedLanguage.English;

            try
            {
                stringResourceDocument.Load( stringResourcePath );
            }
            catch( System.Exception e )
            {
                // If loading failed, log the except so it appears in the unity editor.
                Debug.LogError( "Failed to load string resource document." );
                Debug.LogException( e );
            }
        }


        public string GetResourceString( string id )
        {
            var node = stringResourceDocument.SelectSingleNode( "localStrings/string[@id='" + id + "']/text[@lang=" + Language + "]" );
            if( node == null )
            {
                Debug.LogError( "Unable to retrieve resource string for id '" + id + "'." );
                return "ERROR READING RESOURCE FILE";
            }

            return node.InnerText;
        }


        // Private:


        private const string                stringResourcePath      = "Assets/Strings/StringResources.xml";

        private XmlDocument                 stringResourceDocument  = new XmlDocument();
    }
}