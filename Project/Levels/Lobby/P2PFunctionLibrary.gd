extends Node

func CreateClientPeer(id: int) -> void:
	var peer : MultiplayerPeer = SteamMultiplayerPeer.new()
	peer.create_client(id, 0)
	multiplayer.multiplayer_peer = peer
	
func CreateServerPeer() -> void:
	var peer : MultiplayerPeer = SteamMultiplayerPeer.new()
	peer.create_host(0)
	multiplayer.multiplayer_peer = peer
