using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Ideally, classes that subscribe to BlackOverlayEvents should unsubscribe immediately after their callback is called, in order to prevent conflicts with other game behaviors.
 */
public class ShowBlackOverlayEvent : GameEvent
{
    
}

public class HideBlackOverlayEvent : GameEvent
{

}

public class TransitionOverBlackOverlayEvent : GameEvent
{

}