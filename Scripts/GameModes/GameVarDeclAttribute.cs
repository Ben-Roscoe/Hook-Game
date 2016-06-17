using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class GameVarDeclAttribute : Attribute
    {


        // Public:

        public string Name { get; set; }
        public string NameToken { get; set; }
        public int    Ordering { get; set; }

        public GameVarDeclAttribute( int ordering, string name, string nameToken )
        {
            this.Name = name;
            this.NameToken = nameToken;

            this.Ordering = ordering;
        }
    }

    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class BoolGameVarDeclAttribute : GameVarDeclAttribute
    {


        // Public:

        public bool   DefaultValue { get; set; }
        
        public BoolGameVarDeclAttribute( int ordering, string name, string nameToken, bool defaultValue )
            : base( ordering, name, nameToken )
        {
            this.DefaultValue = defaultValue;
        }
    }


    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class IntGameVarDeclAttribute : GameVarDeclAttribute
    {


        // Public:

        public int  Min { get; set; }
        public int Max { get; set; }
        public int DefaultValue { get; set; }



        public IntGameVarDeclAttribute( int ordering, string name, string nameToken, int min, int max, int defaultValue )
            : base( ordering, name, nameToken )
        {
            this.DefaultValue = defaultValue;
            this.Min = min;
            this.Max = max;
        }
    }


    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class FloatGameVarDeclAttribute : GameVarDeclAttribute
    {


        // Public:

        public float Min { get; set; }
        public float Max { get; set; }
        public float DefaultValue { get; set; }



        public FloatGameVarDeclAttribute( int ordering, string name, string nameToken, float min, float max, float defaultValue )
            : base( ordering, name, nameToken )
        {
            this.DefaultValue = defaultValue;
            this.Min = min;
            this.Max = max;
        }
    }
}
