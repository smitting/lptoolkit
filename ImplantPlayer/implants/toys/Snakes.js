
var colors = ['red','redorange','orange','amber','yellow','yellowgreen','green','off'];

var snake = { 
	// position
	x: 4, y: 4, 
	// movement
	dx: 1, dy: 0, 
	// last movement time
	lastMove: 0, 
	// colors to draw
	color: 'yellow', color2: 'yellowgreen',
	// current size
	size: 3,
	// snake parts to erase
	tail: []
	};
	
// current food location	
var food = null;	
	
// how often to move the snake	
var time = 500;

// alternates between colors
var alternate = false;

// current game move
var mode = 'gameover';



var crashDuration = 5000;
var crashStart = 0;
var crashSpeed = 100;
var crashLast = 0;


//
// Puts a new food pellet on the map, but not on the tail.
//
function placeFood() {
	food = { x: Math.floor(Math.random() * 8), y: Math.floor(Math.random() * 8) + 1 };
	for (var i = 0; i < snake.tail.length; i++) {
		if (snake.tail[i].x == food.x && snake.tail[i].y == food.y) {
			placeFood();
			return;
		}
	}
	
	implant.pads.set(food.x, food.y,'red');
}


implant.pads.on('press', function(e) {
	if (mode == 'play') {
		if (e.y == 0) {
			switch (e.x) {
				case 0:
					if (snake.dx != 0 || snake.dy != 1) {
						snake.dx = 0;
						snake.dy = -1;
					}
					break;
				case 1:
					if (snake.dx != 0 || snake.dy != -1) {
						snake.dx = 0;
						snake.dy = 1;
					}
					break;
				case 2:
					if (snake.dx != 1 || snake.dy != 0) {
						snake.dx = -1;
						snake.dy = 0;
					}
					break;
				case 3:
					if (snake.dx != -1 || snake.dy != 0) {
						snake.dx = 1;
						snake.dy = 0;
					}
					break;
			}		
		}
            else {
                var distX = Math.abs(e.x - snake.x);
                var distY = Math.abs(e.y - snake.y);
                if (distX >= distY) {
                    snake.dy = 0
                    snake.dx = e.x > snake.x ? 1 : -1;
                }
                else {
                    snake.dx = 0;
                    snake.dy = e.y > snake.y ? 1 : -1;
                }
        
            }	
	}
	else if (mode == 'gameover') {
		start();
	}
});

function checkCrash() {
	if (snake.x >= implant.pads.width) {
		snake.x = implant.pads.width - 1;
		return true;
	}
	if (snake.y >= implant.pads.height) {
		snake.y = implant.pads.height - 1;
		return true;
	}
	if (snake.x < 0) {
		snake.x = 0;
		return true;
	}
	if (snake.y < 0) {
		snake.y = 0;
		return true;
	}	
	for (var i = 0; i < snake.tail.length; i++) {
		if (snake.x == snake.tail[i].x && snake.y == snake.tail[i].y) {
			return true;
		}
	}
	return false;
}

// draws the snake at its current location, letting the tail go until
// it's reached its target length and then start removing it
function drawSnake() {
	
	if (checkCrash()) {
		crashStart = new Date().getTime();
		mode = 'crash';
		return;		
	}
	
	if (alternate) {
		alternate = false;
	}
	else {
		alternate = true;
	}
	implant.pads.set(snake.x, snake.y, alternate ? snake.color : snake.color2);
	
	// don't each tail if crashed
	snake.tail.push({x:snake.x,y:snake.y});
	if (snake.tail.length > snake.size) {
		var eat = snake.tail.shift();
		implant.pads.set(eat.x, eat.y, 'off');			
	}
	
	// check if we ate the food
	if (snake.x == food.x && snake.y == food.y) {
		snake.size++;
		placeFood();
//		time = time * 0.9;
		time = time - 10;
	}
	
}

// clears the map and setups the snake and food
function start() {
	mode='starting';
	
	// clear map
	for (var y = 0; y < implant.pads.height; y++) {
		for (var x = 0; x < implant.pads.width; x++) {			
			implant.pads.set(x, y, 'off');
		}
	}
	
	// setup game
	snake.x = 4;
	snake.y = 4;
	snake.dx = 1;
	snake.dy = 0;
	snake.size = 1;
	snake.tail = [];
	snake.lastmove = 0;
	
	placeFood();
	
	// start
	mode = 'play';
}

//start();

implant.setInterval(function() {

	var now = new Date().getTime();

	if (mode == 'play') {
		// move and draw snake
		if (now - snake.lastMove > time) {
			snake.lastMove = now;
			snake.x += snake.dx;
			snake.y += snake.dy;
			drawSnake();
		}	
	}	
	else if (mode == 'gameover') {
		// just draw random dots
		var x = Math.random() * implant.pads.width;
		var y = Math.random() * implant.pads.height;
		var color = colors[Math.floor(Math.random() * colors.length)];
		implant.pads.set(x, y, color);
	}
	else if (mode == 'crash') {
		// TODO: show some animation
		
		if (now - crashLast > crashSpeed) {
			crashLast = now;
		
			var color = colors[Math.floor(Math.random() * colors.length)];

			for (var x = 0; x < implant.pads.width; x++) {			
				implant.pads.set(x, snake.y, color);
			}
			for (var y = 0; y < implant.pads.height; y++) {			
				implant.pads.set(snake.x , y, color);
			}
			snake.x = Math.random() * 8;
			snake.y = Math.random() * 8;
		
		}
		
		if (now - crashStart > crashDuration) {
			mode = 'gameover';
		}
	}
	
}, 10);
