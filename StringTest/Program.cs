using System;

namespace StringTest
{
    class Program
    {
        const string log = "Scene 'ARMode_Dora' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded.\nTo add a scene to the build settings use the menu File->Build Settings...-UnityEngine.SceneManagement.SceneManager:LoadSceneAsync (string)\nLoading/<LoadScene>d__14:MoveNext () (at Assets/Scripts/Loading.cs:251)\nUnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)\n";

        const string a = "at Assets/Scripts/";
        static void Main(string[] args)
        {
            int index = log.IndexOf(a);
            var result = log.Substring(index + a.Length);
            result = result.Split('.')[0];

            Console.WriteLine("value: {0}, {1}", index, result);
        }
    }
}
