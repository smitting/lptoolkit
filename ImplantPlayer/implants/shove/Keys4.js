// 
// Melodic keyboard similar to Push where each new row is on 4ths.
//

// ASSUMING GRID SIZE 8x8

var ROOT_NOTE = 36;

var roots = {
	c: 36,
	d: 38,
	e: 40,
	f: 41,
	g: 43,
	a: 45,
	b: 47
}
roots[0] = roots.c;
roots[1] = roots.d;
roots[2] = roots.e;
roots[3] = roots.f;
roots[4] = roots.g;
roots[5] = roots.a;
roots[6] = roots.b;

// all scales
var scales = {
	major: [0, 2, 4, 5, 7, 9, 11],
	minor: [0, 2, 3, 5, 7, 8, 10]
}


// testing C-major scale
var isMajor = true;
var scale = scales.major;
var root = roots.c;


/**
  * Returns the index into the scale for each pad with octave.  Every
  * row from the bottom increases by 3 and every column increases by
  * 1.  Splits into octaves.
  *
  * @param x the column of the pad
  * @param y the row of the pad
  * @returns { octave: 0, note: 1, index: 2 }
  */
function getNoteForPad(x, y) {
	var ret = {};
	ret.index = (7-y)*3 + x;
	ret.octave = Math.floor(ret.index / 7);
	ret.note = ret.index % 7;
	return ret;
}

/**
  * Returns the MIDI note for the pad selected
  */
function padToMidi(x, y) {
	var note = getNoteForPad(x, y);
	return noteToMidi(note);
}

function noteToMidi(note) {
	return ROOT_NOTE + note.octave * 12 + scale[note.note];	
}

function redraw() {
	for (var y = 0; y < 8; y++) {
		for (var x = 0; x < 8; x++) {
			var note = getNoteForPad(x, y);
			var c = note.note == 0 ? 'yellow' : 'off';
			implant.pads.set(x, y, c);
		}
	}
}

redraw();
implant.pads.on('press', function(e) {
	if (e.y < 0) return;
	
	if (e.x == 8) {
		// right-most column changes key
		
		// bottom key sets major/minor
		if (e.y == 7) {
			if (isMajor) {
				scale = scales.minor;
				isMajor = false;
				implant.pads.set(e.x, e.y, 'red');				
			}
			else {
				scale = scales.major;
				isMajor = true;
				implant.pads.set(e.x, e.y, 'green');
			}
		}
		else {
			root = roots[e.y];
			ROOT_NOTE = root;
			for (var y = 0; y < 7; y++) {
				var c = y == e.y ? 'yellow' : 'off';
				implant.pads.set(e.x, y, c);
			}			
		}
		return;
	}
	
	implant.pads.set(e.x, e.y, 'green');
	var note = getNoteForPad(e.x, e.y);
	implant.osc.set(noteToMidi(note), 1, 1.0);
});

implant.pads.on('release', function(e) {
	if (e.y < 0) return;

	var note = getNoteForPad(e.x, e.y);
	implant.osc.set(noteToMidi(note), 0, 0);
	redraw();
});
implant.print('Keys4 v0.00');


// repaint whenever the mode changed
// TODO: when there's a new "shown" event use that instead
implant.mode.on('modechanged', function() {
	if (implant.assignedMode == implant.mode.current) {
		redraw();
		implant.print('mode changed to this device.  repainting.')
	}
	else {
		implant.print('mode changed away so not redrawwing.');
	}		
});
