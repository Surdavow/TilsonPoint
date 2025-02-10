using Godot;
using System;

public partial class Game : Node3D
{
    public string ConnectionType;
    ENetMultiplayerPeer MultiplayerPeer = new ENetMultiplayerPeer();
    PackedScene PlayerScene = GD.Load<PackedScene>("res://modules/character/playercontroller.tscn");
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

        MultiplayerPeer.PeerConnected += (id) =>
        {
            GD.Print($"Connected to server with ID: {id}");
            // When a player connects, spawn their character on the host and client
            RpcId(id, nameof(SpawnPlayer), id);
        };
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
            // Clean up players
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

                Multiplayer.MultiplayerPeer = null;
                MultiplayerPeer = null;
            }

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
        // Ensure the player instance is created for each peer
        var playerInstance = PlayerScene.Instantiate<Node>();
        playerInstance.Name = "Player" + peerId.ToString();
        playerInstance.Set("multiplayer_authority", peerId);
        GetNode<Node>("Players").AddChild(playerInstance, true);
        GD.Print($"Spawned player with peer ID: {peerId}");
    }

    private void HostGame()
    {
        // Create server on port 1700
        Error err = MultiplayerPeer.CreateServer(1700);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create server: {err}");
            return;
        }

        MultiplayerPeer.PeerConnected += (id) =>
        {
            GD.Print($"Peer connected: {id}");

            // Check if the peer ID is valid before calling RpcId
            if (MultiplayerPeer.GetPeer((int)id) != null)
            {
                RpcId(id, nameof(SpawnPlayer), id);
            }
            else
            {
                GD.PrintErr($"Attempt to call RPC with unknown peer ID: {id}");
            }
        };

        // Host creates their own player
        SpawnPlayer(MultiplayerPeer.GetUniqueId()); // Host always has their unique peer ID
    }

    private void JoinGame()
    {
        Error err = MultiplayerPeer.CreateClient("localhost", 1700);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create client: {err}");
            return;
        }

        MultiplayerPeer.PeerConnected += (id) =>
        {
            GD.Print($"Peer connected: {id}");

            // Ensure the client spawns their own player
            if (MultiplayerPeer.GetPeer((int)id) != null)
            {
                RpcId(id, nameof(SpawnPlayer), id);
            }
            else
            {
                GD.PrintErr($"Attempt to call RPC with unknown peer ID: {id}");
            }
        };
    }

    private void RemovePlayer(long id)
    {
        var playerName = "Player" + id;
        var player = GetNode("Players").GetNodeOrNull<Node>(playerName);
        if (player != null)
        {
            player.QueueFree();
            GD.Print($"Removed player with peer ID: {id}");
        }
    }
}
