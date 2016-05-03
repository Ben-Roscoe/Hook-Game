using UnityEngine;
using System.Collections;
using System.IO;


namespace Nixin
{
    public class Utility
    {


        public static Texture2D TextureFromFile( string file )
        {
            Texture2D           texture = new Texture2D( 4, 4 );
            try
            { 
                using( FileStream fs = new FileStream( file, FileMode.Open, FileAccess.Read ) )
                {
                    byte[]          textureData = new byte[fs.Length];
                    fs.Read( textureData, 0, textureData.Length );
                    texture.LoadImage( textureData );
                }
            }
            catch
            {
                return null;
            }

            return texture;
        }
    }
}