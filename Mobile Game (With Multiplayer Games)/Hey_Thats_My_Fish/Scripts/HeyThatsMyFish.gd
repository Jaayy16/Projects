extends Node

#Return to main menu button 
@onready var back_button: Button = $BackButton
const Main_Menu = "res://Main Menu/MainMenu.tscn"

@onready var board_container: Container = $"Board Container"
@onready var turn_label: Label = $TurnLabel
@onready var player_name_label: Label = $PlayerNameLabel
@onready var status_label: Label = $StatusLabel
@onready var game_id_label: Label = $GameIDLabel

@onready var join_game_panel: Panel = $JoinGamePanel
@onready var join_button: Button = $JoinGamePanel/JoinPanelContainer/JoinButton
@onready var join_game_input: LineEdit = $JoinGamePanel/JoinPanelContainer/JoinGameInput
@onready var create_button: Button = $JoinGamePanel/JoinPanelContainer/CreateButton

@export var base_tile: Texture2D
@export var OneSmallFish: Texture2D
@export var TwoSmallFish: Texture2D
@export var OneMediumFish: Texture2D
@export var TwoMediumfish: Texture2D
@export var TwoLargefish: Texture2D
@export var ThreeMediumfish: Texture2D
@export var ThreeLargefish: Texture2D

@export var PenguinATexture: Texture2D
@export var PenguinBTexture: Texture2D

var game_started = false

const Hex_Size = 80
const Hex_V_Offset = Hex_Size * 0.75
const Hex_H_Offset = Hex_Size * 0.88
const Grid_Width = 8
const Grid_Height = 8

var board: Dictionary = {}
var players: Dictionary = {}
var current_turn: String = ""
var phase: String = ""
var winner: String = ""

var selected_penguin = ""
var available_moves = []

var hex_pos: Dictionary = {}
var tile_sprites: Dictionary = {}
var penguin_sprites: Dictionary = {}
var highlight_sprites: Dictionary = {}
var pos_to_index: Dictionary = {}
var index_to_pos: Dictionary = {}
 
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	if not create_button.is_connected("pressed", Callable(self, "_on_create_button_pressed")):
		create_button.pressed.connect(_on_create_button_pressed)
	if not join_button.is_connected("pressed", Callable(self, "_on_join_button_pressed")):
		join_button.pressed.connect(_on_join_button_pressed)
	
	if not FishGameManager.game_created.is_connected(_on_game_created):
		FishGameManager.game_created.connect(_on_game_created)
		
	if not FishGameManager.game_joined.is_connected(_on_game_joined):
		FishGameManager.game_joined.connect(_on_game_joined)
	
	if not FishGameManager.game_state_recieved.is_connected(_on_game_updated_state):
		FishGameManager.game_state_recieved.connect(_on_game_updated_state)
	
	if not FishGameManager.error_occurred.is_connected(_on_error_occurred):
		FishGameManager.error_occurred.connect(_on_error_occurred)
	
	if not back_button.is_connected("pressed", Callable(self, "_on_back_button_pressed")):
		back_button.pressed.connect(_on_back_button_pressed)
	
	join_game_panel.visible = true
	board_container.visible = false

func create_hex_grid() -> void:
	
	for child in board_container.get_children():
		child.queue_free()
	
	hex_pos.clear()
	tile_sprites.clear()
	penguin_sprites.clear()
	highlight_sprites.clear()
	pos_to_index.clear()
	index_to_pos.clear()
	
	var index = 0 
	
	var total_width = (Grid_Width - 1) * Hex_H_Offset
	var total_height = (Grid_Height - 1) * Hex_V_Offset
	
	var centre_offset = (board_container.size / 2.0) - Vector2(total_width / 2.0, total_height / 2.0)
	
	for j in range(Grid_Height):
		for i in range(Grid_Width):
			
			var pos_key = "%d,%d" % [i, j]
			var x = i * Hex_H_Offset
			
			if j % 2 == 1:
				x += Hex_H_Offset /2
			
			var y = j * Hex_V_Offset
			
			var hex_position = centre_offset + Vector2(x, y)
			
			hex_pos[pos_key] = hex_position
			pos_to_index[pos_key] = index
			index_to_pos[index] = pos_key
			index += 1
			
			
			var tile_button = TextureButton.new()
			tile_button.name = "Hex_%s" % pos_key
			tile_button.position = hex_position - Vector2(Hex_Size * 0.5, Hex_Size * 0.5)
			tile_button.custom_minimum_size = Vector2(Hex_Size, Hex_Size)
			tile_button.texture_normal = base_tile
			tile_button.ignore_texture_size = true
			tile_button.stretch_mode = TextureButton.STRETCH_SCALE
			
			#var tile_container = Control.new()
			#tile_container.name = "Hex_%s" % pos_key
			#tile_container.position = hex_position
			#tile_container.custom_minimum_size = Vector2(Hex_Size * 1.5, Hex_Size * 1.5)
			#tile_container.pivot_offset = Vector2(Hex_Size * 0.75, Hex_Size * 0.75)
			#tile_container.mouse_filter = Control.MOUSE_FILTER_PASS
			
			board_container.add_child(tile_button)
			
			#var tile_spr = Sprite2D.new()
			#tile_spr.name = "TileSprite_%s" % pos_key
			#tile_spr.texture = base_tile
			#tile_spr.centered = true
			#tile_spr.position = Vector2(Hex_Size, Hex_Size)
			#tile_spr.z_index = 0
			#tile_spr.scale = Vector2(1.0, 1.0)
			
			tile_sprites[pos_key] = tile_button
			
			#var click_area = Control.new()
			#click_area.name = "Clickarea"
			#click_area.custom_minimum_size = Vector2(Hex_Size * 2, Hex_Size * 2)
			#click_area.position = Vector2(0, 0)
			#click_area.mouse_filter = Control.MOUSE_FILTER_STOP
			#tile_container.add_child(click_area)
			
			tile_button.pressed.connect(func(): _on_tile_clicked(pos_key, null))

