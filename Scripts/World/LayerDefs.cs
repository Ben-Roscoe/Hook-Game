using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public partial class LayerDefs
    {


        // Public:


        public const int Default                = 1 << 0;
        public const int TransparentFX          = 1 << 1;
        public const int IgnoreRaycast          = 1 << 2;
        public const int EmptyReserved1         = 1 << 3;
        public const int Water                  = 1 << 4;
        public const int UI                     = 1 << 5;
        public const int EmptyReserved2         = 1 << 6;
        public const int EmptyReserved3         = 1 << 7;

        public const int RealtimeLighting       = 1 << 8;
        public const int PotentialNavMesh       = 1 << 9;
        public const int Selectable             = 1 << 10;
        public const int AbilityTargetable      = 1 << 11;
        public const int QueryAbilityTargetable = 1 << 12;
        public const int Hook                   = 1 << 13;
        public const int HookCharacter          = 1 << 14;
        public const int FogOfWarBlocker        = 1 << 15;
        public const int FogOfWarHider          = 1 << 16;

        public const int DefaultPos                 = 0;
        public const int TransparentFXPos           = 1;
        public const int IgnoreRaycastPos           = 2;
        public const int EmptyReserved1Pos          = 3;
        public const int WaterPos                   = 4;
        public const int UIPos                      = 5;
        public const int EmptyReserved2Pos          = 6;
        public const int EmptyReserved3Pos          = 7;

        public const int RealtimeLightingPos        = 8;
        public const int PotentialNavMeshPos        = 9;
        public const int SelectablePos              = 10;
        public const int AbilityTargetablePos       = 11;
        public const int QueryAbilityTargetablePos  = 12;
        public const int HookPos                    = 13;
        public const int HookCharacterPos           = 14;
        public const int FogOfWarBlockerPos         = 15;
        public const int FogOfWarHiderPos           = 16;
    }
}
