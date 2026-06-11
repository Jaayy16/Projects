extends Control

#return to main menu
@onready var back_button: Button = $BackButton
const Main_Menu = "res://Main Menu/MainMenu.tscn"

@onready var board_container: Container = $BoardContainer
@onready var board_sprite: Sprite2D = $board_sprite
@onready var turn_label: Label = $TurnLabel
@onready var player_name_label: Label = $PlayerNameLabel
@onready var status_label: Label = $StatusLabel
@onready var game_id_label: Label = $GameIDLabel

#Game Panel Elements
@onready var join_game_panel: Panel = $JoinGamePanel
@onready var join_game_input: LineEdit = $JoinGamePanel/JoinPanelContainer/JoinGameInput
@onready var join_button: Button = $JoinGamePanel/JoinPanelContainer/JoinButton
@onready var create_button: Button = $JoinGamePanel/JoinPanelContainer/CreateButton

@export var boardTexture: Texture2D
@export var XTexture: Texture2D
@export var OTexture: Texture2D

var board_pos = []
var piece_sprites = {}
var player_symbol = ""
var game_started = false

const board_layout = [
	Vector2(100, 100),
	Vector2(300, 100),
	Vector2(500, 100),
	Vector2(100, 300),
	Vector2(300, 300),
	Vector2(500, 300),
	Vector2(100, 500),
	Vector2(300, 500),
	Vector2(500, 500),
]

var selected_piece = -1
var placement_phase = true

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	if boardTexture:
		board_sprite.texture = boardTexture
		board_sprite.centered = true
		
		var screen_size = get_viewport_rect().size
		board_sprite.position = screen_size / 2
		
		board_sprite.z_index = 0

	create_board()
	board_container.z_index = 1
	
	if not create_button.is_connected("pressed", Callable(self, "_on_create_button_pressed")):
		create_button.pressed.connect(_on_create_button_pressed)
	if not join_button.is_connected("pressed", Callable(self, "_on_join_button_pressed")):
		join_button.pressed.connect(_on_join_button_pressed)
	
	if not MorrisGameManager.game_created.is_connected(_on_game_created):
		MorrisGameManager.game_created.connect(_on_game_created)
		
	if not MorrisGameManager.game_joined.is_connected(_on_game_joined):
		MorrisGameManager.game_joined.connect(_on_game_joined)
	
	if not MorrisGameManager.game_updated_state.is_connected(_on_game_updated_state):
		MorrisGameManager.game_updated_state.connect(_on_game_updated_state)
	
	if not MorrisGameManager.error_occurred.is_connected(_on_game_error):
		MorrisGameManager.error_occurred.connect(_on_game_error)
	
	game_started = false
	join_game_panel.visible = true
	board_container.visible = false
	
	if not back_button.is_connected("pressed", Callable(self, "_on_back_button_pressed")):
		back_button.pressed.connect(_on_back_button_pressed)

func create_board() -> void: 
	
	var screen_size = get_viewport_rect().size
	var board_offset = screen_size / 2 - Vector2(300, 300)
	
	for i in range(9):
		var pos_node = Control.new()
		pos_node.name = "Pos_%d" % i
		pos_node.position = board_layout[i] + board_offset
		pos_node.custom_minimum_size = Vector2(150, 150)
		pos_node.mouse_filter = Control.MOUSE_FILTER_STOP
		pos_node.pivot_offset = Vector2(75,75)
		
		var piece_sprite = Sprite2D.new()
		piece_sprite.name = "PieceSprite"
		pos_node.custom_minimum_size = Vector2(60, 60)
		piece_sprite.centered = true
		piece_sprite.position = Vector2(0,0)
		piece_sprite.z_index = 1
		pos_node.add_child(piece_sprite)
		
		piece_sprites[i] = piece_sprite
		
		pos_node.gui_input.connect(func(event: InputEvent): _on_pos_clicked(i, event))
		
		board_container.add_child(pos_node)
		board_pos.append(pos_node)

func _on_pos_clicked(pos: int, event: InputEvent) -> void:
	if not event is InputEventMouseButton or not event.pressed:
		return
		
	print("Clicked pos: ", pos)
	print("Your player_id: ", MorrisGameManager.player_id)
	print("Current turn: ", MorrisGameManager.current_turn)
	get_tree().root.set_input_as_handled()
	
	if MorrisGameManager.current_turn != MorrisGameManager.player_id:
		status_label.add_theme_color_override("font_color", Color.RED)
		status_label.text = "Not your turn! Current: %s, You: %s" % [MorrisGameManager.current_turn, MorrisGameManager.player_id]
		return
	
	if placement_phase:
		if MorrisGameManager.board[pos] == null:
			MorrisGameManager.place_piece(pos)
		else:
			status_label.add_theme_color_override("font_color", Color.RED)
			status_label.text = "Position taken!"
	else:
		if selected_piece == -1:
			if MorrisGameManager.board[pos] == player_symbol:
				selected_piece = pos
				_highlight_pos(pos)
				status_label.add_theme_color_override("font_color", Color.GREEN)
				status_label.text = "Selected piece at %d. Click Destination" % pos
			else:
				status_label.add_theme_color_override("font_color", Color.RED)
				status_label.text = "Thats not your piece"
		else:
			if pos == selected_piece:
				selected_piece = -1
				_clear_highlight()
				status_label.text = "Piece Deselected"
			elif MorrisGameManager.board[pos] == null:
				MorrisGameManager.move_piece(selected_piece, pos)
				selected_piece = -1
				_clear_highlight()
			else:
				status_label.add_theme_color_override("font_color", Color.RED)
				status_label.text = "Position is occupied"