func _on_tile_clicked(pos_key: String, event) -> void:
	
	if FishGameManager.amt_of_players_in_game < 2:
		turn_label.text = "Waiting for opponent"
		return
	
	if FishGameManager.phase == "placement":
		_handle_placement_click(pos_key)
	elif FishGameManager.phase == "movement":
		_handle_movement_click(pos_key)
		
	if board.has(pos_key):
		print("Tile data: ", board[pos_key])
	else:
		print("Error: This postition not in board")
		print("First 10 board keys: ", board.keys().slice(0, 10))
		
		
	if current_turn != FishGameManager.player_id:
		status_label.add_theme_color_override("font_color", Color.RED)
		turn_label.text = "Not your turn"
		return

func _handle_placement_click(pos_key: String) -> void:
	
	var tile = board.get(pos_key, {})
	
	print("Clicking tile: ", pos_key)
	print("Tile data: ", tile)
	
	if tile.is_empty():
		status_label.add_theme_color_override("font_color", Color.RED)
		status_label.text = "Tile not found in board"
		return
	
	var fish_count = int(tile.get("fish", 0))
	
	if fish_count != 1:
		show_notification("Only 1-fish tile allowed!", Color.RED)
		_pulse_tile(pos_key, Color.RED)
		return
	
	if tile.get("visited", false):
		status_label.add_theme_color_override("font_color", Color.RED)
		status_label.text = "Tile Already visited"
		_pulse_tile(pos_key, Color.RED)
		return
		
	status_label.add_theme_color_override("font_color", Color.YELLOW)
	status_label.text = "Placing Penguin..."
	_pulse_tile(pos_key, Color.GREEN)
	FishGameManager.place_penguin(pos_key)

func _handle_movement_click(pos_key: String) -> void:
	
	var tile = board.get(pos_key, {})
	
	if selected_penguin == "":
		
		if tile.get("player") != FishGameManager.player_id:
			turn_label.add_theme_color_override("font_color", Color.RED)
			turn_label.text = "Not Your Penguin"
			return
		
		selected_penguin = pos_key
		_highlight_available_moves(pos_key)
		turn_label.add_theme_color_override("font_color", Color.GREEN)
		turn_label.text = "Penguin selected. Click new tile"
		_highlight_tile(pos_key, Color.CYAN)
		return
	
	if pos_key == selected_penguin:
		show_notification("Invalid Move!", Color.RED)
		_clear_highlights()
		return
	
	if pos_key not in available_moves:
		status_label.add_theme_color_override("font_color", Color.RED)
		status_label.text = "Invalid move"
		_pulse_tile(pos_key, Color.RED)
		return
	
	turn_label.add_theme_color_override("font_color", Color.YELLOW)
	turn_label.text = "Moving Penguin..."
	_pulse_tile(pos_key, Color.GREEN)
	FishGameManager.move_penguin(selected_penguin, pos_key)
	selected_penguin = ""
	available_moves = []
	_clear_highlights()

func _highlight_available_moves(from_pos: String) -> void:
	
	available_moves = _get_adjacent_tiles(from_pos)
	
	for pos_key in available_moves:
		_highlight_tile(pos_key, Color.YELLOW)

