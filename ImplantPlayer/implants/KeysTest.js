// test of keyboard.  Just repeats 16 notes of octaves on all notes played.

var down = [];
var playing = [];

function add(x) {
	for (var i = 0; i < down.length; i++) {
		if (down[i] == x) return;
	}
	down.push(x);
	down.sort();
}
function remove(x) {
	for (var i = 0; i < down.length; i++) {
		if (down[i] == x) {
			down.splice(i, 1);
			return;
		}
	}	
}


implant.keys.on('noteon', function(e) {
	implant.print('noteon: ' + e.x);
	if (e.x < 60) {
		add(e.x);
	}
	else {
		implant.osc.set(e.x, 1, 1);
	}
});

implant.keys.on('noteoff', function(e) {
	if (e.x < 60) {
		remove(e.x);
	}
	else {
		stop(e.x);
		
	}
});

function play(pitch) {
	implant.osc.set(pitch, 1, 1);	
	playing.push(pitch);
}
function stop(pitch) {
	implant.osc.set(pitch, 0, 0);		
}
function stopAll(pitch) {	
	// stop previous notes
	for (var i = 0; i < playing.length; i++) {
		stop(playing[i]);
	}
	playing = [];
}

var rate = 24;
var noteIndex = 0;

implant.time.on('1/96', function(e) {
	var step = Math.floor(e.x % rate);
	
	if (step == 0) {
		if (noteIndex >= down.length) {
			noteIndex = 0;
		}
		if (down.length > 0) {
			play(down[noteIndex]);
			noteIndex++;
		}
	}
	else if (step == rate * 0.5) {
		stopAll();
	}	
	
});


implant.print('KeysTest.js starting.');

