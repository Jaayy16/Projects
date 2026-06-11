from flask import Flask, request, jsonify
from flask_socketio import SocketIO, emit, join_room
import uuid
import json

from heyThatsMyFish import *

app = Flask(__name__)
socketio = SocketIO(app, cors_allowed_origins="*", async_mode='threading') 

games = {}

@app.route('/')
def home():
    return "Godot Server is running"


Winning_Combos = [
    [0, 1, 2],
    [3, 4, 5],
    [6, 7, 8],
    [0, 3, 6],
    [1, 4, 7],
    [2, 5, 8],
    [0, 4, 8],
    [2, 4, 6],
]

def create_game_helper (player_name):
  
    game_id = str(uuid.uuid4())[:8]
    player_id = str(uuid.uuid4())
    games[game_id] = {
        "players": {
            player_id: {"name": player_name, "symbol": "X"}
        },
        "board": [None] * 9,
        "current_turn" : player_id,
        "winner": None,
        "move_count": 0
    }
    return game_id, player_id

def join_game_helper (game_id, player_name):
    
    if game_id not in games:
        return None, "game not found"
    
    game = games[game_id]
    if len(game["players"]) >= 2:
        return None, "game is full"
    
    player_id = str(uuid.uuid4())
    game["players"][player_id] = {"name": player_name, "symbol": "O"}

    socketio.emit('game_state_changed', {'game_id': game_id}, room = game_id)
    socketio.emit('player_joined', {'game_id': game_id, 'player_name': player_name}, room = game_id)
    return player_id, None

def check_winner(board, symbol):
   
    for combo in Winning_Combos:
        if all(board[i] == symbol for i in combo):
            return True
    return False

def make_move_helper (game_id, player_id, from_pos, to_pos, pos):
    
    if game_id not in games:
        return False, "game not found"

    game = games[game_id]

    if player_id != game["current_turn"]:
        return False, "Not your turn"

    if pos is not None:

        if pos < 0 or pos > 8:
            return False, "Invalid position"
        
        if game["board"][pos] is not None:
            return False, "Position already occupied"
        
        symbol = game["players"][player_id]["symbol"]
        game["board"][pos] = symbol
        game["move_count"] += 1
    
    elif from_pos is not None and to_pos is not None:
        
        if from_pos < 0 or from_pos > 8 or to_pos < 0 or to_pos > 8:
            return False, "Invalid position"
        
        if game["board"][from_pos] is None:
            return False, "No piece at from position"
        
        if game["board"][to_pos] is not None:
            return False, "Destination already occupied"
            
        symbol = game["players"][player_id]["symbol"]
        game["board"][from_pos] = None
        game["board"][to_pos] = symbol
        game["move_count"] += 1

    else:
        return False, "Invalid move parameters"

    symbol = game["players"][player_id]["symbol"]

    if game["move_count"] >= 5 and check_winner(game["board"], symbol):
        game["winner"] = player_id
    else:
        player_ids = list(game["players"].keys())
        if player_ids[0] == player_id:
            game["current_turn"] = player_ids[1]
        else:
            game["current_turn"] = player_ids[0]
    
    socketio.emit('game_state_changed', {'game_id': game_id}, room = game_id)
    return True, games[game_id]

@app.route('/api/game/create_game', methods=['POST'])
def create_game():
    
    data = request.get_json()
    player_name = data.get('player_name')
    game_type = data.get('game_type', 'morris')
    
    if not player_name:
        return jsonify({"error": "Player name is required"}), 400
    
    if game_type == "fish":
        game_id, player_id, game_state = create_fish_game(player_name)
        games[game_id] = game_state
    else:
        game_id, player_id = create_game_helper(player_name)
    
    return jsonify({"game_id": game_id, "player_id": player_id, "player_name": player_name}), 200

@app.route('/api/game/join_game', methods=['POST'])
def join_game():
    
    data = request.get_json()
    game_id = data.get("game_id")
    player_name = data.get("player_name")
    
    if not game_id or not player_name:
        return jsonify({"error": "game ID and player name are required"}), 400
    

    if game_id not in games:
        return jsonify({"error": "game not found"}), 404
    
    game = games[game_id]
    game_type = game.get("game_type", "morris")

    if game_type == "fish":
        player_id, error = join_fish_game(game, player_name)
    else:
        player_id, error = join_game_helper(game_id, player_name)

    if error:
        return jsonify({"error": error}), 400
    
    socketio.emit('player_joined', { 'game_id': game_id, 'player_name': player_name, 'total_players': len(game["players"]) }, room = game_id)

    return jsonify({"game_id": game_id, "player_id": player_id,  "player_name": player_name}), 200

@app.route('/api/game/get_game_state', methods=['POST'])
def get_game_state():
    
    data = request.get_json()
    game_id = data.get("game_id")

    if not game_id or game_id not in games:
        return jsonify({"error": "game ID is required"}), 400
    
    return jsonify({"game_state": games[game_id]}), 200

