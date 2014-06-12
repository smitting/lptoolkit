
var LOGS_CMD_URL = '/logs/';

// maximum number of log entries
var MAX_LENGTH = 100;

// minimum msec between push data changes
var MIN_PUSHTIME = 250;




//-------------------------------------------------------------------
function Logs() {
	var me = this;

    // jquery references to key elements
    this.$table = $('#logs');
    this.$tbody = $('#logs tbody');
    this.$thead = $('#logs thead');

    // the loaded data set
    this.items = [];
	this.logtype = 'console';


    // load the latest data from the server 
    this.load();

	// reload when the dropdown changes
	$('#ddlLogType').on('change', function() {
		me.refresh();
	})
	
}	

Logs.prototype.fillTable = function() {	
	
	this.$thead.html('');
	switch (this.logtype) {
		case 'console':
			$('<th>Timestamp</th>').appendTo(this.$thead);
			$('<th>Source</th>').appendTo(this.$thead);
			$('<th>Message</th>').appendTo(this.$thead);
			break;
		case 'midi':
			$('<th>Timestamp</th>').appendTo(this.$thead);
			$('<th>Message</th>').appendTo(this.$thead);
			$('<th>Path</th>').appendTo(this.$thead);
			break;
		case 'osc':
			break;
	}
	
	function padLeft(s, length, c) {
		c = c || ' ';
		s = String(s);
		while (s.length < length) {
			s = c + s;
		}
		return s;
	}
	function padRight(s, length, c) {
		c = c || ' ';
		s = String(s);
		while (s.length < length) {
			s = s + c;
		}
		return s;
	}


	// format must be hh:mm:ss.tttt
	function splitTime(time) {
		var parts = time.split(':');
		if (parts.length != 3) return {};
		var subparts = parts[2].split('.');
		if (subparts.length != 2) return {};
		
		return {
			h: parts[0],
			m: parts[1],
			s: subparts[0],
			t: subparts[1]
		};		
	}
	
	// inserts " for all parts that are a repeat
	function timeDittos(now, last) {
		if (last == null) return now;
		if (now == last) return '"';
		
		var nowTime = splitTime(now);
		var lastTime = splitTime(last);
		
		if (nowTime.h != lastTime.h) {
			return nowTime.h + ':' + nowTime.m + ':' + nowTime.s + '.' + nowTime.t;
		}
		else if (nowTime.m != lastTime.m) {
			return '  :' + nowTime.m + ':' + nowTime.s + '.' + nowTime.t;
		}
		else if (nowTime.s != lastTime.s ) {
			return '     :' + nowTime.s + '.' + nowTime.t;
		}
		else if (nowTime.t != lastTime.t) {
			return '        .' + nowTime.t;
		}
		else {
			return '            ';
		}		
	}
	
	
	this.$tbody.html('');
	var lastTimestamp = null;
	
	var now = new Date().getTime();
	for (var i = 0; i < this.items.length; i++) {
		var item = this.items[i];
		
		var $row = $('<tr>');
		
		
		if (item.added && (now - item.added) < 1000) {
			$row.addClass('recent');
		}
		
		
		
		switch (this.logtype) {
			case 'console':
				$row.append('<td>' + item.Timestamp + '</td>');
				$row.append('<td>' + item.Source + '</td>');
				$row.append('<td>' + item.Message + '</td>');
				break;
			case 'midi':
				var timestamp = item.Timestamp;
				timestamp = timestamp.substring(timestamp.indexOf('T') + 1);
				timestamp = timestamp.substring(0, timestamp.indexOf('-'));
				
				var fulltime = timestamp;
				if (lastTimestamp != null) {
					timestamp = timeDittos(timestamp, lastTimestamp);
				}
				lastTimestamp = fulltime;
			
				var message = '';
				switch (item.Message.Type) {
					case 1:
						message = 'On';
						break;
					case 2:
						message = 'CC';
						break;
					default:
						message = '' + item.Message.Type;
						break;
				}				
				message += '-';
				message += padLeft(item.Message.Pitch, 3, ' ');
				message += padLeft(item.Message.Velocity, 5, ' ');
				
//				var path = (item.Incoming?'From':'To');
//				path += ' ' + item.Source;
				var path = item.Source + ' TO ' + item.Destination;
				
			
				$row.append('<td class="Timestamp">' + timestamp.replace(/ /g, '&nbsp;') + '</td>');
				$row.append('<td class="MidiMessage">' + message.replace(/ /g, '&nbsp;') + '</td>');
				$row.append('<td class="Path">' + path + '</td>');
				break;
		}

		this.$tbody.append($row);
	}
	
	if (this.items.length == 0) {
		this.$tbody.append('<tr><td colspan="10">This log is empty</td></tr>')
	}
	
};


