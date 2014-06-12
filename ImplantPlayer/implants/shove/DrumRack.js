//
// Project: Shove - Push emulator series
// File: 	DrumRack.js
// Author: 	Scott Mitting
// Date: 	2014-05-08
// Abstract: 
//	This is a drum rack implant similar to the drum sequencer provided
//	by Ableton Push:
//	- The lower-left quadrant provides a 4x4 grid for finger drumming in yellow.  
//	- The top 4 rows provides the sequence steps.  
//	- The lower-right grid of 4x4 provides looping options.
//	- The circles on the far right provide the 8 step levels
//
// This plugin assumes a 9x8 area
//


var MIDI_START = 36;

var MAX_STEPS = 32 * 3;

var selectedDrum = 0;
var stepLevel = 0;
var loopStart = 0, loopEnd = 3;
var running = false;

// tracks for each drum
var tracks = [];
for (var drum = 0; drum < 16; drum++) {
	var steps = [];
	for (var i = 0; i < 32; i++) {
		steps[i] = 0;
	}
	tracks[drum] = {steps:steps};	
}

/**
  * Sends the requested drum hit to MIDI out.
  */
function drumhit(drum) {
	implant.osc.set(MIDI_START + drum, 0, 1.0);
}

/**
  * Returns true iff the track is empty.
  */
function isTrackEmpty(track) {
	var steps = tracks[track].steps;
	for (var i = loopStart; i <= loopEnd; i++) {
		if (steps[i] > 0) return false;
	}
	return true;
}


/**
  * All drawing is handled by this object.
  */
var Draw = {
	/**
	  * Draws the sequencer area.
	  */ 
	sequencer: function() {
		
		
		var i = 0;
		var steps = tracks[selectedDrum].steps;
		for (var y = 0; y < 4; y++) {
			for (var x = 0; x < 8; x++, i++) {
				var color = steps[i] > 0 ? 'green' : 'off';
				if (i < loopStart) color = 'red';
				if (i > loopEnd) color = 'red';
				implant.pads.set(x,y,color);
			}
		}		
	},
	/**
	  * Draws the drum rack area in yellow, with the selected drum
	  * for the sequencer in red, and drums with sequenced data
	  * in amber.
	  */
	drumrack: function() {
		var i = 0;
		for (var y = 7; y >= 4; y--) {
			for (var x = 0; x < 4; x++, i++) {
				var color = 'yellow'
				if (i == selectedDrum) {
					color = 'red';
				}
				else if (isTrackEmpty(i) == false) {
					color = 'amber';
				}
				implant.pads.set(x,y,color);
			}
		}		
	},
	/**
	  * Draws the looping options area in green.
	  */	
	looping: function() {
		var i = 0;
		for (var y = 4; y < 8; y++) {
			for (var x = 4; x < 8; x++,i++) {
				var color = (i >= loopStart && i <= loopEnd) ? 'green' : 'off';
				implant.pads.set(x,y,color);
			}
		}
	},
	
	/**
	  * Draws the highlighted step level in red.
	  */
	steplevels: function() {
		var x = 8;
		var i = 0;
		for (var y = 0; y < 7; y++, i++) {
			var color = (i == stepLevel) ? 'red' : 'off';
			implant.pads.set(x,y,color);
		}	
		
		// bottom one is run state
		implant.pads.set(8,7,running ? 'green' : 'red');
	},
		
	/**
	  * Redraws all of it.
	  */
	repaint: function() {
		this.sequencer();
		this.drumrack();
		this.looping();
		this.steplevels();
	}	
};

/**
  * Object dealing with all button press events.
  */
var Buttons = {
	step: {
		press: function(e) {
			
			if (e.y == 7) {
				running = !running;
			}
			else {			
				stepLevel = this._valueFor(e);
			}	
			Draw.steplevels();	
			Draw.sequencer();	
		},
		release: function(e) {
			
		},
		/**
		  * Returns the new index for the step value for a button/
		  */
		_valueFor: function(e) {
			var y = e.y;
			return y;			
		}
	},
	sequence: {
		press: function(e) {
			var i = this._indexFor(e);
			var steps = tracks[selectedDrum].steps;
			steps[i] = steps[i] > 0 ? 0 : 1;
			Draw.sequencer();			
		},
		release: function(e) {
			
		},
		/**
		  * Returns the index in the sequence for a button
		  */
		_indexFor: function(e) {
			var x = e.x;
			var y = e.y;
			return y * 8 + x;			
		}
	},
	drumrack: {
		press: function(e) {
			var old = selectedDrum;
			var x = e.x;
			var y = 7 - e.y;
			selectedDrum = y * 4 + x;
			Draw.drumrack();
			if (old != selectedDrum) {
				Draw.sequencer();
			}
			
			// send midi drum hit		
			drumhit(selectedDrum);
		},
		release: function(e) {
//			implant.osc.set(selectedDrum, 0, 0);
		}
	},
	looping: {
		hold: null,
		press: function(e) {
			if (this.hold == null) {
				this.hold = e;
				loopStart = this._pointFor(e);
				loopEnd = this._pointFor(e);
				Draw.looping();
				Draw.sequencer();
			}
			else {
				loopEnd = this._pointFor(e);
				Draw.looping();
				Draw.sequencer();
			}			
		},
		release: function(e) {
			if (this.hold != null) {
				if (this.hold.x == e.x && this.hold.y == e.y) {
					this.hold = null;
				}
			}
		},
		/**
		  * Returns value for x/y location in event
		  */
		_pointFor: function (e) {
			var x = e.x - 4;
			var y = e.y - 4;
			return y * 4 + x;			
		}		
	},
	/**
	  * Returns what component a button is for.
	  */
	which: function(e) {		
		if (e.x == 8) {
			return 'step';
		}
		if (e.y < 4) {
			return 'sequence';
		}
		if (e.x < 4) {
			return 'drumrack';
		}
		return 'looping';		
	}
};

// route events to correct function
implant.pads.on('press', function(e) {
	Buttons[Buttons.which(e)].press(e);
});
implant.pads.on('release', function(e) {
	Buttons[Buttons.which(e)].release(e);	
});

// repaint whenever the mode changed
// TODO: when there's a new "shown" event use that instead
implant.mode.on('modechanged', function() {	
	if (implant.assignedMode == implant.mode.current) {
		Draw.repaint();
		implant.print('mode changed to this device.  repainting.')
	}
	else {
		implant.print('mode changed away so not redrawwing.');
	}		
});

// run sequencer
implant.time.on('1/96', function(e) {
	if (running) {
		if (e.value % 6 != 0) return;
		
		var step = Math.floor(e.value / 6);
		
		// TODO: use steps somehow
		var length = (loopEnd - loopStart) + 1;
		var beat = loopStart + (step % length); 
		
		implant.print('e=' + e.value + ' step=' + step + ' beat=' + beat + ' length=' + length + ' tracks=' + tracks.length);
		
		for (var track = 0; track < tracks.length; track++) {
//			if (tracks[track].steps.length <= beat) break;
			implant.print('tracks[' + track + '].steps[' + beat + '] = ' + tracks[track].steps[beat]);
			if (tracks[track].steps[beat] > 0) {
				drumhit(track);
			}			
		}
		
	}
});

// repaint
Draw.repaint();
implant.print('Shove DrumRack v0.01');





