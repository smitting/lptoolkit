//
// File: 	Signature.js 
// Author: 	Scott Mitting
// Date: 	June 1, 2014
// Abstract:
//	Implant that displays the detected key signature for all the notes
// 	being played, along with all the notes.
//
//	TODO: support this for multiple users.
//
// Adding support for color coded messaging between all cluster members
// so everyone can request and acknowledge changes during an improvisational
// set like having a breakdown coming up, with a countdown.
//


var Colors = {
	majorKey: 'red',
	minorKey: 'orange',
	note: 'green',
	valid: 'yellow',
	invalid: 'off'	
};


var MyNode = implant.clusterNodeId;





// #include <KeySignature>
//**** TODO: Make this be an include file!!! **** 


/**
  * Takes an input that is either a MIDI pitch or a description of
  * a note like "C4" or "Bb3" and turns it into a MIDI pitch.
  */
function forceToMidiPitch(note) {
	
	if (typeof note === 'number') {
		return note;
	}
	
	if (typeof note === 'string') {
		
		// trim the string
		note = note.replace(/ /g, '');
		note = note.replace(/-/g, '');		
		note = note.toLowerCase();
		
		// last digit is the octave
		var octave = Number(note[note.length - 1]);
		var number = 0;
		var key = note.substring(0, note.length - 1);
		switch (key) {
			case 'c':
				number = 0;
				break;
			case 'c#':
			case 'db':
				number = 1;
				break;
			case 'd':
				number = 2;
				break;
			case 'd#':
			case 'eb':
				number = 3;
				break;
			case 'e':
				number = 4;
				break;
			case 'f':
				number = 5;
				break;
			case 'f#':
			case 'gb':
				number = 6;
				break;
			case 'g':
				number = 7;
				break;
			case 'g#':
			case 'ab':
				number = 8;
				break;
			case 'a':
				number = 9;
				break;
			case 'a#':
			case 'bb':
				number = 10;
				break;
			case 'b':
				number = 11;
				break;
			default:
				return null;
		}		
		
		return 12 + (octave * 12) + number;		
	}
	
	// not supported
	return null;	
}


// translations
var Translations = {
	'C': 	0,	
	'C#': 	1, 	'Db': 	1,
	'D': 	2,
	'D#': 	3,	'Eb': 	3,
	'E': 	4,
	'F': 	5,
	'F#': 	6, 	'Gb': 	6,
	'G': 	7,
	'G#': 	8, 	'Ab': 	8,
	'A': 	9,
	'A#': 	10, 'Bb': 	10,
	'B': 	11
};

var Scales = {
	// the base offsets for major and minor used to compute all other scales.
	root: {
		major: [ 0,   2,   4,   5,   7,   9,   11 ],		
	 	minor: [ 0,   2,   3,   5,   7,   8,   10 ]		
	},
};
// compute the other scales
for (var note in Translations) {
	var number = Translations[note];
	Scales[note] = { major: [], minor: [] };
	for (var i = 0; i < Scales.root.major.length; i++) {
		Scales[note].major.push((number + Scales.root.major[i]) % 12);
	}
	for (var i = 0; i < Scales.root.minor.length; i++) {
		Scales[note].minor.push((number + Scales.root.minor[i]) % 12);		
	}
}


//
// Splits a string like Cmaj into ['C','maj']
//
function splitKey(key) {
	var isMinor = key.indexOf('min') > -1;
	key = key.replace('maj', '');
	key = key.replace('min', '');
	return [key, isMinor?'min':'maj'];
}

function isValid(x, y) {
	if (y == 1) {
		return true;
	}
	if (x == 2) return false;
	if (x > 5) return false;
	return true; 	
}

// for painting a keyboard on a launchpad (as best we can)
var locations = [
	{ x: 0, y: 1 }, // C
	{ x: 0, y: 0 }, // C#
	{ x: 1, y: 1 }, // D
	{ x: 1, y: 0 }, // D#
	{ x: 2, y: 1 }, // E
	{ x: 3, y: 1 }, // F
	{ x: 3, y: 0 }, // F#
	{ x: 4, y: 1 }, // G
	{ x: 4, y: 0 }, // G#
	{ x: 5, y: 1 }, // A
	{ x: 5, y: 0 }, // A#
	{ x: 6, y: 1 }  // B
];



