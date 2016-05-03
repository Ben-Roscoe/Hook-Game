using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Nixin
{
    public class Team
    {
        public string LocalisedNameId { get; set; }
        public Int16 Score { get; set; }


        public Team( string localisedNameId )
        {
            this.LocalisedNameId = localisedNameId;
            this.Score  = 0;
        }
    }

    public enum TeamType
    {
        Red             = 0,
        Blue            = 1,
    }

    public class TeamDeathmatchGameState : HookGameMatchState
    {


        // Public:


        public override void OnActorInitialise( bool replicates, long networkOwner, bool acceptsNewConnections, Controller responsibleController )
        {
            base.OnActorInitialise( replicates, networkOwner, acceptsNewConnections, responsibleController );

            teams[( int )TeamType.Red] = new Team( LocalisationIds.TeamDeathmatch.RedTeamName );
            teams[( int )TeamType.Blue] = new Team( LocalisationIds.TeamDeathmatch.BlueTeamName );
        }


        public void ScoreKill( TeamType team )
        {
            ++teams[( int )team].Score;
        }


        public TeamType OtherTeam( TeamType team )
        {
            return ( TeamType )( ( ( int )team + 1 ) % 2 );
        }


        public Team GetWinner()
        {
            var red     = teams[( int )TeamType.Red];
            var blue    = teams[( int )TeamType.Blue];

            if( red.Score > blue.Score )
            {
                return red;
            }
            return blue;
        }


        public int GetTeamCount( TeamType team )
        {
            int count = 0;
            foreach( var x in ActivePlayerStats )
            {
                var stats = x as TeamDeathmatchPlayerStats;
                if( stats != null && stats.TeamType == team )
                {
                    ++count;
                }
            }
            return count;
        }


        public override SelectableType GetSelectableType( SelectableComponent selectable, SelectorComponent selector )
        {
            var selectableStats = selectable.Owner.ResponsibleStats;
            var selectorStats   = selector.Owner.ResponsibleStats;
            if( selectableStats == null || selectorStats == null )
            {
                return SelectableType.Neutral;
            }

            return IsOnSameTeam( selectableStats, selectorStats ) ? SelectableType.Good : SelectableType.Bad;
        }


        public bool IsOnSameTeam( Actor a, Actor b )
        {
            var statsA = a.ResponsibleStats as TeamDeathmatchPlayerStats;
            if( statsA == null )
            {
                return false;
            }

            var statsB = b.ResponsibleStats as TeamDeathmatchPlayerStats;
            if( statsB == null )
            {
                return false;
            }

            return statsA.TeamType == statsB.TeamType;
        }


        public Team GetTeam( TeamType team )
        {
            return teams[( int )team];
        }


        public override void WriteSnapshot( NetBuffer buffer )
        {
            base.WriteSnapshot( buffer );

            buffer.Write( teams[( int )TeamType.Red].Score );
            buffer.Write( teams[( int )TeamType.Blue].Score );
        }


        public override void ReadSnapshot( NetBuffer buffer, bool isFuture )
        {
            base.ReadSnapshot( buffer, isFuture );

            teams[( int )TeamType.Red].Score    = buffer.ReadInt16( teams[( int )TeamType.Red].Score, isFuture );
            teams[( int )TeamType.Blue].Score   = buffer.ReadInt16( teams[( int )TeamType.Blue].Score, isFuture );
        }


        // Private:


        private Team[]                      teams = new Team[2];
    }
}
