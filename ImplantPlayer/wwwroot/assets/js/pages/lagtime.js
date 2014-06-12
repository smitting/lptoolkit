
var LAGTIME_CMD_URL = '/logs/lag';

// minimum msec between push data changes
var MIN_PUSHTIME = 1000;

var LOG_SIZE = 500;


var colors = [
	'red',
	'blue',
	'green',
	'yellow',
	'orange',
	'purple',
	'brown',
	'black'
];	
	var tagColors = {};
	var nextColorIndex = 0;


//-------------------------------------------------------------------
function LagTime() {
	var me = this;

    // jquery references to key elements
    this.$table = $('#lagtime');
    this.$tbody = $('#lagtime tbody');
	this.$key = $('#key tbody');

    // the loaded data set
    this.data = { Times: [], Average: 0 };


    // load the latest data from the server 
	if (MIN_PUSHTIME > 0) {
		setInterval(function() {
		    me.load();		
		}, MIN_PUSHTIME);
	}
    me.load();		
	
}	

LagTime.prototype.fillTable = function() {	
	
	// build the key
//	this.$key.html('');
	if (this.$keyrow == null) {
		this.$keyrow = $('<tr>').appendTo(this.$key);		
	}
	for (var i = 0; i < this.data.Tags.length; i++) {
		var tag = this.data.Tags[i];
		if (tagColors.hasOwnProperty(tag) == false) {
			tagColors[tag] = colors[nextColorIndex++];

			var $cell = $('<td>');
			$('<div class="keysquare">').css({background:tagColors[tag]}).appendTo($cell);
			$('<span>').html(tag).appendTo($cell);
			$cell.appendTo(this.$keyrow );
		}
	}
	
		
	// build the results with those colors
	this.$tbody.html('');	

	var $row = $('<tr>');
	$('<td>Average</td>').appendTo($row);
	$('<td>' + this.data.Average + ' msec</td>').appendTo($row);
	$row.appendTo(this.$tbody);
	
	for (var i = 0; i < this.data.Times.length; i++) {
		$row = $('<tr>');
		var $cell = $('<td colspan="2">').addClass('lagbar').appendTo($row);
		var color = tagColors[this.data.Tags[i]];
		var msec = this.data.Times[i];
		$('<div>').css({width: msec, background: color }).appendTo($cell);
		$row.appendTo(this.$tbody);
	}
};


/**
  * Replaces the entire table with a loading message.
  */
LagTime.prototype.showLoading = function() {
	this.$tbody.html('<tr><td colspan="10">Loading current settings...</td></tr>');	
};

/**
  * Loads the entire requested log and displays them sorted.
  */
LagTime.prototype.load = function() {
	var me = this;
    this.ajax(null, { count: LOG_SIZE }, function (res) {
        me.data = JSON.parse(res);				
		me.fillTable();
    });
};

LagTime.prototype.ajax = function (cmd, data, callback) {
    var me = this;

    // add default callback as needed
    if (typeof callback !== 'function') {
        callback = function (res) {
        };
    }

    // insert command to data when provided
    if (cmd != null) {
        data = data || {};
        data.cmd = cmd;
    }

    // make ajax call
       $.ajax({ url: LAGTIME_CMD_URL, data: data })
		.done(function (res) { callback(res); })
		.fail(function (xhr, textStatus, errorThrown) { me.onError(xhr.responseText); });
};




var lagtime = new LagTime();