/*
 * Object that receives a set of midi pitches and returns the most 
 * likely key signature from those notes.  Just keeps it simple, 
 * returning the base pitch major or minor.
 *
 * @param nodeId	the cluster node this key signature is from
 * @param y			where on the launchpad to paint
 */
function KeySignature(nodeId, y) {
	this.nodeId = nodeId;
	this.notes = [];
	this.y = y == null ? 0 : y;
}

/**
  * Adds a note as being held down, each by name or MIDI value.
  * 
  * @note - if a number, the midi pitch.  otherwise something like 'C4'
  */
KeySignature.prototype.add = function KeySignature_add(note) {
	note = forceToMidiPitch(note);
	if (note == 0) return;
	for (var i = 0; i < this.notes.length; i++) {
		if (this.notes[i] == note) {
			return;
		}
	}	
	this.notes.push(note);
	this.notes.sort();
};


KeySignature.prototype.remove = function KeySignature_remove(note) {
	note = forceToMidiPitch(note);
	for (var i = 0; i < this.notes.length; i++) {
		if (this.notes[i] == note) {
			this.notes.splice(i, 1);
			return;
		}
	}	
};

/**
  * Converts the contents of this object to a string for debugging.
  */
KeySignature.prototype.debug = function KeySignature_debug() {
	return '[nodeId=' + this.nodeId + ', notes=' + JSON.stringify(this.notes) + ']';
};

/**
  * Replaces all of the notes in this object with the supplied array.
  */
KeySignature.prototype.set = function KeySignature_set(notes) {
	this.notes = [];
	for (var i = 0; i < notes.length; i++) {
		var note = forceToMidiPitch(notes[i])
		if (note != 0) {
			this.notes.push(note);
		}
	}
	this.notes.sort();
};

/**
  * Returns all valid keys that the notes could belong to.
  */
KeySignature.prototype.keys = function KeySignature_keys() {
	
//	console.log('searching for = ' + JSON.stringify(this.notes));
	
	// find all of the keys that every note is in
	var candidates = [];
	for (var key in Scales) {
		if (key == 'root') continue;
		var major = Scales[key].major;
		var minor = Scales[key].minor;
		
		var found = true;
		for (var j = 0; j < this.notes.length; j++) {
			var searching = this.notes[j] % 12;
			var foundNote = false;
			for (var i = 0; i < major.length; i++) {
				if (major[i] == searching) {
					foundNote = true;
					break;
				}
			}
//				console.log('searching for ' + searching + ' in ' + key + ' major = ' + major + " FOUND=" + foundNote);
			if (foundNote == false) {
				found = false; 
				break;
			}					
		}
		if (found) {
			candidates.push({ name: key + 'maj', notes: major });
		}

		found = true;
		for (var j = 0; j < this.notes.length; j++) {			
			var searching = this.notes[j] % 12;
			var foundNote = false;
			for (var i = 0; i < minor.length; i++) {
				if (minor[i] == searching) {
					foundNote = true;
					break;
				}
			}
//					console.log('searching for ' + searching + ' in ' + key + ' minor = ' + minor + " FOUND=" + foundNote);

			if (foundNote == false) {
				found = false; 
				break;
			}
		}
		if (found) {
			candidates.push({ name: key + 'min', notes: minor });
		}
	}
	
	return candidates;
	
};



/**
  * Computes the most likely key signature from the current keys.
  * This is pretty simple. The Krumhansl-Schmuckler key-finding 
  * algorithm would be better, but likely more slow.  I may just
  * implement this in the API.
  */
KeySignature.prototype.key = function KeySignature_key() {
	if (this.notes.length == 0) return null;
	
	var candidates = this.keys();
	if (candidates == null || candidates.length == 0) return null;
	
	// just return the chord where the first note is equal to the lowest note we have
	for (var i = 0; i < this.notes.length; i++) {
		var lowNote = this.notes[i] % 12;
		for (var cindex = 0; cindex < candidates.length; cindex++) {
			var candidate = candidates[cindex].notes;
			if (candidate.length == 0) continue;
			if (candidate[0] == lowNote) return candidates[cindex].name;
		}
	}	

	return candidate[0].name;
};

