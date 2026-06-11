extends Control

#Return to main menu button 
@onready var back_button: Button = $BackButton
const Main_Menu = "res://Main Menu/MainMenu.tscn"


const correct_answer_needed = 2
const num_tile_scene = preload("res://Maths Games/Resources/number_tile.tscn") 

var correct_count = 0
var current_num = 0
var all_nums = []
var is_checking = false

@onready var game_progress: ProgressBar = $GameProgress
@onready var instructions: Label = $Instructions
@onready var round_label: Label = $RoundLabel
@onready var num_container: GridContainer = $"Num Container"
@onready var correct_or_wrong_label: Label = $CorrectOrWrongLabel

var num_instances = []

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameState.reset_search_game()
	
	correct_count = 0
	
	await get_tree().process_frame
	
	num_container.columns = 6
	
	for i in range(36):
		var num = num_tile_scene.instantiate()
		num.game_Manager = self
		num.custom_minimum_size = Vector2(80, 80)
		num.set_game_type("search")
		num_container.add_child(num)
		num_instances.append(num)
		
	num_container.add_theme_constant_override("h_separation", 10)
	num_container.add_theme_constant_override("v_separation", 10)
	
	await get_tree().process_frame
	
	for num in num_instances:
		num.origin_pos = num.global_position
	
	gen_new_round()
	game_progress.max_value = correct_answer_needed
	game_progress.value = 0
	
	back_button.pressed.connect(_on_back_button_pressed)


func gen_new_round() -> void:

	all_nums = []
	
	var min_range = 1
	var max_range = 100
	
	if correct_count > 0:
		if current_num < 50:
			min_range = 50
			max_range = 100
		else:
			min_range = 1
			max_range = 50
	
	for i in range(36):
		all_nums.append(randi() % 100 + 1)
		
	current_num = all_nums[randi() % all_nums.size()]
		
	for i in range(num_instances.size()):
		num_instances[i].set_number(all_nums[i])
	
	speak_num(current_num)

func speak_num(num: int) -> void:
	var toSpeak = "%d" % num
	DisplayServer.tts_speak(toSpeak, "en")
	await get_tree().create_timer(2.0).timeout
	DisplayServer.tts_speak(toSpeak, "en")

func _on_gui_input(event: InputEvent, num: Control) -> void:
	if is_checking:
		return
	
	if event is InputEventMouseButton and event.pressed:
		is_checking = true
		check_ans(num.tile_num)
		get_tree().root.set_input_as_handled()
		await get_tree().create_timer(0.2).timeout
		is_checking = false

func check_ans(selected_num: int) -> bool:
	if selected_num == current_num:
		correct_count += 1
		game_progress.value = correct_count
		round_label.text = "Round: %d / %d" % [correct_count, correct_answer_needed]
		
		show_correct_feedback()
		
		if correct_count >= correct_answer_needed:

			game_won()
		else:

			await get_tree().create_timer(1.0).timeout
			gen_new_round()
		return true
	else:
		show_wrong_feedback()
	return false

func show_correct_feedback() -> void:
	correct_or_wrong_label.text = "Correct"
	correct_or_wrong_label.add_theme_color_override("font_color", Color.GREEN)
	await get_tree().create_timer(0.5).timeout
	correct_or_wrong_label.text = ""
	correct_or_wrong_label.add_theme_color_override("font_color", Color.WHITE)

func show_wrong_feedback() -> void:
	GameState.search_mistakes += 1
	GameState.search_score -= 5
	
	correct_or_wrong_label.text = "Incorrect"
	correct_or_wrong_label.add_theme_color_override("font_color", Color.RED)
	
	#Blink Tile
	var correct_num = null
	for num in num_instances:
		if num.tile_num == current_num:
			correct_num = num
			break
	
	if correct_num:
		for i in range(3):
			var tween = create_tween()
			tween.tween_property(correct_num, "modulate:a", 0.3, 0.2)
			tween.tween_property(correct_num, "modulate:a", 1.0, 0.2)
	
	await get_tree().create_timer(0.5).timeout
	correct_or_wrong_label.text = ""
	correct_or_wrong_label.add_theme_color_override("font_color", Color.WHITE)

func game_won() -> void:
	GameState.add_search_score(50)
	GameState.search_lvls_completed += 1
	GameState.last_game_type = "Search"
	
	instructions.add_theme_color_override("font_color", Color.GREEN)
	instructions.text = "Good Job! Returning to Main Menu..."
	await get_tree().create_timer(2.0).timeout
	get_tree().change_scene_to_file("res://Main Menu/MainMenu.tscn")


func _on_back_button_pressed() -> void:
	get_tree().change_scene_to_file(Main_Menu)
