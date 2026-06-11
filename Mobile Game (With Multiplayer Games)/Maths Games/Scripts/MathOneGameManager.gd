extends Node

#Return to main menu button 
@onready var back_button: Button = $BackButton
const Main_Menu = "res://Main Menu/MainMenu.tscn"

const correct_answer_needed = 5
const num_tile_scene = preload("res://Maths Games/Resources/number_tile.tscn") 

var correct_amt = 0
var current_ansr = 0
var num1 = 0
var num2 = 0
var answer_tiles = [] 

@onready var answer_container: HBoxContainer = $AnswerContainer
@onready var game_progress: ProgressBar = $GameProgress
@onready var answer_box: ColorRect = $AnswerBox
@onready var equation_label: Label = $EquationLabel

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameState.reset_addition_game()
	
	await get_tree().process_frame
	
	for i in range(3):
		var tile = num_tile_scene.instantiate()
		tile.game_Manager = self
		tile.custom_minimum_size = Vector2(100, 100)
		tile.set_game_type("addition")
		answer_container.add_child(tile)
		answer_tiles.append(tile)
	
	await get_tree().process_frame
	
	for tile in answer_tiles:
		tile.origin_pos = tile.global_position
	
	gen_new_equation()
	game_progress.max_value = correct_answer_needed
	game_progress.value = 0
	
	back_button.pressed.connect(_on_back_button_pressed)


func gen_new_equation() -> void:
	num1 = randi() % 10 + 1
	num2 = randi() % 10 + 1
	current_ansr = num1 + num2
	equation_label.text = "%d + %d = ?" % [num1 , num2]
	update_ansr_choices()

func update_ansr_choices() -> void:
	var ansr = gen_ansr_choices()
	ansr.shuffle()
	
	for i in range(answer_tiles.size()):
		answer_tiles[i].set_number(ansr[i])
		answer_tiles[i].reset_pos()

func gen_ansr_choices() -> Array:
	var ansrs = [current_ansr]
	
	while ansrs.size() < 3:
		var wrong_ansr = randi() % 20 + 1
		if wrong_ansr != current_ansr and wrong_ansr not in ansrs:
			ansrs.append(wrong_ansr)
	
	return ansrs

func check_ansr(selected_ansr) -> bool:
	if selected_ansr == current_ansr:
		correct_amt += 1
		game_progress.value = correct_amt
		
		if correct_amt >= correct_answer_needed:
			game_won()
		else:
			gen_new_equation()
		return true
	else:
		show_wrong_ansr_feedback()
	return false

func show_wrong_ansr_feedback() -> void:
	
	GameState.addition_mistakes += 1
	GameState.addition_score -= 5
	equation_label.add_theme_color_override("font_color", Color.RED)
	await get_tree().create_timer(0.5).timeout
	equation_label.add_theme_color_override("font_color", Color.WHITE)

func game_won() -> void:	
	
	GameState.add_addition_score(50)
	GameState.addition_lvls_completed += 1
	GameState.last_game_type = "Addition"
	
	equation_label.add_theme_color_override("font_color", Color.GREEN)
	equation_label.text = "Great Job! Returning to Main Menu..."
	await get_tree().create_timer(2.0).timeout
	get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")


func _on_back_button_pressed() -> void:
	get_tree().change_scene_to_file(Main_Menu)
