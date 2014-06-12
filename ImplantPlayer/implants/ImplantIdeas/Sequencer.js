/**
  * Core sequencer implant.  Goal is to turn this into some sort of
  * standard include for other implants in the future.
  */


// TODO: allow this library to be includes like so:
//#include <Sequencer>


/**
  * Object that manages sequencer data in javascript, providing means
  * for drawing the data on a pad device, storing the sequences 
  * separately inside of a user session, and sending data over OSC.
  *
  * @param tracks the number of tracks in the sequence
  * @param length the number of values in each track initially.
  */
function SequencerData(tracks, length) {
	
	// setup empty arrays of desired size
	this.data = [];
	for (var track = 0; track < tracks; track++) {
		var trackdata = { steps: [] };
		for (var step = 0; step < length; step++) {
			trackdata.steps.push({ value: 0 });
		}
		this.data.push(trackdata);
	}
	
	// function that draws one pad from a data value.  can be changed
	// by client code
	this.drawStep = function(x, y, data, highlight) {
		var color;
		if (highlight) {
			color = data.value > 0 ? 'orange' : 'yellow';			
		}
		else {
			color = data.value > 0 ? 'green' : 'off';
		}
		implant.pads.set(x, y, color);
	};
	
	// current scrolling for the view
	this.view = { track: 0, offset: 0 };
	
	// current range to draw the sequencing data within.. default entire implant active area
	this.range = { x: 0, y: 0, width: implant.pads.width, height: implant.pads.height };
	
	// the location into the track currently being played
	this.cursor = 0;
	
}

/**
  * Returns an array a number of steps within a track at a certain
  * point within the track.  Writing to the objects within the array
  * changes the data stored within the sequencer.
  *
  * @param track the track to get data from
  * @offset where within the track to start getting data
  * @length the number of steps to return
  */
SequencerData.prototype.getTrackData = function(track, offset, length) {
	var ret = [];
	for (var i = 0; i < length; i++) {
		ret.push(this.data[track].steps[offset + i]);
	}
	return ret;
};

/**
  * Draws some of the values from one track on the connected pad 
  * device starting at a given x,y on the device at a given offset
  * into the data and then going right for a certain number of steps.
  *
  * @param padx starting location on the device
  * @param pady starting location on the device
  * @param track
  * @param offset
  * @param length
  */
SequencerData.prototype.paintTrack = function(padx, pady, track, offset, length) {
	var data = this.getTrackData(track, offset, length);
	for (var i = 0; i < length; i++) {
		this.drawStep(padx + i, pady, data[i]);
	}
	return data;
};

/**
  * Paints all of the tracks currently in view for the current settings.
  */
SequencerData.prototype.paint = function SequencerData_paint() {
	for (var y = 0; y < this.range.height; y++) {
		this.paintTrack(this.range.x, this.range.y + y, this.view.track + y, this.view.offset, this.range.width);
	}
};

/**
  * Updates the x/y scrolling on the hardware and repaints.
  */
SequencerData.prototype.scrollTo = function SequencerData_scrollTo(track, offset) {
	this.view.track = track;
	this.view.offset = offset;
	this.paint();
};

/**
  * Relative scrolling.
  */
SequencerData.prototype.scrollBy = function SequencerData_scrollBy(x, y) {
	this.scrollTo(this.view.track + y, this.view.offset + x);
};

/**
  * Handles a click event in the sequenced area.
  */
SequencerData.prototype.click = function SequencerData_click(e) {
	if (e.x < this.range.x) return;
	if (e.x >= this.range.x + this.range.width) return;
	if (e.y < this.range.y) return;
	if (e.y >= this.range.y + this.range.height) return;
	
	// find item
	var track = (e.y - this.range.y) + this.view.track;
	var step = (e.x - this.range.x) + this.view.offset;
	
	// toggle item
	var item = this.data[track].steps[step];
	if (item.value > 0) {
		item.value = 0;
	}
	else {
		item.value = 1;
	}
	
	// repaint item if onscreen
	this.drawStep(e.x, e.y, item);
};

/**
  * Returns true iff the step is currently visible
  */
SequencerData.prototype.isOnScreen = function SequencerData_isOnScreen(step) {
	if (this.view.offset > step) return false;
	if (step - this.view.offset > this.range.width) return false;
	return true;
};

