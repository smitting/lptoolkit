//
// File: 	LP-Arp.js
// Date: 	2014-05-25
// Author: 	Scott Mitting
// Abstract:
//	This is a simple arpeggiator that uses the launchpad to define
//	the order of notes to play for every note held down.  The notes
//	are synced to the clock starting with when the first note was
//	held down.  Subsequent notes are synced to the first.
//


//-------------------------------------------------------------------
// Data
//-------------------------------------------------------------------

// the keys currently being held down
var keysHeldDown = [];

// the number of steps (the right-most is used for determining the rate)
var stepCount = implant.pads.width - 1;

// the arpeggiated steps, init to -1 or last session value
var steps = [];
for (var i = 0; i < stepCount; i++) {
	var key = 'arp_' + i;
	if (implant.session.has(key)) {
		steps[i] = implant.session.get(key);
	}
	else {
		steps[i] = -1;
	}
}

// the different available speeds
var speeds = [ 4, 8, 16, 24, 32, 48, 96, 144 ];

// the current playback step
var step = 0;

// how many ticks to wait before advancing a step
var rate = 8;


//-------------------------------------------------------------------
// Methods
//-------------------------------------------------------------------

function addKey(pitch) {
	for (var i = 0; i < keysHeldDown.length; i++) {
		if (keysHeldDown[i] == pitch) {
			return;
		}
	}
	keysHeldDown.push(pitch);
}

function removeKey(pitch) {
	for (var i = 0; i < keysHeldDown.length; i++) {
		if (keysHeldDown[i] == pitch) {
			keysHeldDown.splice(i, 1);
		}
	}
	
	// put the step off screen if this was the last key
	if (keysHeldDown.length == 0) {
		step = -1;
		paintAll();
	}
}

function noKeysDown() {
	return keysHeldDown.length == 0;
}



// === LaunchPad ===

function paintStep(x) {		

	var onColor = 'green';
	var offColor = 'off';
	if (x == step) {
		onColor = 'green';
		offColor = 'yellow'
	}

	var s = x < steps.length ? (implant.pads.height - 1) - steps[x] : -1;
	for (var y = 0; y < implant.pads.height; y++) {
		var color = y == s ? onColor : offColor;
		implant.pads.set(x, y, color);
	}
}

//
// The right-most column is used for the speed 
//
function paintRate() {
	var x = implant.pads.width - 1;
	var test = [];
	for (var y = 0; y < speeds.length; y++) {
		var speed = speeds[y];
		var color = speed == rate ? 'red' : 'yellow';
		implant.pads.set(x, y, color);				
		test.push(y + '=' + color);
	}	
	
}

function paintAll() {
	for (var x = 0; x < steps.length; x++) {
		paintStep(x);
	}
	paintRate();
}


// === Data Access ===

//
// Stores a new value for the step in system memory and in the
// session so it will have the same value next time we load
// the program.
//
function changeStep(step, value) {
	if (step < 0) return;
	if (step >= steps.length) return;
	steps[step] = value;
	implant.session.set('arp_' + step, value);
}




// === Playback ===

//
// Starts all notes for the current arpegiator step.
//
function playArpStep() {
	var offset = steps[step];
	if (offset < 0) return;
	
	for (var i = 0; i < keysHeldDown.length; i++) {
		var key = keysHeldDown[i];
		play(key + offset);
	}
} 



// manages playing and stopping notes
var playing = [];
function play(pitch) {
	implant.osc.set(pitch, 1, 1);
	implant.print('set(' + pitch + ',1,1');	
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


//-------------------------------------------------------------------
// Events
//-------------------------------------------------------------------


//
// Maintain notes to be arpeggiated.
// 
implant.keys.on('noteon', function(e) {
	implant.print('got note ' + e.x);
	if (noKeysDown()) {
		step = 0;
	}
	addKey(e.x);
});
implant.keys.on('noteoff', function(e) {
	removeKey(e.x);
});

implant.pads.on('press', function(e) {
	implant.print('pad press: ' + e.x + ',' + e.y);
	
	
	// last column is for setting rate
	if (e.x == implant.pads.width - 1) {
		rate = speeds[e.y];
		paintRate();
		return;
	}
	
	// either set new value or toggle it off
	var y = (implant.pads.height - 1) - e.y;
	changeStep(e.x, steps[e.x] == y ? -1 : y);
	
	// update screen
	paintAll();
});


//
// Repaint if our mode gets set
//
implant.mode.on('modechanged', function(e) {
	if (implant.mode.current == implant.assignedMode) {
		paintAll();
	}
});


//
// Arpeggiate all notes held down against the beat clock
//
implant.time.on('1/96', function(e) {
	var tick = Math.floor(e.x % rate);	
	if (tick == 0) {
		if (noKeysDown()) {
			return;
		}

		playArpStep();
		paintAll();
		step++;
		if (step >= steps.length) {
			step = 0;
		}
	}
	else if (tick == rate * 0.5) {
		stopAll();
	}	
});


