func _highlight_pos(pos: int) -> void:
	var piece_sprite = piece_sprites[pos]
	piece_sprite.scale = Vector2(1.3, 1.3)

func _clear_highlight() -> void:
	for i in range(9):
		var piece_sprite = piece_sprites[i]
		piece_sprite.scale = Vector2(1.0, 1.0)

func _update_board_display() -> void:
	
	for i in range(9):
		var piece = MorrisGameManager.board[i]
		var sprite = piece_sprites[i]
		
		if piece == null:
			sprite.texture = null
		elif piece == "X":
			sprite.texture = XTexture
			if i != selected_piece:
				sprite.scale = Vector2(1.0, 1.0)
		elif piece == "O":
			sprite.texture = OTexture
			if i != selected_piece:
				sprite.scale = Vector2(1.0, 1.0)

		
	if MorrisGameManager.current_turn:
		var current_player_name = MorrisGameManager.players.get(MorrisGameManager.current_turn, {}).get("name", "unknown")
		turn_label.text = "Current Turn: %s" % current_player_name
	
	var piece_count = 0
	
	for piece in MorrisGameManager.board:
		if piece != null:
			piece_count += 1
	
	placement_phase = piece_count < 6

func _on_game_created() -> void:
	
	game_started = true
	join_game_panel.visible = false
	board_container.visible = true
	
	player_symbol = "X"
	
	status_label.add_theme_color_override("font_color", Color.WHITE)
	status_label.text = "Your Symbol is X. Waiting for opponent"
	
	game_id_label.text = "Game ID: " + MorrisGameManager.game_id
	game_id_label.add_theme_color_override("font_color", Color.YELLOW)
	
	player_name_label.text = "You: " + MorrisGameManager.players.get(MorrisGameManager.player_id, {}).get("name", "Player")
	player_name_label.add_theme_color_override("font_color", Color.RED)
	
	print("GameID Text: " + game_id_label.text)
 
func _on_game_joined() -> void:
	
	game_started = true
	join_game_panel.visible = false
	board_container.visible = true
	
	player_symbol = "O"
	
	status_label.add_theme_color_override("font_color", Color.WHITE)
	status_label.text = "Joined Game. Your Symbol is O"
	
	player_name_label.text = "You: " + MorrisGameManager.players.get(MorrisGameManager.player_id, {}).get("name", "Player")
	player_name_label.add_theme_color_override("font_color", Color.WHITE)

func _on_game_updated_state():
	#print("Update Board UI")
	_update_board_display()
	_update_status_label()

func _update_status_label() -> void:
	
	if MorrisGameManager.amt_of_players_in_game < 2:
		status_label.add_theme_color_override("font_color", Color.YELLOW)
		status_label.text = "Waiting for opponent to join"
	
	if MorrisGameManager.amt_of_players_in_game == 2:
		if MorrisGameManager.winner != "":
			if MorrisGameManager.winner == MorrisGameManager.player_id:
				status_label.add_theme_color_override("font_color", Color.GREEN)
				status_label.text = "You Won!"
				await get_tree().create_timer(2.0).timeout
				MorrisGameManager.leave_current_game()
				get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")
			else:
				status_label.add_theme_color_override("font_color", Color.RED)
				status_label.text = "You Lost"
				await get_tree().create_timer(2.0).timeout
				MorrisGameManager.leave_current_game()
				get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")
		elif MorrisGameManager.current_turn == MorrisGameManager.player_id:
			status_label.add_theme_color_override("font_color", Color.GREEN)
			status_label.text = "Your Turn"
		else:
			status_label.add_theme_color_override("font_color", Color.RED)
			status_label.text = "Opponenets Turn"

func _on_game_error(message: String):
	status_label.add_theme_color_override("font_color", Color.RED)
	status_label.text = "ErrorL %s" % message

func _on_create_button_pressed() -> void:
	print("Create game button pressed")
	MorrisGameManager.create_game("Player"+ str(randi_range(0 ,99)))

func _on_join_button_pressed() -> void:
	var game_id = join_game_input.text.strip_edges()
	
	if game_id == "":
		status_label.text = "Please Enter Game ID"
		return
	
	print("Join game button pressed with ID: ", game_id)
	MorrisGameManager.join_game(game_id, "Player"+ str(randi_range(0 ,99)))

func _on_back_button_pressed() -> void:
	MorrisGameManager.leave_current_game()
	get_tree().change_scene_to_file(Main_Menu)