SequencerData.prototype.drawHighlight = function SequencerData_drawHighlight(step) {
	var x = step - this.view.offset + this.range.x;
	for (var trackNumber = 0; trackNumber <= this.range.height; trackNumber++) {
		var y = this.range.y + trackNumber - this.view.track;
		

		var track = this.data[trackNumber + this.view.track];
		if (track == null) continue;
		var steps = track.steps;
		if (steps == null) continue;
		var item = steps[step];
		if (item != null) {
			this.drawStep(x, y, item, true);
		}
	}	
};

/**
  * Draws all of the sequencer data for a given vertical strip, 
  * undoing the drawing to highlight the play cursor.
  */
SequencerData.prototype.undrawHighlight = function SequencerData_undrawHighlight(step) {
	
	
	// ignore out of range requests
	if (step < 0) return;
	if (step >= this.getSongLength()) return;
	
	
	// draw step data visible along highlight line
	var x = step - this.view.offset + this.range.x;
	for (var trackNumber = 0; trackNumber <= this.range.height; trackNumber++) {
		var y = this.range.y + trackNumber - this.view.track;

		var track = this.data[trackNumber + this.view.track];
		if (track == null) continue;
		var steps = track.steps;
		if (steps == null) continue;
		var item = steps[step];
		if (item != null) {
			this.drawStep(x, y, item);
		}
	}
	
};

/**
  * Returns the number of steps in the sequence.
  */
SequencerData.prototype.getSongLength = function SequencerData_getSongLength() {	
 	return this.data[0].steps.length;
}

/**
  * Sends out notes for each active track and highlights it.
  */
SequencerData.prototype.playStep = function SequencerData_playStep(step) {
	
	// undo last highlight
	this.undrawHighlight(this.cursor);
	
	// change cursor
	var songLength = this.getSongLength();
	this.cursor = step % songLength;
	
	// play tracks on step
	for (var trackNumber = 0; trackNumber < this.data.length; trackNumber++) {
		var track = this.data[trackNumber];
		if (track == null) continue;
		
		var steps = track.steps;
		if (steps == null) continue;
		if (this.cursor >= steps.length) continue;
		
		var hit = steps[this.cursor].value > 0;
		if (hit) {
			implant.osc.set(track, 0, 1);
		}
	}

	// draw
	this.drawHighlight(this.cursor);
};



//---------[Implant Settings]--------------
/*
implant.register(function() {
	this.settings.defaultTitle = 'Sequencer Library Test';
	this.settings.defaultOscFormat = '/sequence/{y}/{x}';
	this.settings.activeArea = { x: 0, y: 0, width: 8, height: 8 };
	this.settings.midiMap.add({
		oscAddress: '/sequence/1/{x}'
		oscValueFrom: 0,
		oscValueTo: 1,
		midiType: 'noteon',
		midiNote: '0+{x}',
		midiValueFrom: 0,
		midiValueTo: 127		
	});
	this.settings.midiMap.add({
		oscAddress: '/sequence/0/{x}'
		oscValueFrom: 0,
		oscValueTo: 1,
		midiType: 'noteoff',
		midiNote: '0+{x}',
		midiValueFrom: 0,
		midiValueTo: 127		
	});
});
*/


//---------[Test]--------------
var seq = new SequencerData(8,8);
seq.range = { x: 0, y: 0, width: 8, height: 8 };
seq.paint();



// how much to devide 96 ticks by
var sequencerSpeed = 24;
var speeds = [ 
	3,	// 32nds	
	6,	// 16ths 
	9,  // 16th dotted
	12, // 8th
	18, // 8th dotted
	24, // quarter
	36, // dotted quarter
	48  // half
	]
	
function drawSpeedMenu() {	
	var x = 8;
	for (var y = 0; y < speeds.length; y++) {
		var color = sequencerSpeed == speeds[y] ? 'orange' : 'yellow';
		implant.pads.set(x, y, color);
	}	
}	
drawSpeedMenu();
	

implant.pads.on('press', function(e) {
	implant.print('set ' + e.x + ',' + e.y);
	if (e.x < 8) {
		seq.click(e);		
	}
	else {
		// speeds
		sequencerSpeed = speeds[e.y];
		drawSpeedMenu();
	}
});

implant.mode.on('modechanged', function(e) {
	seq.paint();
});



implant.time.on('1/96', function(e) {
	if ((e.x % sequencerSpeed) == 0) {
		seq.playStep(Math.floor(e.x / sequencerSpeed));
	}
});




