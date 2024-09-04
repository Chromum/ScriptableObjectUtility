using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScriptableObjectAttribute : Attribute
{
    string scriptablePath;

    public ScriptableObjectAttribute(string path)
    {
        scriptablePath = path;
    }

    public string scriptableObjectPath
    {
        get { return scriptablePath;}
    }
}
