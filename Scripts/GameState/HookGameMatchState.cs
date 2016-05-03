using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Nixin
{
    public class HookGameMatchState : GameState
    {


        // Public:


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( currentMatchState );
            buffer.Write( secondsUntilStart );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            currentMatchState = buffer.ReadByte( currentMatchState, isFuture );
            secondsUntilStart = buffer.ReadByte( secondsUntilStart, isFuture );
        }


        public virtual SelectableType GetSelectableType( SelectableComponent selectable, SelectorComponent selector )
        {
            return SelectableType.Neutral;
        }


        public byte CurrentMatchState
        {
            get
            {
                return currentMatchState;
            }
            set
            {
                currentMatchState = value;
            }
        }


        public byte SecondsUntilStart
        {
            get
            {
                return secondsUntilStart;
            }
            set
            {
                secondsUntilStart = value;
            }
        }


        // Private:


        private byte currentMatchState = MatchState.PreMatch;
        private byte secondsUntilStart = 0;
    }


    public class MatchState
    {
        public const byte PreMatch       = 0;
        public const byte StartCountdown = 1;
        public const byte InProgress     = 2;
        public const byte PostMatch      = 3;
        public const byte LoadingNextMap = 4;
    }
}
