using UnityEngine;
using Colyseus;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        Dictionary<string,object> data = new Dictionary<string, object>()
        {
            {"skins", _skins.length}
        };
        _room = await client.JoinOrCreate<State>(GameRoomName);
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
        snake.Init(player.d, mat);
        PlayerAim aim = Instantiate(_aim, position, Quaternion.identity);
        aim.Init(snake.Speed);

        Controller controller = Instantiate(_controllerPrefab);
        controller.Init(aim, player, snake);

        snake.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));
    }
    #endregion

    #region Enemy

    Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();
    private void CreateEnemy(string key, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);

        Material mat = _skins.GetMaterial(player.skin);

        Snake snake = Instantiate(_snakePrefab);
        snake.Init(player.d, mat);

        EnemyController enemy = snake.AddComponent<EnemyController>();
        enemy.Init(player, snake);
        enemy.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));
        _enemies.Add(key, enemy);
        
    }
    private void RemoveEnemy(string key, Player value)
    {
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
}
