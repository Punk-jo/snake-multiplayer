using UnityEngine;
using Colyseus;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [field: SerializeField] public Skins _skins;
    #region Server
    private const string GameRoomName = "state_handler";

    private ColyseusRoom<State> _room;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeClient();
        Connection();
    }

    private async void Connection()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"skins", _skins.length},
            {"login", PlayerSettings.Instance.Login}
        };
        _room = await client.JoinOrCreate<State>(GameRoomName, data);
        _room.OnStateChange += OnChange;
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (isFirstState == false) return;
        _room.OnStateChange -= OnChange;

        state.players.ForEach((key, player) =>
        {
            if (key == _room.SessionId) CreatePlayer(player);
            else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;

        _room.State.apples.ForEach(CreateApple);

        _room.State.apples.OnAdd += (key, apple) => CreateApple(apple);
        _room.State.apples.OnRemove += RemoveApple;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        LeaveRoom();
    }
    public void LeaveRoom()
    {
        _room?.Leave();
    }
    public void SendMessage(string key, Dictionary<string, object> data)
    {
        _room.Send(key, data);
    }
    public void SendMessage(string key, string data)
    {
        _room.Send(key, data);
    }
    #endregion

    #region Player
    [SerializeField] private Controller _controllerPrefab;
    [SerializeField] private PlayerAim _aim;
    [SerializeField] private Snake _snakePrefab;
    private void CreatePlayer(Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);
        Quaternion quaternion = Quaternion.identity;
        Material mat = _skins.GetMaterial(player.skin);

        Snake snake = Instantiate(_snakePrefab, position, quaternion);
        snake.Init(player.d, mat,player.login, true);
        PlayerAim aim = Instantiate(_aim, position, Quaternion.identity);
        aim.Init(snake._head, snake.Speed);

        Controller controller = Instantiate(_controllerPrefab);
        controller.Init(_room.SessionId, aim, player, snake);

        snake.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));

        AddLeader(_room.SessionId, player);
    }
    #endregion

    #region Enemy

    Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();
    private void CreateEnemy(string key, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);

        Material mat = _skins.GetMaterial(player.skin);

        Snake snake = Instantiate(_snakePrefab);
        snake.Init(player.d, mat,player.login);

        EnemyController enemy = snake.AddComponent<EnemyController>();
        enemy.Init(key, player, snake);
        enemy.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));
        _enemies.Add(key, enemy);

        AddLeader(key, player);

    }
    private void RemoveEnemy(string key, Player value)
    {
        RemoveLeader(key);
        if (_enemies.ContainsKey(key) == false)
        {
            Debug.LogError("Попытка уничтожения енеми.");
            return;
        }
        EnemyController enemy = _enemies[key];
        _enemies.Remove(key);
        enemy.Destroy();
    }

    #endregion
    #region apple
    [SerializeField] private Apple _applePrefab;
    private Dictionary<Vector2Float, Apple> _apples = new Dictionary<Vector2Float, Apple>();
    private void CreateApple(Vector2Float vector2Float)
    {
        Vector3 position = new Vector3(vector2Float.x, 0, vector2Float.z);
        Apple apple = Instantiate(_applePrefab, position, Quaternion.identity);
        apple.Init(vector2Float);
        _apples.Add(vector2Float, apple);
    }
    private void RemoveApple(int key, Vector2Float vector2Float)
    {
        if (_apples.ContainsKey(vector2Float) == false) return;

        Apple apple = _apples[vector2Float];
        _apples.Remove(vector2Float);
        apple.Destroy();
    }
    #endregion

    #region LeaderBoard

    private class LoginScorePair
    {
        public string login;
        public float score;
    }
    [SerializeField] private TextMeshProUGUI _text;
    Dictionary<string, LoginScorePair> _leaders = new Dictionary<string, LoginScorePair>();
    private void AddLeader(string sessionID, Player player)
    {
        if (_leaders.ContainsKey(sessionID)) return;

        _leaders.Add(sessionID, new LoginScorePair
        {
            login = player.login,
            score = player.score
        });
        UpdateBoard();
    }
    private void RemoveLeader(string sessionID)
    {
        if(_leaders.ContainsKey(sessionID) == false) return;
        _leaders.Remove(sessionID);
        UpdateBoard();
    }

    public void UpdateScore(string sessionID, int score)
    {
        if (_leaders.ContainsKey(sessionID) == false) return;

        _leaders[sessionID].score = score;
        UpdateBoard();
    }

    private void UpdateBoard()
    {
        int topCount = Math.Clamp(_leaders.Count, 0 ,8);
        var top8 = _leaders.OrderByDescending(pair => pair.Value.score).Take(8);

        string text = "";
        int i = 1;
        foreach(var item in top8)
        {
            text += $"{i}.{item.Value.login}: {item.Value.score}\n";
            i++;
        }
        _text.text = text;
    }
    #endregion
}