func _get_adjacent_tiles(pos_key: String) -> Array:
	
	var parts: PackedStringArray = pos_key.split(",")
	var fi: int = int(parts[0].strip_edges())
	var fj: int = int(parts[1].strip_edges())

	var neighbors = []
	
	var neighbor_offsets = [
		Vector2i(1,0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1), Vector2i(1, -1), Vector2i(-1, 1)
	]
	
	for offset in neighbor_offsets:
		
		var ni = fi + offset.x
		var nj = fj + offset.y
		
		var neighbor_key = "%d,%d" % [ni, nj]
		
		if neighbor_key not in board:
			continue
			
		var tile = board[neighbor_key]
		
		if not tile.get("visited", false) and tile.get("player") == null:
			neighbors.append(neighbor_key)
		
	return neighbors

func _highlight_tile(pos_key: String, color: Color) -> void:
	
	if pos_key not in hex_pos:
		return
		
	if pos_key in highlight_sprites:
		highlight_sprites[pos_key].queue_free()
		
	var tile_button = tile_sprites.get(pos_key)
	if not tile_button:
		return
	
	var highlight = Sprite2D.new()
	highlight.name = "Highlight_%s" % pos_key
	highlight.texture = base_tile
	highlight.centered = true
	highlight.position = Vector2(Hex_Size * 0.5, Hex_Size * 0.5)
	highlight.z_index = 1
	highlight.self_modulate = color
	highlight.scale = Vector2(0.25, 0.25)
	
	tile_button.add_child(highlight)
	highlight_sprites[pos_key] = highlight
	
	#var tile_container = board_container.get_node_or_null("Hex_%s" % pos_key)
	#if tile_container:
		#tile_container.add_child(highlight)
		#highlight_sprites[pos_key] = highlight

func _pulse_tile(pos_key: String, color: Color) -> void:
	
	if pos_key not in hex_pos:
		return
	
	var tile_button = tile_sprites.get(pos_key)
	if not tile_button:
		return
	
	var pulse = Sprite2D.new()
	pulse.texture = base_tile
	pulse.centered = true
	pulse.position = Vector2(Hex_Size * 0.5, Hex_Size * 0.5)
	pulse.z_index = 1
	pulse.self_modulate = color
	pulse.scale = Vector2(0.25, 0.25)
	
	tile_button.add_child(pulse)
	
	var tween = create_tween()
	tween.tween_property(pulse, "scale", Vector2(0.5, 0.5), 0.1)
	tween.tween_property(pulse, "scale", Vector2(0.25, 0.25), 0.1)
	tween.tween_callback(pulse.queue_free)

func _clear_highlights() -> void:
	
	for pos_key in highlight_sprites.keys():
		if highlight_sprites[pos_key]:
			highlight_sprites[pos_key].queue_free()
	
	highlight_sprites.clear()

func _update_board_display() -> void:
	for pos_key in board:
		var tile = board[pos_key]
		_update_tile_spr(pos_key, tile)

func _update_tile_spr(pos_key: String, tile: Dictionary) -> void:
	
	if pos_key not in tile_sprites:
		return
	
	var tile_button = tile_sprites[pos_key]
	var penguin_id = tile.get("player")
	var is_visited = tile.get("visited", false)
	
	if is_visited:
		tile_button.modulate = Color.DARK_GRAY
	else:
		tile_button.modulate = Color.WHITE
		
	var fish_count = int(tile.get("fish", 1))

	var seed_value = hash(pos_key)
	seed(seed_value)
	
	match fish_count:
		1:
			var one_fish_textures = [OneSmallFish, OneMediumFish]
			tile_button.texture_normal = one_fish_textures[randi() % one_fish_textures.size()]
		2:
			var two_fish_textures = [TwoSmallFish, TwoMediumfish, TwoLargefish]
			tile_button.texture_normal = two_fish_textures[randi() % two_fish_textures.size()]
		3:
			var three_fish_textures = [ThreeMediumfish, ThreeLargefish]
			tile_button.texture_normal = three_fish_textures[randi() % three_fish_textures.size()]
		_:
			tile_button.texture_normal = base_tile
	
	for child in tile_button.get_children():
		if str(child.name).begins_with("Penguin_"):
			child.queue_free()
			
			for key in penguin_sprites.keys():
				if key.ends_with("_" + pos_key):
					penguin_sprites.erase(key)
	
	if penguin_id != null and not is_visited:
		_create_penguin_sprite(penguin_id, pos_key, tile_button)

