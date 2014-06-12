//
// File: 	Melody-Arp.js
// Date: 	2014-05-31
// Author: 	Scott Mitting
// Abstract:
//	This is a MIDI melody editor for the launchpad.  Right now it's
//	just 8x8.
//	
//	A start note is orange, and sustain is in yellow.  Holding down
//	a pad and then touching another pad on the same row will cause
//	the note to be held.
//
//	TODO: support scrolling
//	TODO: tie to a sequencer and save as .MID files
//
// Additions:
//	- notes 72-83 select a sequence
//



var version = '0.02';
var devDate = '2014-05-31'

implant.print('Starting Melody-Arp ' + version);
implant.print('by Scott Mitting (' + devDate + ')');


//-------------------------------------------------------------------
// Data
//-------------------------------------------------------------------

// the variable in the session the melody will be named as
var SESSION_NAME = 'TEST';


// first midi note where sequence selector starts
var SEQUENCE_START = 72;

// the number of steps (the right-most is used for determining the rate)
var stepCount = implant.pads.width - 1;

// when set to true on the next tick step zero will start
var startSequence = false;

var maxX = implant.pads.width - 2;
var maxY = implant.pads.height - 1;

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


var KeySignature = {
	     // C  D  E  F  G  A  B  C
	major: [0, 2, 4, 5, 7, 9, 11,12],
	     // C  D  Eb F  G  Ab Bb C
	minor: [0, 2, 3, 5, 7, 8, 10,12]	
};

var isMajor = true;
var keyNotes = KeySignature.major;


//
// Colors used by this program.
//
var Colors = {
	noteStart: 'orange',
	note: 'yellow',
	current: 'green',
	cursor: 'red',
	major: 'green',
	minor: 'orange'
};



//
// Sends a method's arguments to the console.  The first
// argument is the name followed by the argments.
//
function debug() {
//	return; // comment out to enabled debugging
	
	var s = arguments[0] + '(';
	for (var i = 1; i < arguments.length; i++) {
		if (i > 1) s += ', ';
		var arg = arguments[i];
		if (typeof arg === 'string') {
			
		}
		else if (typeof arg === 'number') {
			
		}
		else {
			arg = JSON.stringify(arg);
		}
		
		s += arg;
	}
	s += ')';
	implant.print(s);	
}