/**
  * Paints this key signature on the launchpad.
  */
KeySignature.prototype.paint = function KeySignature_paint() {
	var me = this;
	
	// get new signature
	var key = this.key();
	
	function set(array, x, y, c) {
		for (var i = 0; i < array.length; i++) {
			if (array[i].x == x && array[i].y == y) {
				array[i].c = c;
			}
		}
	}
	
	function send(array) {
		for (var i = 0; i < array.length; i++) {
			implant.pads.set(array[i].x, me.y + array[i].y, array[i].c);
		};
	}
	
	// set initial values to valid or invalid
	var grid = [];	
	for (var y = 0; y <= 1; y++) {
		for (var x = 0; x <= 6; x++) {
			var c = isValid(x, y) ? Colors.valid : Colors.invalid;
			grid.push({x:x,y:y,c:c});
		}
	}
	
	// set all notes that are held
	for (var i = 0; i < this.notes[i]; i++) {
		var note = this.notes[i] % 12;
		var loc = locations[note];
		set(grid, loc.x, loc.y, Colors.note);		
	}
	
	// set the key signature
	if (key != null) {
		var _key = splitKey(key);
		var note = forceToMidiPitch(_key[0] + '0') % 12;
		var c = _key[1] == 'maj' ? Colors.majorKey : Colors.minorKey;
		var loc = locations[note];
		set(grid, loc.x, loc.y, c);		
	}
	
	
	// paint
	send(grid);
	
};


/*** END OF INCLUDE ============================ */





/*** START OF DISPLAY FOR KEY SIGNATURE ========= */


/*** CLUSTER COMMUNICATONS FOR SIGNATURE ============*/

/**
  * Manages a series of Signature objects, each identified by the
  * cluster node they were received of, and paints each set in
  * a separate area of the grid.
  */
function SignatureManager() {
	this.nextSignatureY = 1;
	
	// the current nodes, which contains the signature object and painting data
	this.nodes = {};	
}

/**
  * Returns the node object for a given clsuter node, creating
  * new instances as necessary.
  */
SignatureManager.prototype.getNodeObject = function SignatureManager_getNodeObject(nodeId) {
	
	var ret = this.findNode(nodeId);
	if (ret == null) {
		ret = this.nodes[nodeId] = new KeySignature(nodeId, this.nextSignatureY);
		implant.print('Added node ' + nodeId + ' at ' + ret.y);
		this.nextSignatureY += 3;		
	}
	return ret;
};

/**
  * Returns the node object only if it exists
  */
SignatureManager.prototype.findNode = function SignatureManager_getNodeObject(nodeId) {	
	for (var key in this.nodes) {
		if (key == nodeId) {
			return this.nodes[key];
		}
	}
	return null;
};


/**
  * Sends the current set of notes for a given node via OSC.
  */
SignatureManager.prototype.sendOSC = function SignatureManager_sendOsc(nodeId) {
	var node = this.findNode(nodeId);	
	if (node != null) {
		implant.osc.send('/keys/' + nodeId, node.notes);
	}
};

/**
  * Changes the notes from a remove OSC message, if it's valid.  
  * Returns the node id when valid.
  */
SignatureManager.prototype.receiveOSC = function SignatureManager_receiveOsc(e) {
	if (e.address == null || e.address.indexOf('/keys/') != 0) {
		return null;
	}
	
	var nodeId = e.address.substring(6);
	var node = this.getNodeObject(nodeId);	

	// parse values string
	var strNotes = String(e.values).split(',');
	var notes = [];
	for (var i = 0; i < strNotes.length; i++) {
		notes.push(Number(strNotes[i]));
	}
	
	// set and paint
	node.set(notes);
	this.paint();
	
	return nodeId;
};



/**
  * Paints all the current signatures.
  */
SignatureManager.prototype.paint = function SignatureManager_paint() {
	
	// paint over 
	for (var key in this.nodes) {
//		implant.print(this.nodes[key].debug());
		this.nodes[key].paint();
	}
};









/*** CLUSTER COMMUNICATONS AND VOTING ================*/

