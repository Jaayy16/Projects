extends Node

@onready var addition_game: Button = $AdditionGame
@onready var number_search_game: Button = $NumberSearchGame
@onready var three_mens_morris: Button = $ThreeMensMorris
@onready var hey_thats_my_fish: Button = $HeyThatsMyFish
@onready var stats_label: Label = $StatsLabel

const MATHS_GAME_ONE = "res://Maths Games/Scenes/MathsGameOne.tscn"
const MATHS_GAME_TWO = "res://Maths Games/Scenes/MathsGameTwo.tscn"
const Three_Mens_Morris = "res://Three_Mens_Morris/Scenes/three_Mens_Morris.tscn"
const Hey_Thats_My_Fish = "res://Hey_Thats_My_Fish/Scenes/HeyThatsMyFish.tscn"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	stats_label.text = "| Addition High Score: %d | Search High Score: %d |" % [GameState.addition_score, GameState.search_score]
	
	if addition_game.is_connected("pressed", Callable(self, "_on_addition_game_pressed")):
		addition_game.disconnect("pressed", Callable(self, "_on_addition_game_pressed"))
	
	if number_search_game.is_connected("pressed", Callable(self, "_on_number_search_game_pressed")):
		number_search_game.disconnect("pressed", Callable(self, "_on_number_search_game_pressed"))
	
	if three_mens_morris.is_connected("pressed", Callable(self, "_on_three_mens_morris_pressed")):
		three_mens_morris.disconnect("pressed", Callable(self, "_on_three_mens_morris_pressed"))
	
	if hey_thats_my_fish.is_connected("pressed", Callable(self, "_on_hey_thats_my_fish_pressed")):
		hey_thats_my_fish.disconnect("pressed", Callable(self, "_on_hey_thats_my_fish_pressed"))
	
	
	addition_game.pressed.connect(_on_addition_game_pressed)
	number_search_game.pressed.connect(_on_number_search_game_pressed)
	three_mens_morris.pressed.connect(_on_three_mens_morris_pressed)
	hey_thats_my_fish.pressed.connect(_on_hey_thats_my_fish_pressed)

func _on_addition_game_pressed() -> void:
	load_game(MATHS_GAME_ONE)

func _on_number_search_game_pressed() -> void:
	load_game(MATHS_GAME_TWO)

func _on_three_mens_morris_pressed() -> void:
	load_game(Three_Mens_Morris)

func _on_hey_thats_my_fish_pressed() -> void:
	load_game(Hey_Thats_My_Fish)

func load_game(scene_path: String) -> void:
	if ResourceLoader.exists(scene_path):
		get_tree().change_scene_to_file(scene_path)
