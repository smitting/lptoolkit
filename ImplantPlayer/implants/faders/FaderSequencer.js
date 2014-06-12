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
var onColor = 'orange';
var offColor = 'off';

// limnit fader range outside of menus
var minY = 1;
var maxY = implant.pads.height-1;
var height = implant.pads.height-1;


/**
  * Stores the number of faders and the maximum drawing range.
  */
var Range = {

	// the min and max y value for drawing faders
	faderValue: {
		min: 1,
		max: implant.pads.height - 1
	},
	
	// the min and max fader number
	faderIndex: {
		min: 0,
		max: implant.pads.width - 1
	},
	
	// the min and max x value for the sequencer menu
	menuX: {
		min: 0,
		max: implant.pads.width - 1
	},
	
	// the min and max y value for the sequencer menu	
	menuY: {
		min: 0,
		max: 0
	}
	
};

/**
  * Stores all colors used by implant.
  */
var Colors = {
	
	// color used by faders 
	fader: {
		on: 'orange',
		off: 'off'
	},
	
	// colors used by the sequencer menu
	menu: {
		background: 'yellow'
	}
};



// storage for fader values
var faderValues = [];
var faderTops = [];

// current LFO settings
var lfo = [];





// controls which faders are in which modes, since the menu can
// override normal fader functionality
var faderMode = [];
for (var x = 0; x < implant.pads.width; x++) {
	faderMode[x] = 'fader';
}


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

/**
  * All drawing to the launchpad is handled by this object.
  */
var Draw = {
	
	/**
	  * Paints all faders and sequences.
	  */
	repaint: function() {

		// draw sequencer menu
		this.sequenceMenu();

		// determine submode
		var submode = 'fader';
		
		// draw submode		
		if (submode == 'fader') {
			// draw each fader
			var x0 = Range.faderIndex.min;
			var x1 = Range.faderIndex.max;
			for (var x = x0; x <= x1; x++) {
				this.fader(x);
			}
		}
		else if (submode == 'sequencer') {
			// draw sequencer
			
		}		
	},
	
	/**
	  * Redraws one fader.
	  *
	  * @param x the fader to redraw.
	  */
	fader: function(x) {
		
		// get range and value as percentage
		var y0 = Range.faderValue.min;
		var y1 = Range.faderValue.max;
		var p = faderValues[x]; 
		var p0 = 1 - p;
		
		// tween
		var topY = Math.floor(y0 * p + y1 * p0);
		
		// draw
		for (var y = y0; y <= y1; y++) {
			var color = y < topY ? Colors.fader.off : Colors.fader.on;
			implant.pads.set(x, y, color);			
		}		
	},
	
	/**
	  * Redraws the sequencing window.  For now it's just a yellow 
	  * bar across the top.
	  */
	sequenceMenu: function() {
		
		// get range
		var x0 = Range.menuX.min;
		var x1 = Range.menuX.max;
		var y0 = Range.menuY.min;
		var y1 = Range.menuY.max;
		var color = Colors.menu.background;

		// draw
		for (var y = y0; y <= y1; y++) {
			for (var x = x0; x <= x1; x++) {
				implant.pads.set(x, y, color);
			}
		}
	}
}


// TODO: REPLACE ALL THIS CRAP!!!
// object dealing with rate and sequence menus.
var menu = {
	lastDraw: 0,
	lastColor: '',
	beat96: 0,
	draw: function() {
		var now = new Date().getTime();
		var color = (this.beat96%12) >= 6 ? 'off' : 'yellow';
		if (now - this.lastDraw > 1000 || color != this.lastColor) {			
			this.lastDraw = now;
			this.lastColor = color;
			for (var i = 0; i < implant.pads.width; i++) {
//				implant.pads.set(i,0,color);
			}
		}
	}
};
implant.pads.on('1/96', function(e) {
	menu.beat96 = e.value % 96;
	menu.draw();
});
// END OF CRAP



// first button being held down in a sequence
var sequenceStart = null;


/**
  * Sets all of the fader values to zero.
  */
function reset() {
	for (var x = 0; x < implant.pads.width; x++) {
		//	setValue(x, 0);
		// testing getting them from session
		var value = implant.session.get('fader' + x);	
		setValue(x,value);
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
	
	/*
	for (var y = minY; y <= maxY; y++) {
        var color = y < topY ? offColor : onColor;
        implant.pads.set(x, y, color);
	}	
	*/
	
	faderTops[x] = topY;
	faderValues[x] = getValueFromTopY(topY);
    implant.osc.set(x, 0, faderValues[x]); 
    implant.session.set('fader' + x, faderValues[x]);

	// draw just the changed fader
	Draw.fader(x);
}

function repaintFader(fader) {
	Draw.fader(fader);
}


/**
  * Returns what a fader value should be when given the highest y to
  * highlight on a fader.
  *
  * @param topY the highest y position lit up
  * @returns a value from 0 to 1
  */
function getValueFromTopY(topY) {
	return Math.floor(maxY - topY) / (height-1);	
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
	var fh = Math.floor(value * height);
	return maxY - fh;
}


// --- Initialization ---

reset();

this.on('devicechange', function(e) {

	reset();
});



// counts repeats
var lastPress = null;


/**
  * Event triggered whenever the fader area (not the menu) is pressed.
  * Simple presses set a constant fader value, but holding the first
  * button and then pressing others creates an OSC.
  */
function faderPress(e) {
	
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
}

// update button state upon release and watch for the end of a sequence
function faderRelease(e) {	
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
}


/**
  * Handles presses to the menu, which is the yellow bar at the top.
  */
function menuPress(e) {
	// for now we'll just put the fader in "menu" mode while held down and make the row yellow

	
	// TODO: replace in drawing code
	
	faderMode[e.x] == 'menu';
	for (var y = minY; y <= maxY; y++) {
//		implant.pads.set(e.x, y, 'yellow');
	}
}

function menuRelease(e) {
	
	
	faderMode[e.x] == 'fader';
	
	// TODO: replace in drawing code
	//repaintFader(e.x);
}



this.pads.on('press', function(e) {
	
	if (e.y == 0) {
		menuPress(e);
	}
	else if (e.y >= minY && e.y <= maxY && faderMode[e.x] == 'fader') {
		faderPress(e);
	}
	
});

this.pads.on('release', function(e) {
	
	if (e.y == 0) {
	    menuRelease(e);
	}
	else if (e.y >= minY && e.y <= maxY && faderMode[e.x] == 'fader') {
	    faderRelease(e);
	}

		
});


this.setInterval(function() {
	
	menu.draw();
	
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



// repaint whenever the mode changed
// TODO: when there's a new "shown" event use that instead
implant.mode.on('modechanged', function() {
	
	/*
	for (var x = 0; x < implant.pads.width; x++) {
		for (var y = 0; y < implant.pads.height; y++) {
			implant.pads.set(x,y,'off');
		}
	}
	*/
	
	if (implant.assignedMode == implant.mode.current) {
		Draw.repaint();
		implant.print('mode changed to this device.  repainting.')
	}
	else {
		implant.print('mode changed away so not redrawwing.');
	}
		
});