@app.route('/api/game/make_move', methods=['POST'])
def make_move():
   
    data = request.get_json()
    game_id = data.get("game_id")
    player_id = data.get("player_id")

    if not game_id or not player_id:
        return jsonify({"error": "Missing game or player ID's"}), 400

    if game_id not in games:
        return jsonify({"error": "game not found"}), 404

    game = games[game_id]
    game_type = game.get("game_type", "morris")

    print(f"DEBUG - Received data: {data}")
    print(f"DEBUG - Game Type: {game_type}") 

    if game_type == "fish":
        to_pos = data.get("to_pos")
        from_pos = data.get("from_pos")
        if not to_pos or not from_pos: 
            return jsonify({"error": "form_pos and to_pos are required for fish game"}), 400
        
        success, result = make_fish_move(game, player_id, from_pos, to_pos)
    
    else:

        pos = data.get("pos")
        from_pos = data.get("from_pos")
        to_pos = data.get("to_pos")

        success, result = make_move_helper(game_id, player_id, from_pos, to_pos, pos)

    if not success:
        print(f"DEBUG - Move failed: {result}")
        return jsonify({"error": result}), 400
    
    print(f"DEBUG - Move successful")
    socketio.emit('game_state_changed', {'game_id': game_id, 'game_type': game_type}, room = game_id)

    return jsonify({
        "status": "success", 
        "game_state":{            
            "board":game["board"],
            "current_turn": game["current_turn"],
            "winner": game.get("winner"),
            "move_count": game.get("move_count", 0),
            "players": game.get("players", {})
        }
    }), 200

@app.route('/api/game/get_available_moves', methods=['POST'])
def get_available_moves():

    data = request.get_json()
    game_id = data.get("game_id")
    player_id = data.get("player_id")

    if game_id not in games:
        return jsonify({"error": "game not found"}), 404
    
    game = games[game_id]

    if game.get("game_type") != "fish":
        return jsonify({"error": "available moves only for fish game"}), 400
    
    player = game["players"][player_id]
    moves = get_available_moves(game["board"], player["penguin_pos"])

    return jsonify({"available_moves": moves}), 200

@app.route('/api/game/leave_game', methods=['POST'])
def leave_game():
    data = request.get_json()
    game_id = data.get("game_id")

    if game_id in games:
        del games[game_id]
        socketio.emit('game_state_changed', {'game_id': game_id}, room = game_id)
        return jsonify({"status": "success"}), 200
    else:
        return jsonify({"error": "game not found"}), 404

#Penguin Game Unique Logic

def place_penguin_helper(game, player_id, pos):

    player_ids = list(game["players"].keys())
    if len(player_ids) < 2:
        pass

    board = game["board"]
    tile = board.get(pos)
    
    if not tile: return False, "invalid position"

    if tile.get("visited") or tile.get("player") is not None:
        return False, "tile already occupied"

    if tile["fish"] != 1:
        return False, "can only place on 1 fish tile"

    tile["player"] = player_id
    game["players"][player_id]["penguins"].append(pos)

    total_penguins = sum(len(p["penguins"]) for p in game["players"].values())

    if total_penguins >= 8:
        game["phase"] = "movement"
    
    if len(game["players"]) == 2:
        current_index = player_ids.index(player_id)
        game["current_turn"] = player_ids[1 - current_index]

    return True, game

@app.route('/api/game/place_penguin', methods=['POST'])
def place_penguin():
    
    data = request.get_json()
    game_id = data.get("game_id")
    player_id = data.get("player_id")
    pos = data.get("pos")

    if not game_id or not player_id or not pos:
        return jsonify({"error": "Missing Parameters"}), 404
    
    if game_id not in games:
        return jsonify({"error": "game not found"}), 404

    game = games[game_id]
    success, result = place_penguin_helper(game, player_id, pos)

    if not success:
        return jsonify({"error": result}), 400

    socketio.emit('game_state_changed', {'game_id': game_id}, room = game_id)
    return jsonify({"status": "success", "game_state": result}), 200

#SocketIO Events

@socketio.on('connect')
def handle_connect():
    print(f"Client connected: {request.sid}")
    emit('connect_response', {'data': 'Connected to server'})

@socketio.on('disconnect')
def handle_disconnect():
    print(f"Client disconnected: {request.sid}")

@socketio.on('join_game')
def handle_join_game(data):
    
    game_id = data['game_id']
    player_id = data['player_id']
    join_room(game_id)
    print(f"Player {player_id} joined game {game_id}")
    emit('join_game_response', {'status': 'joined'})

if __name__ == '__main__':
    socketio.run(app, host = '127.0.0.1', port = 5000, debug = True)