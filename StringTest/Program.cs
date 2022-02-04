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

            string str = "beautiful girl is sohee";
        }


        enum Sohee
        {
            pretty = 1000,
            cuty = 1001,
            beautiful = 1002,
            all = 2002

        }

        static void FindSohee(Sohee sohee, string memo, string script)
        {
            Console.WriteLine("Code: {0}, Content: {1}, Script: {2}", (int)sohee, memo, script);
        }
    }
}
