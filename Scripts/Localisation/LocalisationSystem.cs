using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Nixin
{
    public class LocalisationSystem : NixinSystem
    {


        // Public:


        public LocalisationSystem( World world, string localisationFolderPath ) : base( world )
        {
            this.localisationFolderPath = localisationFolderPath;

            // TODO: Config file.
            CreateLocalisationFiles( localisationFolderPath );
            currentLanguage = localisationLanguages[LocalisationLanguage.English];
        }


        public string GetLocalString( string id )
        {
            Assert.IsTrue( currentLanguage != null );
            return currentLanguage.GetLocalString( id );
        }


        public string CurrentLanguageName
        {
            get
            {
                return currentLanguage.Language;
            }
            set
            {
                if( currentLanguage.Language != value )
                {
                    currentLanguage = localisationLanguages[value];
                    ContainingWorld.OnWorldLocalisationChanged();
                }
            }
        }


        public string LocalisationFolderPath
        {
            get
            {
                return localisationFolderPath;
            }
        }


        // Private:


        private string          localisationFolderPath      = "";

        private Dictionary<string, LocalisationLanguage>   localisationLanguages = new Dictionary<string, LocalisationLanguage>();
        private LocalisationLanguage                       currentLanguage       = null;


        private void CreateLocalisationFiles( string localisationFolderPath )
        {
            if( !Directory.Exists( localisationFolderPath ) )
            {
                throw new LocalisationSystemException( "Given localisation folder does not exist." );
            }

            var files           = Directory.GetFiles( localisationFolderPath );
            List<string> xmls   = new List<string>( files.Length );
            for( int i = 0; i < files.Length; ++i )
            {
                if( !files[i].EndsWith( LocalisationLanguage.Extension ) )
                {
                    continue;
                }
                xmls.Add( File.ReadAllText( files[i] ) );
            }

            // TODO: Make this a loop. Maybe an enum?
            localisationLanguages[LocalisationLanguage.English] = new LocalisationLanguage(
                LocalisationLanguage.English, xmls );
            localisationLanguages[LocalisationLanguage.Japanese] = new LocalisationLanguage(
                LocalisationLanguage.Japanese, xmls );
        }
        
    }


    public class LocalisationSystemException : Exception
    {
        public LocalisationSystemException() : base()
        {
        }


        public LocalisationSystemException( string msg ) : base( msg )
        {
        }


        public LocalisationSystemException( string msg, Exception inner ) : base( msg, inner )
        {
        }
    }
}
