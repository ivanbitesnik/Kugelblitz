extends Node

func CreateClientPeer(id: int) -> void:
	var peer : MultiplayerPeer = SteamMultiplayerPeer.new()
	var OPTIONS_ARRAY : Array
	peer.create_client(id, 0, OPTIONS_ARRAY)
	
func CreateServerPeer() -> void:
	var peer : MultiplayerPeer = SteamMultiplayerPeer.new()
	var OPTIONS_ARRAY : Array
	peer.create_host(0, OPTIONS_ARRAY)
	multiplayer.multiplayer_peer = peer
