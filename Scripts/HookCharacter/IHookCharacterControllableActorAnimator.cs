using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixin
{
    public delegate void PostDeathCallback();
    public delegate void ThrowHookCallback();
    public delegate void HitMeleeCallback();

    public interface IHookCharacterControllableActorAnimator
    {
        void OnDeath( PostDeathCallback postDeathCallback );
        void OnStartThrowHook( ThrowHookCallback throwHookCallback );
        void OnStartMelee( HitMeleeCallback hitMeleeCallback );
    }
}
