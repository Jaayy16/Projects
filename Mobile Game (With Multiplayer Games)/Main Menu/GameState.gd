extends Node

static var _instance: Node

#Variables for 'Addition Game'
var addition_score = 0
var addition_mistakes = 0
var addition_lvl = 1
var addition_lvls_completed = 0

#Variables for 'Search Game'
var search_score = 0
var search_mistakes = 0
var search_lvl = 1
var search_lvls_completed = 0

#Variables for 'Three Mens Morris Game'
var morris_score = 0
var morris_lvls_completed = 0

#General Variables
var total_games_played = 0
var last_game_type = ""

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if _instance == null:
		_instance = self
	else:
		queue_free()

func reset_addition_game() -> void:
	addition_score = 0
	addition_mistakes = 0
	addition_lvl = 1
	addition_lvls_completed = 0

func reset_search_game() -> void:
	search_score = 0
	search_mistakes = 0
	search_lvl = 1
	search_lvls_completed = 0

func reset_three_mens_morris() -> void:
	morris_score = 0
	morris_lvls_completed = 0

func add_addition_score(points: int) -> void:
	addition_score += points

func add_search_score(points: int) -> void:
	search_score += points

func add_morris_score(points: int) -> void:
	morris_score += points

static func get_instance() -> Node:
	return _instance
