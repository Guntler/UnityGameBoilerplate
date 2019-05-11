using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterInputEvent : GameEvent
{
    public string KeyName = "";
    public string Action = "";

    public RegisterInputEvent(List<string> key, string action) : base()
    {

    }
}