// the current colors for messaging
var messageColors = [
	'green', 	
	'red',
	'orange',
	'yellow'
];


/**
  * Handles one voting line on a launchpad.
  */
function ClusterVoting(y) {
	
	// can be set by manager class to change behavior
	this.isRemote = false;
	
	// the row to paint this record on
	this.y = y == null ? 0 : y;
	
	// set to a color when voting on something
	this.voteColor = null;

	// the message color currently being counted down
	this.countdownColor = null;

	// how many beats are left
	this.countdownBeat = -1;

	// the countdown that just finished!
	this.actionColor = null;
	this.actionBeat = -1;
	
}

/**
  * Gets the current mode this line is in based on the variables
  */
ClusterVoting.prototype.getMode = function ClusterVoting_getMode() {
	
	if (this.actionColor != null) return 'ACTION';
	if (this.countdownColor != null) return 'COUNTDOWN';
	if (this.voteColor != null) return 'VOTING';
	return 'SELECTION';	
};

/**
  * Copies the data from another instance to this one.
  */
ClusterVoting.prototype.sync = function ClusterVoting_sync(other) {
	this.actionColor = other.actionColor;
	this.actionBeat = other.actionBeat;
	this.countdownColor = other.countdownColor;
	this.countdownBeat = other.countdownBeat;
	this.voteColor = other.voteColor;
	this.paint();	
};

/**
  * Starts the countdown with the color we were voting on.
  */
ClusterVoting.prototype.startCountdown = function ClusterVoting_startCountdown() {
	this.countdownColor = this.voteColor;
	this.countdownBeat = 8 * 4;
	this.voteColor = null;	
};

/**
  * Starts blinking that the countdown was done.
  */
ClusterVoting.prototype.startAction = function ClusterVoting_startAction() {	
	this.actionColor = this.countdownColor;
	this.actionBeat = 32;
	this.countdownColor = null;
};

/**
  * Takes the voting back to selection mode.
  */
ClusterVoting.prototype.resetToSelection = function ClusterVoting_resetToSelection() {	
	this.actionColor = null;
	this.countdownColor = null;
	this.voteColor = null;
};

/**
  * Sends this user's vote to the cluster.
  */
ClusterVoting.prototype.sendVoteOSC = function ClusterVoting_sendVoteOSC(vote) {
	implant.print('My Vote was ' + vote);
	implant.osc.send('/vote/' + MyNode, [ vote] );
};


/**
  * Paints this voting row, which acts differently for local and remote
  * voting rows.
  */
ClusterVoting.prototype.paint = function ClusterVoting_paint() {
	
	var me = this;
	
	//
	// When in action mode, flash between the color and off
	// on the whole row.
	//
	function paintAction() {		
		var y = me.y;		
		var c = me.actionBeat % 2 == 0 ? me.actionColor : 'off';
		if (!c || c == 'undefined') c = 'off';
		for (var x = 0; x < implant.pads.width; x++) {	
			implant.pads.set(x, y, c);
		}		
	}
	
	function paintCountdown() {
		
		var y = me.y;		
		for (var x = 0; x < implant.pads.width; x++) {
			var c = x <= (me.countdownBeat / 4) ? me.countdownColor : 'off';

			if (x == Math.floor(me.countdownBeat / 4)) {
				if (me.countdownBeat % 2 == 1) c = 'off';
			}

			if (!c || c == 'undefined') c = 'off';
			implant.pads.set(x, y, c);
		}
	}
	
	//
	// Voting mode shows a green "yes" and a red "no" as the last
	// two pads, with one off pad for padding, and the rest of the
	// row is the color we're voting on.
	//
	// Remote rows are just empty until we get an OSC message
	// saying the way they voted.
	//
	function paintVoting() {
		var y = me.y;		
		var MAX_X = implant.pads.width - 1;		
		for (var x = 0; x < implant.pads.width; x++) {
			var c = 'off';

			if (me.isRemote) {
				// TODO: if remote clicked show green
				if (me.remoteYes) {
					c = 'green';
				}				
			}
			else {
				c = me.voteColor;
				if (x == MAX_X) { // no
					c = 'red';
				}
				else if (x == MAX_X - 1) { // yes
					c = 'green';
				}
				else if (x == MAX_X - 2) { // padding
					c = 'off';
				}
			}
			if (!c || c == 'undefined') c = 'off';
			implant.pads.set(x, y, c);			
		}
		
	}
	
	//
	// Selection mode just paints one pad each of the available colors.
	//
	// Don't show anything on remote rows.
	//
	function paintSelection() {	
		
		var y = me.y;
		for (var x = 0; x < implant.pads.width; x++) {
			var c = 'off';
			if (me.isRemote) {
				
			}	
			else {		
				if (x < messageColors.length) {
					c = messageColors[x];
				}
			}
			if (!c || c == 'undefined') c = 'off';	
			implant.pads.set(x, y, c);			
		}
	}
	
	// call the appropriate paint method
	switch (this.getMode()) {
		case 'ACTION': 		return paintAction();
		case 'COUNTDOWN': 	return paintCountdown();
		case 'VOTING': 		return paintVoting();
		case 'SELECTION':
		default: 			return paintSelection();
	}
};

