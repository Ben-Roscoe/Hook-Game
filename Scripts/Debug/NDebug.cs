using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nixin
{
    public static class NDebug
    {


        // Public:


        public static void PrintSubsystemDebug( DebugSubsystem subsystem, string message )
        {
            var info = GetInformationForSubSystem( subsystem );
            UnityEngine.Assertions.Assert.IsTrue( info.name != "" );

            string finalMsg = "<color=#" + ColorUtility.ToHtmlStringRGB( info.outputColour ) + ">" + info.name + ": " + message + "</color>";
            UnityEngine.Debug.Log( finalMsg );
        }


        // Private:


        public enum DebugSubsystem
        {
            Gameplay,
            Networking,
            Animation,
            Resources,
            Timing,
            AI,
        }


        private struct DebugSubsystemInformation
        {
            public string      name;
            public Color       outputColour;

            public DebugSubsystemInformation( string name, Color outputColour )
            {
                this.name               = name;
                this.outputColour       = outputColour;
            }
        }


        private static DebugSubsystemInformation GetInformationForSubSystem( DebugSubsystem subsystem )
        {
            switch( subsystem )
            {
                case DebugSubsystem.Gameplay:        return new DebugSubsystemInformation( "Gameplay", Color.green );
                case DebugSubsystem.Networking:      return new DebugSubsystemInformation( "Networking", Color.yellow );
                case DebugSubsystem.Animation:       return new DebugSubsystemInformation( "Animation", Color.red );
                case DebugSubsystem.Resources:       return new DebugSubsystemInformation( "Resources", Color.blue );
                case DebugSubsystem.Timing:          return new DebugSubsystemInformation( "Timing", Color.white );
                case DebugSubsystem.AI:              return new DebugSubsystemInformation( "AI", Color.cyan );
            }
            return new DebugSubsystemInformation();
        }
    }
}
