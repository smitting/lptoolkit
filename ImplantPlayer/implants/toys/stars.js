//
//
// Abstract:
//	Highlights lines around the press buttons.
//


var Stars = {
	down: [],
	indexOf: function(e) {
		for (var i = 0; i < this.down.length; i++) {
			if (this.down[i].x == e.x && this.down[i].y == e.y) {
//			if (Math.floor(this.down[i].x) == Math.floor(e.x) && Math.floor(this.down[i].y) == Math.floor(e.y)) {
				return i;
			}
		}
		return -1;
	},
	add: function(e) {
		var i = this.indexOf(e);
		if (i == -1) {
			var p = {
				x: e.x,
				y: e.y,
				start: new Date().getTime() 
			};
			this.down.push(p);
		}
	},
	remove: function(e) {
		var i = this.indexOf(e);
		if (i > -1) {
			this.down.splice(i, 1);
		}
		else {
			implant.print('star not found to remove: (' + e.x + ',' + e.y + ')');
		}
	},
	paint: function() {
		
		// System TODO:  Should be able to double buffer in the software
		// and push all the changes to the hardware at once, possibly
		// taking advantage of the launchpad hardware double buffering.
		
		var grid = [];
		for (var y = 0; y < 8; y++) {
			grid[y] = [];
			for (var x = 0; x < 8; x++) {
				grid[y][x] = 'off';
			} 
		}
/*
		for (var y = 0; y < 8; y++) {			
			for (var x = 0; x < 8; x++) {
				implant.pads.set(x,y,'off');
			} 
		}
*/		
		var now = new Date().getTime();
		for (var i = 0; i < this.down.length; i++) {
			var e = this.down[i];
			if (e == null) continue;
			
//			implant.pads.set(e.x, e.y, 'red');

			
			
			var duration = now - e.start;
			var length = (duration / 250) % 4;
			for (var j = 0; j < length; j++) {
				var c;
				switch (j) {
					case 0: c = 'red'; break;
					case 1: c = 'orange'; break;
					case 2: c = 'yellow'; break;
					default: c = 'yellow'; break;
				}
				/*
				implant.pads.set(e.x, e.y + j, c);
				implant.pads.set(e.x, e.y - j, c);
				implant.pads.set(e.x + j, e.y, c);
				implant.pads.set(e.x - j, e.y, c);
				*/
				if (e.x + j < 8) {
					grid[e.y][e.x + j] = c;
				}
				if (e.x - j >= 0) {
					grid[e.y][e.x - j] = c;
				}
				if (e.y + j < 8) {
					grid[e.y + j][e.x] = c;
				}
				if (e.y - j >= 0) {
					grid[e.y - j][e.x] = c;
				}
				
			}
		}		
				
		for (var y = 0; y < 8; y++) {
			for (var x = 0; x < 8; x++) {				
				implant.pads.set(x, y, grid[y][x]);
			} 
		}
	}	
};



implant.pads.on('press', function(e) {
	Stars.add(e);	
});
implant.pads.on('release', function(e) {
	Stars.remove(e);
});
implant.time.on('1/96', function(e) {
	if (e.value % 6 == 0) {
		Stars.paint();
	}
});

implant.print('----[Stars]----');