/**
  * Handles 1/96 tick event for this object.
  */
ClusterVoting.prototype.onBeat = function ClusterVoting_onBeat(e) {
	var me = this;
	
	if (this.getMode() != 'VOTING') {
		this.remoteYes = null;
	}

	// remote nodes just do data maintenance
	if (this.isRemote) {
		return;
	}
		
	//
	// Drops down the count every 1/16th note until its done an
	// then goes into selection mode.
	//
	function beatAction() {
		if ((e.x % 3) == 0) {
			me.actionBeat--;
			if (me.actionBeat < 0) {
				me.resetToSelection();
			}
			me.paint();
		}		
	}
	
	//
	// Drops down the count down every beat until it's done and
	// then goes into action mode.
	//
	function beatCountdown() {
		if ((e.x % 6) == 0) {
			me.countdownBeat--;
			if (me.countdownBeat < 0) {
				me.startAction();
			}
			me.paint();
		}		
	}
	function beatVoting() {
		// TODO: maybe blink or something
	}
	function beatSelection() {
		
	}	
	
	switch (me.getMode()) {		
		case 'ACTION': 		return beatAction();
		case 'COUNTDOWN': 	return beatCountdown();
		case 'VOTING': 		return beatVoting();
		case 'SELECTION':
		default: 			return beatSelection();
	}	
};

/**
  * Handles pad press events for this object.
  */
ClusterVoting.prototype.onPress = function ClusterVoting_onPress(e) {
	
	var me = this;
	
	// ignore wrong row
	if (this.y != e.y) {
		return;
	}
	
	// ignore other cluster's voting
	if (this.isRemote) {
		return;
	}
	
	
	//
	// Not much to do while waiting for the countdown.
	//
	function pressCountdown() {
		
	}
	
	//
	// Watch for yes or no.
	//
	function pressVoting() {		
		var MAX_X = implant.pads.width - 1;
		if (e.x == MAX_X) { // no
			
			// cancel and tell others to cancel
			me.remoteYes = 0;
			me.resetToSelection();
			me.sendVoteOSC(0);
		}
		else if (e.x == MAX_X - 1) { // yes
						
			// tell others, wait until we all voted to go into a countdown
			me.remoteYes = 1;
			me.sendVoteOSC(1);			
			
		}
		
		me.paint();
	}
	
	//
	// If a valid message color is pressed, use it in voting mode
	//
	function pressSelection() {
		if (e.x < 0) return;
		if (e.x >= messageColors.length) return;
		me.voteColor = messageColors[e.x];		
		me.paint();		
	}
	
	// call the appropriate method
	switch (me.getMode()) {
		case 'COUNTDOWN': 	return pressCountdown();
		case 'VOTING': 		return pressVoting();
		case 'SELECTION':
		default: 			return pressSelection();
	}
};




/*** CLUSTER VOTING MANAGEMENT ================*/

/**
  * Creates an object to manage voting on different nodes, linked to a 
  * SignatureManager and places the voting right above its signature.
  */
function VotingManager(smgr) {
	this.nodes = {};
	this.signatureManager = smgr;
}

/**
  * Passes the 1/96 tick to the correct objects.
  */
