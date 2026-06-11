import uuid
from copy import deepcopy

GRID_WIDTH = 8
GRID_HEIGHT = 8
Max_Fish_Per_Spot = 3
Min_Fish_Per_Spot = 1

def create_board():
    board = {}
    for x in range(GRID_WIDTH):
        for y in range(GRID_HEIGHT):
            pos_key = f"{x},{y}"
            board[pos_key] = {
                "fish": (x + y) % Max_Fish_Per_Spot + Min_Fish_Per_Spot,
                "visited": False,
                "player": None
            }
    return board

def create_fish_game(player_name):
    
    game_id = str(uuid.uuid4())[:8]
    player_id = str(uuid.uuid4())

    return game_id, player_id, {
        "game_type": "fish",
        "players": {
            player_id:{
                "name": player_name,
                "score": 0,
                "penguins": [],
                "symbol": "A"
            }
    },
        "board": create_board(),
        "current_turn": player_id,
        "move_count": 0,
        "winner": None,
        "status": "waiting",
        "phase": "placement"
    }

def join_fish_game(game, player_name):
    if len(game["players"]) >= 2:
        return None, "game is full"
    
    player_id = str(uuid.uuid4())
    game["players"][player_id] = {
        "name": player_name,
        "score": 0,
        "penguins": [],
        "symbol": "B"
    }

    game["status"] = "active"
    return player_id, None

def is_valid_move(board, from_pos, to_pos):

    try:
        from_x, from_y = map(int, from_pos.split(','))
        to_x, to_y = map(int, to_pos.split(','))
    except:
        return False
    
    if not(0 <= to_x < GRID_WIDTH and 0 <= to_y < GRID_HEIGHT):
        return False

    if board.get(to_pos, {}).get("visited", True):
        return False
    
    dist = abs(to_x - from_x) + abs(to_y - from_y)
    return dist == 1

def get_available_moves(board, player_pos):

    parts = player_pos.split(',')
    x, y = int(parts[0]), int(parts[1])

    neighbors = [(x+1, y), (x-1, y), (x, y+1), (x, y-1), (x+1, y+1), (x-1, y-1)]
    directions = [(0,1), (0,-1), (1,0), (-1,0), (1,-1), (-1,1)]

    available = []
    for dx, dy in neighbors:

        nx, ny = x + dx, y + dy
        
        key = f"{nx},{ny}"

        if key in board:
            tile = board[key]
            
            if not [tile["visited"], tile["player"]] is None:
                available.append(key)
    
    return available

def make_fish_move(game, player_id, from_pos, to_pos):
    
    if player_id != game["current_turn"]:
        return False, "not your turn"
    
    board = game["board"]
    player = game["players"][player_id]
   
    if from_pos not in player["penguins"]:
        return False, "invalid move: no penguin at from_pos"

    if not is_valid_move(board, from_pos, to_pos):
        return False, "invalid move"

    fish_collected = board[to_pos]["fish"]
    player["score"] += fish_collected

    board[from_pos]["visited"] = True
    board[from_pos]["player"] = player_id

    board[to_pos]["visited"] = False
    board[to_pos]["player"] = player_id

    player["penguins"].remove(from_pos)
    player["penguins"].append(to_pos)
    
    game["move_count"] += 1
    players_list = list(game["players"].keys())
    current_index = players_list.index(player_id)
    next_player_id = players_list[1 - current_index]

    next_can_move = any(get_available_moves(game["board"], p_pos) for p_pos in game["players"][next_player_id]["penguins"])
    
    current_can_move = any(get_available_moves(game["board"], p_pos) for p_pos in game["players"][player_id]["penguins"])
    
    if next_can_move:
        game["current_turn"] = next_player_id
    elif current_can_move:
        game["current_turn"] = player_id
    else:
        game["status"] = "finished"
        p1, p2 = players_list[0], players_list[1]

        if p1["score"] > p2["score"]:
            game["winner"] = players_list[0]
        elif p1["score"] < p2["score"]:
            game["winner"] = players_list[1]
        else:
            game["winner"] = "draw"
    
    return True, None