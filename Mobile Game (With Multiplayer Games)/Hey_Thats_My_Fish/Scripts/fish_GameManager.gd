extends Node

var game_id: String = ""
var player_id: String = ""
var amt_of_players_in_game = 0
var current_turn: String = ""
var board: Dictionary = {}
var phase: String = ""
var winner: String = ""
var players: Dictionary = {}

var is_requesting = true
var socket_ready = false
var poll_timer = 0.0

const Api_Base_Url = "http://127.0.0.1:5000/api/"
const Socketio_Base_Url = "http://127.0.0.1:5000/socket.io"

var socket_client: SocketIOClient = null
var http_requests = {}

enum requestType {Create_game, Join_game, Get_State, Make_Move, Place_Penguin, Leave_Game}

signal game_created(response)
signal game_joined(response)
signal game_state_recieved(game_state)
signal move_response(response)
signal place_response(response)
signal socket_event_received(event_name, payload)
signal error_occurred(message)

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
	requestType.Place_Penguin:{
		"endpoint": "game/place_penguin",
		"method": HTTPClient.METHOD_POST
	},
	requestType.Leave_Game:{
		"endpoint": "game/leave_game",
		"method": HTTPClient.METHOD_POST
	}
}

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	print("Fish GameManager Ininitialized.")
	
	for request_Type in requestType.values():
		var http_request = HTTPRequest.new()
		add_child(http_request)
		http_request.request_completed.connect(_handle_response.bindv([request_Type]))
		http_requests[request_Type] = http_request
		
	error_occurred.connect(_on_error_occurred)
	is_requesting = false

func _process(delta: float) -> void:
	if game_id == "":
		return
		
	poll_timer += delta
	
	if poll_timer >= 1.0:
		poll_timer = 0.0
		get_state()

func create_game(player_name: String) -> void:
	
	print("Creating game with player ", player_name)
	
	var data = {
		"player_name": player_name,
		"game_type" : "fish"
	}
	_send_request(requestType.Create_game, data)

func join_game(game_id_input: String, player_name: String) -> void:
	
	var data = {
		"game_id": game_id_input,
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

func get_state() -> void:
	var request = http_requests.get(requestType.Get_State)
	
	var data = {
		"game_id": game_id,
	}

	if request.get_http_client_status() != HTTPClient.STATUS_DISCONNECTED:
		return
	
	_send_request(requestType.Get_State, data)

func place_penguin(pos: String) -> void:
	var data = {
		"game_id": game_id,
		"player_id": player_id,
		"pos": pos
	}
	_send_request(requestType.Place_Penguin, data)

func move_penguin(from_pos: String, to_pos: String) -> void:
	var data = {
		"game_id": game_id,
		"player_id": player_id,
		"from_pos": from_pos,
		"to_pos": to_pos
	}
	_send_request(requestType.Make_Move, data)

func _send_request(request_Type: requestType, data: Dictionary = {}) -> void:
	
	if not request_Def.has(request_Type):
		return
	
	if not request_Def.has(request_Type):
		error_occurred.emit("Invalid request type")
		return
	
	var request_info = request_Def[request_Type]
	var url = Api_Base_Url + request_info["endpoint"]
	var method = request_info["method"]
	var headers = ["Content-Type: application/json"]
	var json_data = ""
	
	if method == HTTPClient.METHOD_POST:
		json_data = JSON.stringify(data)
	
	var http_request = http_requests[request_Type]
	
	is_requesting = true
	var error = http_request.request(url, headers, method, json_data)
	
	if error != OK:
		is_requesting = false
		error_occurred.emit("Failed to send " + str(request_Type) + " request")

func _handle_response(result: int, response_code: int, header: PackedStringArray, body: PackedByteArray, request_type: requestType) -> void:
	
	is_requesting = false
	
	if result != HTTPRequest.RESULT_SUCCESS:
		error_occurred.emit("HTTP request failed with result code: " + str(result))
		return
	
	var body_string = body.get_string_from_utf8()
	var json = JSON.new()
	var parse_result = json.parse(body_string)
	
	if parse_result != OK:
		error_occurred.emit("Server Error (Code %d)" % response_code)
		return

	var resp = json.get_data()
	
	if resp == null:
		error_occurred.emit("Response is null")
		return
		
	match request_type:
		requestType.Create_game:
			_handle_create_game_response(resp)
		requestType.Join_game:
			_handle_join_game_response(resp)
		requestType.Get_State:
			_handle_get_state_response(resp)
		requestType.Make_Move:
			_handle_make_move_response(resp)
		requestType.Place_Penguin:
			_handle_place_penguin_response(resp)

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
		
		print("Game ID set: ", game_id)
		print("Player ID set: ", player_id)
		
		players[player_id] = {
			"name": response.get("player_name", "Player"),
			"symbol": "A",
			"score": 0
		}
		amt_of_players_in_game = 1
		current_turn = player_id
		
		game_created.emit(response)
		socket_init()
		
		await get_tree().create_timer(0.5).timeout
		get_state()
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
		"symbol": "B",
		"score": 0
		}
		
		amt_of_players_in_game = players.size()
		
		game_joined.emit(response)
		socket_init()
		
		if game_id != "":
			print("Getting gmae state for game_id: ", game_id)
			await get_tree().create_timer(0.5).timeout
			get_state()
	elif response.has("error"):
		error_occurred.emit(response["error"])
	else:
		error_occurred.emit("Invalid response from join game: " + str(response))

func _handle_get_state_response(response) -> void:
	
	is_requesting = false
		
	if response.has("game_state"):
		
		var game_state = response["game_state"]
				
		self.current_turn = game_state.get("current_turn", self.current_turn)
		self.players = game_state.get("players", {})
		self.phase = game_state.get("phase", "")
		self.amt_of_players_in_game = self.players.size()
	
		if game_state.has("board"):
			
			var raw_board = game_state["board"]
			self.board = {}
			
			if typeof(raw_board) == TYPE_DICTIONARY:
				for pos_key in raw_board.keys():
					var tile = raw_board[pos_key]
					self.board[pos_key] = {
						"fish": tile.get("fish", 0),
						"player": tile.get("player"),
						"visited": tile.get("visited", false)
					}
			elif typeof(raw_board) == TYPE_ARRAY:
				for i in range(raw_board.size()):
					self.board[str(i)] = raw_board[i]
				
		
		if game_state.get("winner") != null:
			winner = game_state["winner"]
			
		
		game_state_recieved.emit(response)
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
		get_state()
	else:
		error_occurred.emit(response["error"])
		return

func _handle_place_penguin_response(response) -> void:
	
	is_requesting = false
	
	if response == null:
		error_occurred.emit("Response is null")
		return
	
	if response.has("error"):
		error_occurred.emit(response["error"])
		return
	
	if response.has("status") and response["status"] == "success":
		print("Penguin Placed")
		get_state()
	else:
		error_occurred.emit("Invalid response format")
		return

func socket_init() -> void:	
	
	if socket_client != null:
		return
	
	socket_client = SocketIOClient.new(Socketio_Base_Url)
	socket_client.on_connect.connect(on_socket_connected)
	socket_client.on_disconnect.connect(on_socket_disconnected)
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

func on_socket_event(event_name: String, payload: Variant, _name_space) -> void:
	socket_event_received.emit(event_name, payload)

func _on_error_occurred(message) -> void:
	print("ERROR:", message)
