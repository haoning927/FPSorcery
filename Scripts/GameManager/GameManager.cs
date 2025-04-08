using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton; // NetworkManager.Singleton 可以直接调用是因为在unity engine

    [SerializeField]
    public MatchingSettings MatchingSettings;

    private Dictionary<string, Player> players = new Dictionary<string, Player>();

    private void Awake()
    {
        Singleton = this;
    }

    public void RegisterPlayer (string name, Player player)
    {
        player.transform.name = name;
        players.Add(name, player);
    }

    public void UnregisterPlayer (string name)
    {
        players.Remove(name);
    }

    public Player GetPlayer(string name)
    {
        return players[name];
    }

    private static string info;

    public static void UpdateInfo(string _info)
    {
        info = _info;
    }

    //private void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(200f, 200f, 200f, 400f));
    //    GUILayout.BeginVertical();

    //    GUIStyle myStyle = new GUIStyle();
    //    myStyle.fontSize = 24;
    //    myStyle.normal.textColor = Color.red;
    //    //GUI.color = Color.red;

    //    //GUILayout.Label(info);
    //    foreach (string name in players.Keys)
    //    {
    //        Player player = GetPlayer(name);
    //        GUILayout.Label(name + " - " + player.GetHealth(), myStyle);
    //    }

    //    GUILayout.EndVertical();
    //    GUILayout.EndArea();
    //}
}
