using Godot;
using System;

public partial class Game : Node3D
{
    public string ConnectionType;
    ENetMultiplayerPeer MultiplayerPeer = new ENetMultiplayerPeer();
    PackedScene PlayerScene = GD.Load<PackedScene>("res://resource/character/PlayerController.tscn");
    private MultiplayerSpawner spawner;
    
    public override void _EnterTree()
    {
        var playersNode = new Node3D { Name = "Players" };
        AddChild(playersNode);

        spawner = new MultiplayerSpawner
        {
            Name = "PlayerSpawner"
        };
        AddChild(spawner);
        
        spawner.SpawnPath = new NodePath("../Players");
        spawner.AddSpawnableScene(PlayerScene.ResourcePath);
    }

    public override void _Ready()
    {
        MultiplayerPeer.PeerDisconnected += (id) => RemovePlayer(id);

        if (ConnectionType == "host")
        {
            HostGame();
        }
        else if (ConnectionType == "client")
        {
            JoinGame();
        }

        if (MultiplayerPeer.GetConnectionStatus() != ENetMultiplayerPeer.ConnectionStatus.Disconnected)
        {
            Multiplayer.MultiplayerPeer = MultiplayerPeer;
        }
    }

    public void DisconnectGame()
    {
        try 
        {
            // First, clean up all players
            var playersNode = GetNode<Node>("Players");
            foreach (Node player in playersNode.GetChildren())
            {
                player.QueueFree();
            }

            // Remove the spawner
            if (spawner != null)
            {
                spawner.QueueFree();
                spawner = null;
            }

            if (Multiplayer.MultiplayerPeer != null)
            {
                if (ConnectionType == "host")
                {
                    MultiplayerPeer.Close();
                }
                else if (ConnectionType == "client")
                {
                    if (Multiplayer.MultiplayerPeer.GetConnectionStatus() == ENetMultiplayerPeer.ConnectionStatus.Connected)
                    {
                        MultiplayerPeer.DisconnectPeer(Multiplayer.GetUniqueId());
                    }
                }
                
                // Important: Set to null before clearing the peer
                Multiplayer.MultiplayerPeer = null;
                MultiplayerPeer = null;
            }

            // Reset connection type
            ConnectionType = null;
            
            GD.Print("Successfully disconnected and cleaned up multiplayer resources");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error during disconnect: {e.Message}");
        }
    }

    public override void _ExitTree()
    {
        DisconnectGame();
        base._ExitTree();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SpawnPlayer(long peerId)
    {
        var playerInstance = PlayerScene.Instantiate<Node>();
        playerInstance.Name = "Player" + peerId.ToString();
        playerInstance.Set("multiplayer_authority", peerId);
        GetNode<Node>("Players").AddChild(playerInstance, true);
        GD.Print($"Spawned player with peer ID: {peerId}");
    }

    private void HostGame()
    {
        Error err = MultiplayerPeer.CreateServer(1700);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create server: {err}");
            return;
        }

        MultiplayerPeer.PeerConnected += (id) =>
        {
            GD.Print($"Peer connected: {id}");
            // Tell the newly connected peer to spawn all existing players
            foreach (Node player in GetNode<Node>("Players").GetChildren())
            {
                long playerId = player.Get("multiplayer_authority").As<long>();
                RpcId(id, nameof(SpawnPlayer), playerId);
            }
            // Tell all peers to spawn the new player
            Rpc(nameof(SpawnPlayer), id);
        };

        // Spawn the host's player
        SpawnPlayer(1); // Host is always peer ID 1
    }

    private void JoinGame()
    {
        Error err = MultiplayerPeer.CreateClient("localhost", 1700);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create client: {err}");
            return;
        }
    }

    private void RemovePlayer(long id)
    {
        var playerName = "Player" + id;
        var player = GetNode("Players").GetNodeOrNull<Node>(playerName);
        if (player != null)
        {
            player.QueueFree();
        }
    }
}