/**
  * Replaces the entire table with a loading message.
  */
Logs.prototype.showLoading = function() {
	this.$tbody.html('<tr><td colspan="10">Loading current settings...</td></tr>');	
};

/**
  * Loads the entire requested log and displays them sorted.
  */
Logs.prototype.load = function() {
	this.showLoading();
	var me = this;
	this.logtype = $('#ddlLogType').val();
    this.ajax(this.logtype, null, null, function (res) {
	
		// display the most recent items
        me.items = JSON.parse(res);				
		me.sortAndDraw();

		// start doing push requests
		me.waitForPush();			
    });
};

/**
  * Sorts the current items, clips them at a max length, and then
  * draws them.
  */
Logs.prototype.sortAndDraw = function() {
	
	this.items.sort(function(a,b) {
		if (a.Ordinal > b.Ordinal) {
			return -1;
		}
		else if (a.Ordinal < b.Ordinal) {
			return 1;
		}
		return 0;			
	});
	
	// drop off data past last MAX_LENGTH message for efficiency
	if (this.items.length > MAX_LENGTH) {
		this.items.length = MAX_LENGTH;
	}
	
	// redraw
	this.fillTable();
};

Logs.prototype.waitForPush = function() {
	var me = this;
	
	// ignore repeat push requests
	if (me.pushRequestRunning) {
		return;
	}
	me.pushRequestRunning = true;

	// get the type of log being viewed
	var lastLogType = me.lastLogType;
	var logType = $('#ddlLogType').val() 
	
	// get next ordinal to load, if not switching log types
	var ordinal = 0;
	if (me.lastLogType == logType) {
		if (me.items != null && me.items.length > 0) {
			ordinal = me.items[0].Ordinal + 1;		
		}
	}
	else {
		// clear the list when we switch types
		me.items = [];	
	}
		
	// max out at rate set by MIN_PUSHTIME
	var delay = 1;
	if (me.lastPushRequest) {
		var now = new Date().getTime();
		if ((now - me.lastPushRequest) < MIN_PUSHTIME) {
			delay = MIN_PUSHTIME - (now - me.lastPushRequest);
		}
	}
	
	setTimeout(function() {
		me.lastPushRequest = new Date().getTime();
		
//		console.log('requesting ordinal #' + ordinal);

		// tell web server to respond when we have new data
	    me.ajax(logType, null, { i: ordinal }, function (res) {

//			console.log('drawing ordinal #' + ordinal);

			// add new logs to list
			try {
				var newItems = JSON.parse(res);		
				for (var i = 0; i < newItems.length; i++) {
					if (ordinal > 0) {
						newItems[i].added = new Date().getTime();
					}
					me.items.push(newItems[i]);
				}
				me.lastLogType = logType;
			}
			catch (ex) {
				console.error('parse error: ' + ex);			
			}

			// draw the sorted list
			try {
				me.sortAndDraw();
			}
			catch (ex) {
				console.error('render error: ' + ex);						
			}

			// do next push request
			me.pushRequestRunning = false;
			me.waitForPush();
		});
		
	}, delay);
};


Logs.prototype.refresh = function() {
	this.load();
};

/**
  * Sends an ajax message to the host.  If a callback is not provided,
  * the default action is to reload the page and close the edit window
  * if it is open.
  */
Logs.prototype.ajax = function (logtype, cmd, data, callback) {
    var me = this;

    // add default callback as needed
    if (typeof callback !== 'function') {
        callback = function (res) {
            // TODO: parse result to check for errors
            me.load();
            me.editId = null;
            me.$modal.modal('hide');
        };
    }

    // insert command to data when provided
    if (cmd != null) {
        data = data || {};
        data.cmd = cmd;
    }

    // make ajax call
       $.ajax({ url: LOGS_CMD_URL + logtype, data: data })
		.done(function (res) { callback(res); })
		.fail(function (xhr, textStatus, errorThrown) { me.onError(xhr.responseText); });
};




var logs = new Logs();