VotingManager.prototype.onBeat = function VotingManager_onBeat(e) {
	for (var key in this.nodes) {
		this.nodes[key].onBeat(e);
	}
	
	// watch for situation where all nodes have voted to proceed
	var startCountdown = true;
	for (var key in this.nodes) {
		
		// TESTING - not watching for remote nodes yet
//		if (key != MyNode) continue;
		
		var node = this.nodes[key];
		if (node.remoteYes != 1 || node.getMode() != 'VOTING') {
			startCountdown = false;
			break;
		}
	}
	if (startCountdown) {
		for (var key in this.nodes) {
			this.nodes[key].startCountdown();
		}
	}
	
	// sync up nodes to local node
	var local = this.nodes[MyNode];
	if (local != null) {		
		for (var key in this.nodes) {
			if (key != MyNode) {
				this.nodes[key].sync(local);
			}
		}
	}	
};


/**
  * Passes the pad press to the correct objects.
  */
VotingManager.prototype.onPress = function VotingManager_onPress(e) {
	// only pass on event if this is the local node
	for (var key in this.nodes) {
		this.nodes[key].onPress(e);
	}	
};


/**
  * Paints all the voting objects.
  */
VotingManager.prototype.paint = function VotingManager_paint(e) {
	for (var key in this.nodes) {
		this.nodes[key].paint(e);
	}	
};


/**
  */
VotingManager.prototype.receiveOSC = function VotingManager_receiveOSC(e) {
	if (e.address == null || e.address.indexOf('/vote/') != 0) {
		return null;
	}
		
	var nodeId = e.address.substring(6);
	var node = this.findNode(nodeId);	
	
	if (node != null) {
		var strValues = String(e.values).split(',');
		if (strValues[0] == '1') {
			implant.print('Node ' + nodeId + ' voted yes:::' + e.values);
			node.remoteYes = 1;
		}
		else if (strValues[0] == '0') {
			implant.print('Node ' + nodeId + ' voted no:::' + e.values);			
			node.remoteYes = 0;
		}
	}
};



/**
  * Returns the voting object for a given node if it exists.
  */
VotingManager.prototype.findNode = function VotingManager_findNode(nodeId) {
	if (this.nodes.hasOwnProperty(nodeId)) {
		return this.nodes[nodeId];
	}
	return null;
};

/**
  * Adds a voting object for a cluster node if it does not already
  * exist, at a location just above its keyboard.
  */
VotingManager.prototype.initNode = function VotingManager_initNode(nodeId) {
	var node = this.findNode(nodeId);
	if (node == null) {
		// find the existing key
		var snode = this.signatureManager.findNode(nodeId);
		if (snode != null) {
			var node = new ClusterVoting(snode.y - 1);
			node.isRemote = nodeId != MyNode;
			node.paint();
			this.nodes[nodeId] = node;
		}
	}	
};








/*** MAIN PROGRAM INITIALIZATION ================*/

implant.print('starting Signature.js');


// clear
for (var y = 0; y < implant.pads.height; y++) {
	for (var x = 0; x < implant.pads.width; x++) {
		implant.pads.set(x, y, 'off');
	}
}


// setup key manager over the cluster
var smgr = new SignatureManager();
implant.keys.on('noteon', function(e) {
	var signature = smgr.getNodeObject(MyNode);
	signature.add(e.x);
	smgr.sendOSC(MyNode);
	smgr.paint();
});
implant.keys.on('noteoff', function(e) {
	var signature = smgr.getNodeObject(MyNode);
	signature.remove(e.x);
	smgr.sendOSC(MyNode);
	smgr.paint();
});
implant.osc.on('change', function(e) {
	var nodeId = smgr.receiveOSC(e);
	if (nodeId != null) {
		vmgr.initNode(nodeId);
	}
});
smgr.paint();




// setup test and events
var vmgr = new VotingManager(smgr);
implant.time.on('1/96', function (e) {
	vmgr.onBeat(e);
});
implant.pads.on('press', function(e) {
	vmgr.onPress(e);
});
implant.osc.on('change', function(e) {
	vmgr.receiveOSC(e);
});


// init local nodes
smgr.getNodeObject(MyNode);
vmgr.initNode(MyNode);

// paint
smgr.paint();
vmgr.paint();