//-------------------------------------------------------------------
// Melody Object
// TODO: change to native sequence
//
// This stores the note data and when they end, with methods to get
// what notes should start at stop at each step.
//-------------------------------------------------------------------
var Melody = {
	
	//
	// Notes in the format { note, start, length }
	//
	notes: [],
	
	//
	// Array of saved sets of notes.  pressing midi 72-83 selects
	// a different set from this list
	//
	sequences: [],
	
	//
	// The selected index
	//
	sequenceIndex: 0,
	
	//
	// Saves this melody to the session so it can be reloaded later.
	//
	save: function Melody_save(name) {		
		implant.session.setString(name, JSON.stringify(this.notes));
		implant.session.setString(name + '_LIST', JSON.stringify(this.notes));
	},
	
	load: function Melody_load(name) {
		if (implant.session.hasString(name)) {
			this.notes = JSON.parse(implant.session.getString(name));
		}
		else {
			this.notes = [];
		}
		if (implant.session.hasString(name + '_LIST')) {
			this.sequences = JSON.parse(implant.session.getString(name + '_LIST'));
		}
		else {
			this.sequences = [];
		}
		
		// destroy the data if they aren't arrays
		function isArray(obj) {
			return  Object.prototype.toString.call( obj ) === '[object Array]' ;
		}
		
		if (!isArray(this.notes)) {
			this.notes = [];
		}
		if (!isArray(this.sequences)) {
			this.sequences = [];
		}
		else {
			for (var i = 0; i < this.sequences.length; i++) {
				if (!isArray(this.sequences[i])) {
					this.sequences[i] = [];
				}
			}
		}
		
		// choose this as sequence 0
		this.sequenceIndex = 0;
		if (this.sequences.length > 0) {
			this.notes = this.sequences[0];
		}
	},
	
	//
	// Changes the sequence in use and repaints it.
	//
	selectSequence: function Melody_selectSequence(index) {
		debug('Melody.selectSequence', index)
		while (this.sequences.length - 1 < index) {
			this.sequences.push([]);
		}
		this.notes = this.sequences[index];
		this.sequenceIndex = index;
		debug('Melody object', this);
		this.paint();
	},
	
	
	//
	// Changes the note at the given location.  If lenght one is set
	// and the note is already length 1, it is removed.
	//
	// note: 	the pad number (x) of the note
	// start: 	the row (y) of the note
	// length: 	how many steps for this note
	//
	set: function Melody_set(note, start, length) {
		
		debug('Melody.set', note, start, length);
		
		var foundNote = false;
		
		// search for note to change
		for (var i = 0; i < this.notes.length; i++) {
			var n = this.notes[i];
			if (n.note == note && n.start == start) {
				if (length == 1 && n.length == 1) {
					this.notes.splice(i, 1);
					foundNote = true;
					break;
				}
				else {
					n.length = length;
					foundNote = true;
					break;
				}
			}
		}
		
		// add note if note found
		if (foundNote == false && length > 0) {
			this.notes.push({ note: note, start: start, length: length});
		}		
		
		// save change to sequence
		this.sequences[this.sequenceIndex] = this.notes;
	},
	
	//
	// Returns an array of note values starting at a given x.
	//
	// start: 	the start of the notes to return
	//
	getNotesStartingAt: function Melody_getNotesStartingAt(start) {
		var ret = [];
		for (var i = 0; i < this.notes.length; i++) {
			var n = this.notes[i];
			if (n.start == start) {
				ret.push(n.note);
			}
		}
		return ret;
	},
	
	//
	// Returns an array of nte values ending at a given x
	//
	// end: 	the end of the notes to return
	//
	getNotesEndingAt: function Melody_getNotesEndingAt(end) {
		var ret = [];
		if (end == 0) end += stepCount;
		for (var i = 0; i < this.notes.length; i++) {
			var n = this.notes[i];
			if (n.start + n.length == end) {
				ret.push(n.note);
			}
		}
		return ret;
	},
	
	//
	// Returns an array of the requested size where each 
	// element is a color, going by x first then y
	//
	// w: 		the width of the visible area
	// h: 		the height of the visible area
	// step: 	where the play cursor is currently (optional)	
	//
	getColors: function Melody_getColors(w, h, step) {
		var ret = [];
		
		step = step == null ? -1 : step;
		
		// initialize grid to off
		for (var y = 0; y < h; y++) {
			for (var x = 0; x < w; x++) {
				ret.push('off');
			}			
		}
		
		// draw each of the notes
		for (var i = 0; i < this.notes.length; i++) {
			var n = this.notes[i];			
			
			var y = maxY - n.note;
			if (y < 0 || y > maxY) break;
			
			// see if this is a note currently being played
			var currentNote = false;
			if (step > -1) {
				if (step >= n.start && step < n.start + n.length) {
					currentNote = true;
				}
			}
			
			// paint the note
			for (var xo = 0; xo < n.length; xo++) {
				var x = n.start + xo;
				if (x >= 0 && x <= maxX) {
					var offset = y * w + x;
					if (currentNote) {
						ret[offset] = Colors.current;
					}
					else {
						ret[offset] = xo == 0 ? Colors.noteStart : Colors.note;
					}
				}				
			}			
		}
		
		// draw the playback cursor if provided
		if (step >= 0 && step <= maxX) {
			for (var y = 0; y <= 0; y++) {
				var offset = y * w + step;
				if (ret[offset] == 'off') {
					ret[offset] = Colors.cursor;
				}				
			}
		}
		
		return ret;
	},
	
	//
	// Paints the launchpad from the melody.  If the current step
	// is provided, all notes on that step are highlighted, and the
	// rest of the rows are colored red.
	//
	// step: 	where the play cursor is currently (optional)
	//
	paint: function(step) {
		step = step == null ? -1 : step;
		
		debug('Melody.paint', step);
		var w = implant.pads.width - 1;
		var h = implant.pads.height;
		var colors = this.getColors(w, h, step);
		var index = 0;
		for (var y = 0; y < h; y++) {
			for (var x = 0; x < w; x++) {
				var c = colors[index++];
				implant.pads.set(x,y,c);
			}
		}
	}
	
};


