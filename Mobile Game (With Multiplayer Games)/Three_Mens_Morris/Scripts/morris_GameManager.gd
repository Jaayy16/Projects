extends Node

var game_id: String = ""
var player_id: String = ""
var amt_of_players_in_game = 0
var current_turn: String = ""
var board: Array = []
var winner: String = ""
var players: Dictionary = {}
var game_type = ""

var is_requesting = true
var socket_ready = false
var poll_timer = 0.0

const Api_Base_Url = "http://127.0.0.1:5000/api/"
const Socketio_Base_Url = "http://127.0.0.1:5000/socket.io"

var socket_client: SocketIOClient = null

enum requestType {Create_game, Join_game, Get_State, Make_Move, Leave_Game}

signal game_created
signal game_joined
signal game_list_recieved(game_list)
signal game_updated_state
signal error_occurred(message)

var http_Requests = {}

var request_Def = {
	requestType.Create_game:{
		"endpoint": "game/create_game",
		"method": HTTPClient.METHOD_POST
	},
	requestType.Join_game:{
		"endpoint": "game/join_game",
		"method": HTTPClient.METHOD_POST
	},
	requestType.Get_State:{
		"endpoint": "game/get_game_state",
		"method": HTTPClient.METHOD_POST
	},
	requestType.Make_Move:{
		"endpoint": "game/make_move",
		"method": HTTPClient.METHOD_POST
	},
	requestType.Leave_Game:{
		"endpoint": "game/leave_game",
		"method": HTTPClient.METHOD_POST
	}
}

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	print("Morris GameManager Ininitialized.")
	
	for request_Type in requestType.values():
		var http_request = HTTPRequest.new()
		add_child(http_request)
		
		match request_Type:
			requestType.Create_game:
				is_requesting = false
				http_request.request_completed.connect(_on_create_game_request_completed)
			requestType.Join_game:
				is_requesting = false
				http_request.request_completed.connect(_on_join_game_request_completed)
			requestType.Get_State:
				is_requesting = false
				http_request.request_completed.connect(_on_get_state_request_completed)
			requestType.Make_Move:
				is_requesting = false
				http_request.request_completed.connect(_on_make_move_request_completed)
	
		http_Requests[request_Type] = http_request
	
	error_occurred.connect(_on_error_occurred)

func _process(delta: float) -> void:
	
	if game_id == "" or winner != "":
		return
		
	poll_timer += delta
	var poll_interval = 0.5 if amt_of_players_in_game == 2 else 2.0
	
	if poll_timer >= poll_interval:
		poll_timer = 0.0
		get_state(game_id)

func socket_init() -> void:	
	
	if socket_client != null:
		return
	
	socket_client = SocketIOClient.new(Socketio_Base_Url)
	socket_client.on_connect.connect(on_socket_connected)
	socket_client.on_disconnect.connect(on_socket_disconnected)
	socket_client.on_disconnect.connect(on_socket_connection_lost)
	socket_client.on_reconnected.connect(on_socket_reconnected)
	socket_client.on_event.connect(on_socket_event)
	
	add_child(socket_client)
	
	await get_tree().process_frame
	socket_client.socketio_connect()
	
	await get_tree().create_timer(1.0).timeout

func on_socket_connected(_payload: Variant, _name_space, error: bool) -> void:
	if error:
		socket_ready = false
		return
	
	socket_ready = true
	
	await get_tree().process_frame
	if game_id != "" and player_id != "":
		var data = {
			"game_id": game_id,
			"player_id": player_id
		}
		
		if socket_ready:
			socket_client.socketio_send("join_game", data)
		else:
			print("Socket not ready yet, will retry...")

func on_socket_disconnected(name_space: String) -> void:
	print("Socket.Io disconnected")

func on_socket_connection_lost() -> void:
	print("Socket.IO connection lost - attempting reconnect")
	error_occurred.emit("Connection lost, reconnecting")

func on_socket_event(event_name: String, payload: Variant, _name_space) -> void:
	
	print("Recieved ", event_name, " ", payload)
	
	if event_name == "game_state_changed":
		get_state(payload["game_id"])
	print ("Received event: ", event_name, " with payload ", payload)
	
	match event_name:
		"game_state_changed":
			if payload and payload.has("game_id"):
				print("Game state changed, fetching new state for: ", payload["game_id"])
				get_state(payload["game_id"])
		"player_joined":
			if payload and payload.has("game_id"):
				print("Player Joined, fetching new state for: ", payload["game_id"])
			get_state(payload["game_id"])

func on_socket_reconnected(_payload: Variant, _name_space, error: bool) -> void:
	print("SocketIo Reconnected")
	if not error:
		if game_id != "" and player_id != "":
			var data = {
				"game_id": game_id,
				"player_id": player_id
			}
			socket_client.socketio_send("join_game", data)
		get_state(game_id)

func _on_create_game_request_completed(result, response_code, headers, body) -> void:
	_handle_response(requestType.Create_game, result, response_code, headers, body)

func _on_join_game_request_completed(result, response_code, headers, body) -> void:
	_handle_response(requestType.Join_game, result, response_code, headers, body)

func _on_get_state_request_completed(result, response_code, headers, body) -> void:
	_handle_response(requestType.Get_State, result, response_code, headers, body)

func _on_make_move_request_completed(result, response_code, headers, body) -> void:
	_handle_response(requestType.Make_Move, result, response_code, headers, body)

