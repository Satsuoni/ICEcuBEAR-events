using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class PreloadSigningAlias
{

    static PreloadSigningAlias()
    {
        PlayerSettings.Android.keystorePass = "password";
        PlayerSettings.Android.keyaliasName = "icecube";
        PlayerSettings.Android.keyaliasPass = "password";
    }

}