//-------------------------------------------------------------------
// MelodyUI Object
//
// Handles all melody input from the user.
//-------------------------------------------------------------------
var MelodyUI = {
	
	// first button pressed in a sequence {x,y}
	first: null,
	
	//
	// A pad button has been pressed
	//
	press: function(e) {
		debug('MelodyUI.press', e);
		
		// if new note sequence
		if (this.first == null) { 
			this.first = { x: e.x, y: e.y};
			
			// highlight what we're doing in green
			implant.pads.set(e.x, e.y, Colors.current);
			
			// set the note length to 1, which will delete the note
			// when we release if we already had a length one and
			// didn't increase the length
			Melody.set(maxY - e.y, this.first.x, 1);
		}
		// if changing note length
		else { 
			
			// ignore pressed on some other row
			if (e.y != this.first.y) return;
			
			// ignore negative note lengths
			if (e.x < this.first.x) return;
						
			// highlight whole section in green
			var length = e.x - this.first.x + 1;
			for (var xo = 0; xo < length; xo++) {
				var x = this.first.x + xo;
				implant.pads.set(x, e.y, Colors.current);
			}
			
			// change the note length			
			Melody.set(maxY - e.y, this.first.x, length);			
		}
	},
	
	//
	// A pad button has been released
	//
	release: function(e) {
		debug('MelodyUI.release', e);
		
		// end of note
		if (this.first != null && this.first.x == e.x && this.first.y == e.y) {
			this.first = null;
			
			// repaint to show what we changed
			Melody.paint();
			Melody.save(SESSION_NAME);
			
			debug('current notes', Melody.notes);
		}
		// release from note length change
		else {
			// ignore releases of length changes
		}
	}
};


implant.pads.on('press', function(e) {
	if (e.x <= maxX) {
		MelodyUI.press(e);
	}	
	else {
		
		if (e.x == implant.pads.width - 1) {
			// rate presses
			if (e.y < implant.pads.height - 1) {
				rate = speeds[e.y];		
			}
			// key signature
			else {				
				if (isMajor) {
					isMajor = false;
					keyNotes = KeySignature.minor;
				}
				else {
					isMajor = true;
					keyNotes = KeySignature.major;
				}				
			}				
			paintRate();		
		}				
	}
});
implant.pads.on('release', function(e) {
	if (e.x <= maxX) {
		MelodyUI.release(e);
	}	
});




