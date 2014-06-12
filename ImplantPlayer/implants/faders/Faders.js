//
// File:    faders/Faders.js
// Author:  Scott Mitting
// Date:    2014-04-23
// Abstract:
//  Simulates parameter faders by columns on the launchpad.  Each column
//  over the active area represents a different OSC value.  Pressing a
//  button highlights all of the buttons up to that button in the active
//  area, and sets the OSC value to a percentage between 0 and 1 correlating
//  to that button.
//
//  LFO support:
//      - holding the first button and then tapping a 2nd buttons makes a SINE lfo synced to the beat tapped
//      - holding the first button and then tapping a sequence in order and tapping the last button
//          causes the LFO to follow the same sequence.
//


// settings
// TODO: this should be in the API and controllable in the program
var onColor = 'yellow';
var offColor = 'off'

// storage for fader values
var faderValues = [];

// current LFO settings
var lfo = [];

// which buttons are being held down.
var heldDown = {
	_list: [],
	down: function(x, y) {
		if (!this.check(x,y)) {
			this._list.push({x:x,y:y});
		}
	},
	up: function(x, y) {		
		for (var i = 0; i < this._list.length; i++) {
			var item = this._list[i];
			if (item.x == x && item.y == y) {
				this._list.splice(i, 1);
				break;				
			}
		}		
	},
	check: function(x, y) {
		for (var i = 0; i < this._list.length; i++) {
			var item = this._list[i];
			if (item.x == x && item.y == y) {
				return true;
			}
		}
		return false;
	}
};

// first button being held down in a sequence
var sequenceStart = null;


/**
  * Sets all of the fader values to zero.
  */
function reset() {
	for (var x = 0; x < implant.pads.width; x++) {
		setValue(x, 0);
	}
}

/**
  * Sets the value of a given fader between 0 and 1.  Calls setTopY()
  * to make sure it all lights up, which also stores the OSC value.
  * 
  * @param fader the x of the fader to change
  * @param value the OSC value to set.
  */
function setValue(fader, value) {
	setTopY(fader, getTopYFromValue(value));	
}

/**
  * Lights up all of the lights on the selected fader from the bottom
  * to this y value, and turns off all above it.  Also sets the fader's
  * numeric value based on this selection.
  * 
  * @param fader the x of the fader to change
  * @param topY the highest y position to light up
  */
function setTopY(fader, topY) {
	var x = Math.floor(fader);
	for (var y = 0; y < implant.pads.height; y++) {
        var color = y < topY ? offColor : onColor;
        implant.pads.set(x, y, color);
	}	
	faderValues[x] = getValueFromTopY(topY);
    implant.osc.set(x, 0, faderValues[x]);
}

/**
  * Returns what a fader value should be when given the highest y to
  * highlight on a fader.
  *
  * @param topY the highest y position lit up
  * @returns a value from 0 to 1
  */
function getValueFromTopY(topY) {
	var maxY = (implant.pads.height - 1)
	return Math.floor(maxY - topY) / maxY;	
}

/**
  * Returns what the top Y would be for a given value.
  *
  * @param value the OSC value from 0 to 1
  * @returns the highest y to light up
  */
function getTopYFromValue(value) {
	if (value < 0) value = 0;
	if (value > 1) value = 0;	
	var fh = Math.floor(value * implant.pads.height);
	return (implant.pads.height - 1) - fh;
}


// --- Initialization ---

reset();


// counts repeats
var lastPress = null;


// watch for press-and-hold events.  presses are for setting a value,
// but press-and-hold is for setting up LFOs
this.pads.on('press', function(e) {
	
	// reset last press if new button pressed
	if (lastPress != null) {
		if (lastPress.x != e.x || lastPress.y != e.y) {
			// handle when doing multiple press in sequence but not as beat tap
			if (sequenceStart != null && e.x == sequenceStart.x) {
				for (var i = 0; i < lastPress.count; i++) {
					lfo[e.x].list.push(lastPress.y);
				}
			}			
			lastPress = null;
		}
		else {
			lastPress.count++;
			lastPress.end = new Date().getTime();
		}
	}
	
	// check if a sequence of buttons happening while the first is held down
	if (sequenceStart != null && heldDown.check(sequenceStart.x, sequenceStart.y)) {
		
		
		// wait to continue sequence when repeated taps because it might be 
		// the beat timing
		if (lastPress == null) {
			// the sequence continues
			lfo[e.x].list.push(e.y);
		}		
		
		// TODO: watch for taps on the wrong fader.
		
	}
	// otherwise treat as simple press but watch for new sequence
	else {
		sequenceStart = { x: e.x, y: e.y };
		lfo[e.x] = { active: false, list: [e.y] };
		setTopY(e.x, e.y);
	}	
	
	// record the current button state
	heldDown.down(e.x, e.y);
	
	// start new last press
	if (lastPress == null) {
		lastPress = { x: e.x, y: e.y, count: 1, start: new Date().getTime(), end: new Date().getTime() };
	}
});

// update button state upon release and watch for the end of a sequence
this.pads.on('release', function(e) {
	heldDown.up(e.x, e.y);	
	if (sequenceStart != null && sequenceStart.x == e.x && sequenceStart.y == e.y) {
		// the sequence ended... check if we should activate the lfo
		if (lfo[e.x] != null && lfo[e.x].list.length > 1) {
			
			var now = new Date().getTime();
			
			// TODO: use the taps to figure out speed
			
			// check if we had an active beat tap going
			var speed = 250;
			if (lastPress && lastPress.count > 1) {
				speed = (lastPress.end - lastPress.start) / (lastPress.count - 1);		
			}			
			
			// detect sine oscilators
			if (lfo[e.x].list.length == 2) {
				lfo[e.x].type = 'sine';
			}
			else {			
				lfo[e.x].type = 'order';
				lfo[e.x].speed = speed / lfo[e.x].list.length;
			}
			lfo[e.x].start = now;
			lfo[e.x].index = 0;
			lfo[e.x].value = -1;
			lfo[e.x].active = true;
		}
		sequenceStart = null;
	}
});


this.setInterval(function() {
	
	var now = new Date().getTime();
	for (var i = 0; i < lfo.length; i++) {
		if (lfo[i] && lfo[i].active && lfo[i].list.length > 1) {
			
			var timeElapsed = now - lfo[i].start;
			
			if (lfo[i].type == 'sine') {
				var oldValue = lfo[i].value;
				var theta = Math.sin((timeElapsed / lfo[i].speed) * Math.PI * 2);
				var a = getValueFromTopY(lfo[i].list[0]);
				var b = getValueFromTopY(lfo[i].list[1]);
				var max = a > b ? a : b;
				var min = a > b ? b : a;
				lfo[i].value = min + theta * (max-min);
				if (lfo[i].value != oldValue) {
					setValue(i, lfo[i].value);
				}
			}
			else {					
				var oldIndex = lfo[i].index;
				var oldValue = lfo[i].value;					
				var stepsElapsed = timeElapsed / lfo[i].speed;
				lfo[i].index = Math.floor(stepsElapsed % lfo[i].list.length);
				lfo[i].value = lfo[i].list[lfo[i].index];				

				// update if changed			
				if (lfo[i].value != oldValue) {
					setTopY(i, lfo[i].value);
				}				
			}			
		}
	}
	
}, 10);



