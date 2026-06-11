extends Control

@export var sprite_2d: Sprite2D
@export var num_label: Label

var tile_num = 0
var is_beingDragged = false
var origin_pos = Vector2.ZERO
var game_Manager: Node
var is_clickable = true
var game_type = "addition"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	gui_input.connect(_on_gui_input)

func _process(_delta) -> void:
	if is_beingDragged and is_clickable:
		global_position = get_global_mouse_position() - size / 2

func set_number(newNumber: int) -> void:
	tile_num = newNumber
	num_label.text = str(newNumber)

func set_game_type(type: String):
	game_type = type

func _on_gui_input(event: InputEvent) -> void:
	if not is_clickable:
		return
	
	if event is InputEventMouseButton:
		if event.pressed:
			if game_type == "addition":
				is_beingDragged = true
				z_index = 100
			elif game_type == "search":
				game_Manager.check_ans(tile_num)
		else:
			is_beingDragged = false
			check_if_placed()
			z_index = 0

#for Addition Game
func check_if_placed() -> void:
	if game_type != "addition":
		return
		
	var answer_box = game_Manager.answer_box
	
	if answer_box:
		var tile_rect = get_global_rect()
		var answer_box_rect = answer_box.get_global_rect()
		
		if answer_box_rect.has_point(tile_rect.get_center()):
			game_Manager.check_ansr(tile_num)
			reset_pos()
		else:
			reset_pos()
	else:
		reset_pos()

func reset_pos() -> void:
	var tween = create_tween()
	tween.tween_property(self, "global_position", origin_pos, 0.2)
	is_beingDragged = false

#For Number Search Game
func on_tile_clicked():
	if is_clickable:
		game_Manager.check_answer(tile_num)