//-------------------------------------------------------------------
// Playing Object
//
// Keeps track of all notes playing.
//-------------------------------------------------------------------
var Playing = {
	
	// current notes playing on some VST instrument
	playing: {},	

	// the keys currently being held down on the midi keyboard
	keysHeldDown: [],
	
	//
	// Starts playing a note
	//
	// pitch: 		the pitch to play
	// trigger: 	(optional) the key that triggered this pitch
	//
	play: function Playing_play(pitch, trigger) {
		debug('Playing.play', pitch);
		implant.osc.set(pitch, 1, 1);
		this.playing[pitch] = { active: true, trigger: trigger };
	},
	
	//
	// Stops a note.
	// 
	stop: function Playing_stop(pitch) {
		debug('Playing.stop', pitch);
		implant.osc.set(pitch, 0, 0);		
		this.playing[pitch] = null;
	},
	
	//
	// Updates the notes to turn on and off from a step in the melody.
	//
	playMelody: function Playing_playMelody(step) {
		
		
		var starts = Melody.getNotesStartingAt(step);
		var stops = Melody.getNotesEndingAt(step);
		for (var j = 0; j < this.keysHeldDown.length; j++) {
			var relativeTo = this.keysHeldDown[j];
			for (var i = 0; i < stops.length; i++) {
				this.stop(relativeTo + keyNotes[stops[i]], relativeTo);
			}
			for (var i = 0; i < starts.length; i++) {
				this.play(relativeTo + keyNotes[starts[i]], relativeTo);
			}
		}		
	},
	
	
	//
	// Stops all the notes.
	//
	stopAll: function Playing_stopAll(relativeTo) {
		
		debug('Playing.stopAll');
		for (var k in this.playing) {
			var item = this.playing[k];
			if (relativeTo == null) {
				if (item != null) {
					this.stop(k);
				}
			}
			else {
				if (item != null && item.trigger == relativeTo) {
					this.stop(k);
				}
			}
		}
		this.playing = {};		
	},
	
	//
	// True iff no midi keys are pressed
	//
	noKeysDown: function Playing_noKeysDown() {
		return this.keysHeldDown.length == 0;
	},


	addKey: function Playing_addKey(pitch) {
		for (var i = 0; i < this.keysHeldDown.length; i++) {
			if (this.keysHeldDown[i] == pitch) {
				return;
			}
		}
		this.keysHeldDown.push(pitch);
	},

 	removeKey: function Playing_removeKey(pitch) {
		for (var i = 0; i < this.keysHeldDown.length; i++) {
			if (this.keysHeldDown[i] == pitch) {
				this.keysHeldDown.splice(i, 1);
			}
		}

		// put the step off screen if this was the last key
	},

	
	
	//
	// Setups up on implant events for this object
	// 
	initEvents: function Playing_initEvents() {
		var me = this;
		
		// watching midi keyboard 
		implant.keys.on('noteon', function(e) {
			
			// if note is above 72, let's select a sequence
			if (e.x >= SEQUENCE_START) {
				Melody.selectSequence(e.x - SEQUENCE_START);
				return;
			}
			
			
			// start sequence on first note
			if (me.noKeysDown()) {
				starSequence = true;
			}
			
			// add key to playback list
			me.addKey(e.x);
		});
		implant.keys.on('noteoff', function(e) {
			// remove key from playback list
			me.removeKey(e.x);
			
			// stop all notes tied to playing this note
			me.stopAll(e.x);
			
			// stop sequence on last note
			if (me.noKeysDown()) {
				step = -1;
				Melody.paint(-1);
				me.stopAll();
			}			
		});
		
		// arpeggiate against midi clock		
		implant.time.on('1/96', function(e) {
			var tick = Math.floor(e.x % rate);	

			// do nothing if no keys are down
			if (me.noKeysDown()) {
				return;
			}

			// progress the clock and repaint
			if (tick == 0) {
				
				if (startSequence) {
					step = 0;
					startSequence = false;
				}
				else {
					step = (step + 1) % stepCount;							
				}				
				
				Melody.paint(step);		
				Playing.playMelody(step);
			}
		});
	}
};
	
	
	
	
	

//
// Repaint if our mode gets set
//
implant.mode.on('modechanged', function(e) {
	if (implant.mode.current == implant.assignedMode) {
		Melody.paint(step);
		paintRate();
	}
});



//
// Rate control stuff (todo: cleanup)
//

// The right-most column is used for the speed 
function paintRate() {
	var x = implant.pads.width - 1;
	for (var y = 0; y < speeds.length; y++) {
		var speed = speeds[y];
		var color = speed == rate ? 'red' : 'yellow';
		implant.pads.set(x, y, color);				
	}		
	
	// bottom most is the key signature
	implant.pads.set(x, implant.pads.height - 1, isMajor ? Colors.major : Colors.minor);
}



// init
Playing.initEvents();
paintRate();
Melody.load(SESSION_NAME);
