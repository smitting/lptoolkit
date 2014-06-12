//
// Simple control of tempo.  Lights up the row every measure, the
// first button goes up 1 beat, next down 1,
// next down 10, next up 10
//

implant.pads.on('press', function(e) {
	if (e.x == 0) {
		implant.time.tempo++;
	}
	else if (e.x == 1) {
		implant.time.tempo--;
	}
	else if (e.x == 2) {
		implant.time.tempo-=10;
	}
	else if (e.x == 3) {
		implant.time.tempo+=10;
	}
	implant.print('tempo set to ' + implant.time.tempo);
});


var lastStep = -1;
implant.time.on('1/96', function(e) {
	var tick = Math.floor(e.x % 96);
	implant.print('x: ' + e.x + ' tick:' + tick);
	
	var step = tick / implant.pads.width;
	if (step != lastStep) {
		lastStep = step;
		for (var x = 0; x < implant.pads.width; x++) {
			var c = x == step ? 'green' : 'yellow';
			implant.pads.set(x, 0, c);
		}
	}
});