func _create_penguin_sprite(player_id: String, pos_key: String, parent_node: Node) -> void:
	
	var old_penguin_key = "%s_%s" % [player_id, pos_key]
	if old_penguin_key in penguin_sprites:
		penguin_sprites[old_penguin_key].queue_free()
		penguin_sprites.erase(old_penguin_key)
	
	var penguin_sprite = Sprite2D.new()
	penguin_sprite.name = "Penguin_%s_%s" % [player_id, pos_key]
	penguin_sprite.centered = true
	penguin_sprite.position = Vector2(Hex_Size * 0.5, Hex_Size * 0.5)
	penguin_sprite.z_index = 1
	penguin_sprite.scale = Vector2(0.6, 0.6)
	
	var player = players.get(player_id, {})
	if player.get("symbol") == "A":
		penguin_sprite.texture = PenguinATexture
	else:
		penguin_sprite.texture = PenguinBTexture
	
	parent_node.add_child(penguin_sprite)
	
	var penguin_key = "%s_%s" % [player_id, pos_key]
	penguin_sprites[penguin_key] = penguin_sprite

func _on_game_created(response: Dictionary) -> void:
	
	game_started = true
	join_game_panel.visible = false
	board_container.visible = true
	
	turn_label.add_theme_color_override("font_color", Color.WHITE)
	turn_label.text = "You are Penguin A"
	
	game_id_label.text = "Game ID: " + FishGameManager.game_id
	game_id_label.add_theme_color_override("font_color", Color.YELLOW)
	
	player_name_label.text = "You:  " + FishGameManager.players.get(FishGameManager.player_id, {}).get("name", "Player")
	player_name_label.add_theme_color_override("font_color", Color.WHITE)
	
	create_hex_grid()
	
	FishGameManager.get_state()

func _on_game_joined(response: Dictionary) -> void:
	
	game_started = true
	join_game_panel.visible = false
	board_container.visible = true
	
	turn_label.add_theme_color_override("font_color", Color.WHITE)
	turn_label.text = "You are Penguin B"
	
	player_name_label.text = "You:  " + FishGameManager.players.get(FishGameManager.player_id, {}).get("name", "Player")
	player_name_label.add_theme_color_override("font_color", Color.WHITE)
	
	create_hex_grid()
	
	FishGameManager.get_state()

func _on_game_updated_state(response: Dictionary) -> void:
	board = FishGameManager.board
	players = FishGameManager.players
	current_turn = FishGameManager.current_turn
	phase = FishGameManager.phase
	winner = FishGameManager.winner
	
	_update_board_display()
	_update_status_label()

func _update_status_label() -> void:
	
	var players_count = len(players)
	
	var score_text = ""
	
	for p_id in players:
		var p = players[p_id]
		score_text += "%s: %d | " % [p.get("name", "Player"), int(p.get("score", 0))]
	
	if players_count < 2:
		turn_label.add_theme_color_override("font_color", Color.YELLOW)
		turn_label.text = "Waiting for opponenet to join"
	
	elif players_count == 2:
		
		if winner != "":
			if winner == "draw":
				status_label.text = "Its a Drase"
			elif winner == FishGameManager.player_id:
				status_label.add_theme_color_override("font_color", Color.GREEN)
				var winner_player = players[winner]
				status_label.text = "You Won"
				await get_tree().create_timer(2.0).timeout
				FishGameManager.leave_current_game()
				get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")
			
			else:
				
				status_label.add_theme_color_override("font_color", Color.RED)
				var winner_player = players[winner]
				status_label.text = "You Lost"
				await get_tree().create_timer(2.0).timeout
				FishGameManager.leave_current_game()
				get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")
		
		elif current_turn == FishGameManager.player_id:
			
			if phase == "placement":
				turn_label.add_theme_color_override("font_color", Color.GREEN)
				turn_label.text = "Your Turn - Place Penguin on tile"
				status_label.text = "Score: " + score_text
			else:
				turn_label.add_theme_color_override("font_color", Color.RED)
				turn_label.text = "Your Turn - Move Penguin"
				status_label.text = "Score: " + score_text
				
		else:
			turn_label.add_theme_color_override("font_color", Color.RED)
			turn_label.text = "Opponent's Turn"
			status_label.add_theme_color_override("font_color", Color.WHITE)
			status_label.text = "Score: " + score_text

func show_notification(message: String, color: Color = Color.WHITE) -> void:
	status_label.add_theme_color_override("font_color", color)
	status_label.text = message
	
	await get_tree().create_timer(1.5).timeout
	
	_update_status_label()

func _on_error_occurred(message: String) -> void:
	status_label.add_theme_color_override("font_color", Color.RED)
	status_label.text = "Error: %s" % message

func _on_create_button_pressed() -> void:
	FishGameManager.create_game("Player"+ str(randi_range(0 ,99)))

func _on_join_button_pressed() -> void:
	var game_id = join_game_input.text.strip_edges()
	
	if game_id == "":
		status_label.text = "Please Enter Game ID"
		return
	
	FishGameManager.join_game(game_id, "Player"+ str(randi_range(0 ,99)))

func _on_back_button_pressed() -> void:
	FishGameManager.leave_current_game()
	get_tree().change_scene_to_file(Main_Menu)