func _handle_response(request_type: requestType, result: int, response_code: int, header: Array, body: PackedByteArray) -> void:
	
	is_requesting = false
	
	#print("Response received - Type: ", requestType, " Code: ", response_code, " Result: ", result)
	#print("Body: ", body.get_string_from_utf8())
	
	if result != HTTPRequest.RESULT_SUCCESS:
		error_occurred.emit("HTTP request faild ith resutl code: " + str(result))
		return
	
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var resp = json.get_data()
	
	if resp == null:
		error_occurred.emit("Response is null")
		return
	
	#print("Parsed Response: ", resp)
	
	match request_type:
		requestType.Create_game:
			_handle_create_game_response(resp)
		requestType.Join_game:
			_handle_join_game_response(resp)
		requestType.Get_State:
			_handle_get_state_response(resp)
		requestType.Make_Move:
			_handle_make_move_response(resp)

func _send_request(request_Type: requestType, data: Dictionary = {}) -> void:
	
	if not request_Def.has(request_Type):
		return
	
	if is_requesting:
		print("Request already in progress, skipping")
		return
	
	var request_info = request_Def[request_Type]
	var url = Api_Base_Url + request_info["endpoint"]
	var method = request_info["method"]
	var headers = ["Content-Type: application/json"]
	var json_data = ""
	
	if method == HTTPClient.METHOD_POST:
		json_data = JSON.stringify(data)
	
	var http_request = http_Requests[request_Type]
	
	is_requesting = true
	var error = http_request.request(url, headers, method, json_data)
	
	if error != OK:
		is_requesting = false
		error_occurred.emit("Failed to send " + str(request_Type) + " request")

func create_game(player_name: String) -> void:
	
	print("create_game called with: ", player_name)
	
	var data = {
		"player_name": player_name,
		"game_type" : "morris"
	}
	_send_request(requestType.Create_game, data)

func join_game(game_id: String, player_name: String) -> void:
	var data = {
		"game_id": game_id,
		"player_name": player_name
	}
	_send_request(requestType.Join_game, data)

func leave_current_game() -> void:
	if game_id == "": return
	
	var data = {"game_id": game_id}
	_send_request(requestType.Leave_Game, data)
	
	game_id = ""
	player_id = ""
	board.clear()


func get_state(game_id: String) -> void:
	var data = {
		"game_id": game_id,
	}
	_send_request(requestType.Get_State, data)

func place_piece(position: int) -> void:
	var data = {
		"game_id": game_id,
		"player_id": player_id,
		"pos" : position
	}
	_send_request(requestType.Make_Move, data)

func move_piece(from_pos: int, to_pos: int) -> void:
	var data = {
		"game_id": game_id,
		"player_id": player_id,
		"from_pos": from_pos,
		"to_pos": to_pos
	}
	_send_request(requestType.Make_Move, data)

func _handle_create_game_response(response) -> void:
	
	is_requesting = false
	
	print("_handle_create_game_response called with: ", response)
	
	if response == null:
		error_occurred.emit("Response is null")
		return
	
	if response.has("game_id") and response.has("player_id"):
		game_id = response["game_id"]
		player_id = response["player_id"]#
		
		players[player_id] = {
			"name": response.get("player_name", "Player"),
			"symbol": "X"
		}
		amt_of_players_in_game = 1
		current_turn = player_id
		
		game_created.emit()
		socket_init()
		
		await get_tree().create_timer(0.5).timeout
		get_state(game_id)
	else:
		error_occurred.emit("Invalid response from create game")

func _handle_join_game_response(response) -> void:
	
	is_requesting = false
	
	if response == null:
		error_occurred.emit("Response is null")
		return
	
	if response.has("player_id"):
		player_id = response["player_id"]
		
		if response.has("game_id"):
			game_id = response["game_id"]
			
		players[player_id] = {
		"name": response.get("player_name", "Player"),
		"symbol": "O"
		}
		
		amt_of_players_in_game = players.size()
		
		game_joined.emit()
		socket_init()
		
		if game_id != "":
			print("Getting gmae state for game_id: ", game_id)
			await get_tree().create_timer(0.5).timeout
			get_state(game_id)

	elif response.has("error"):
		error_occurred.emit(response["error"])
	else:
		error_occurred.emit("Invalid response from join game: " + str(response))

func _handle_get_state_response(response) -> void:
	
	is_requesting = false
	
	if response.has("game_state"):
		
		var game_state = response["game_state"]
		self.current_turn = game_state.get("current_turn", self.current_turn)
		self.board = game_state.get("board", self.board)
		self.players = game_state.get("players", {})
		self.amt_of_players_in_game = self.players.size()
		
		if game_state.get("winner") != null:
			winner = game_state["winner"]
			game_updated_state.emit()
			return
		
		game_updated_state.emit()
	else:
		error_occurred.emit("Invalid response from get state: " + str(response))

func _handle_make_move_response(response) -> void:
	
	is_requesting = false
	
	print("make move resp: ", response)
	if response == null:
		error_occurred.emit("Response is null")
		return
	
	if response.has("status") and response["status"] == "success":
		print("Move successful, waiting for socket.io update")
		get_state(game_id)
	else:
		error_occurred.emit(response["error"])
		return

func _on_error_occurred(message) -> void:
	print("ERROR:", message)
