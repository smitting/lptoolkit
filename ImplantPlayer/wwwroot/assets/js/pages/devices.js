


var DEVICE_TEST = (document.location + '').indexOf('file') == 0;

if (DEVICE_TEST) {
    DEVICE_CMD_URL = 'testdata/devices.json';
}
else {
    DEVICE_CMD_URL = '/settings/devices';
}





//-------------------------------------------------------------------
function DeviceManager() {
	var me = this;

    // load the modal window
    $.get('devices_window.html', function (html) {
        $('body').append(html);
        me.$modal = $('#deviceWindow');
    });

    // jquery references to key elements
    this.$table = $('#devices');
    this.$tbody = $('#devices tbody');

    // the loaded data set
    this.items = [];

    // the current item being edited in the window
    this.editId = null;


	// ways a device can be mapped
	this.getHardware(function (res) {
		me.fillTable();
	});
	this.mappings = ['loading...'];
//	this.mappings = [ '---', 'Pad Device', 'Knob Device', 'MIDI Keyboard', 'MIDI Output'];


    // load the latest data from the server
    this.load();
	
}	


/**
  * All errors are sent to this method.
  */
DeviceManager.prototype.onError = function (msg) {
    alert('Error: ' + msg);
};
	
/**
  * Creates a dropdown showing the available ways to map a MIDI 
  * device with the current setting selected.  An event is already
  * tied to the select to save the mapping change to the server.
  */	
DeviceManager.prototype.createMappingDropdown = function(selected) {
	var me = this;
	var $ret = $('<select>');
	for (var i = 0; i < this.mappings.length; i++) {
		$('<option>').html(this.mappings[i]).appendTo($ret);
	}
	
	// add to just this option if the current mapping no longer exists
	var exists = false;
	for (var i = 0; i < this.mappings.length; i++) {
		if (this.mappings[i] == selected) {
			exists = true;
			break;
		}
	}
	if (!exists) {
		$('<option>').html(selected).appendTo($ret);		
	}
	
	// set selected	
	$ret.val(selected);
	
	// change event sends AJAX to change the mapping
	$ret.change(function() {
		var $this = $(this);
		var id = $this.parents('[deviceid]').attr('deviceid');
		var value = $this.val();
		me.mapDevice(id, value);
	});
	return $ret;
}

DeviceManager.prototype.createEnableCheck = function(item) {
	var me = this
	
	var $ret = $('<input type="checkbox" />');
	if (item.enabled) {
		$ret.attr('checked','checked');
	}
	$ret.change(function() {
		var $this = $(this);
		var id = $this.parents('[deviceid]').attr('deviceid');
		var value = $this.is(':checked');
		me.enableDevice(id, value);
	});
	return $ret;
};

DeviceManager.prototype.fillTable = function() {	
	
	this.$tbody.html('');
	for (var i = 0; i < this.items.length; i++) {
		var item = this.items[i];
		
		var $row = $('<tr deviceid="' + item.id + '">');
		$row.append('<td>' + item.name + '</td>');
		$row.append('<td><input type="checkbox" ' + (item.hasInput ? 'checked' : '') + ' disabled /></td>')
		$row.append('<td><input type="checkbox" ' + (item.hasOutput ? 'checked' : '') + ' disabled /></td>')
		
		var $chkcell = $('<td>').appendTo($row);
		this.createEnableCheck(item).appendTo($chkcell);		
		var $cell = $('<td>').appendTo($row);
		this.createMappingDropdown(item.mappedAs).appendTo($cell);
		this.$tbody.append($row);
	}
	
	if (this.items.length == 0) {
		this.$tbody.append('<tr><td colspan="10">No MIDI devices are attached to this system.</td></tr>')
	}
};


/**
  * Replaces the entire table with a loading message.
  */
DeviceManager.prototype.showLoading = function() {
	this.$tbody.html('<tr><td colspan="10">Loading MIDI devices...</td></tr>');	
};


/**
  * Loads the current MIDI mappings from the server.
  */
DeviceManager.prototype.load = function () {
    var me = this;
    this.showLoading();

    this.ajax(null, null, function (res) {
        me.items = JSON.parse(res);
        me.fillTable();
    });
};

/**
  * Sends an ajax message to the host.  If a callback is not provided,
  * the default action is to reload the page and close the edit window
  * if it is open.
  */
DeviceManager.prototype.ajax = function (cmd, data, callback) {
    var me = this;

    // add default callback as needed
    if (typeof callback !== 'function') {
        callback = function (res) {
            // TODO: parse result to check for errors
            me.load();
        };
    }

    // insert command to data when provided
    if (cmd != null) {
        data = data || {};
        data.cmd = cmd;
    }

    // make ajax call
    if (DEVICE_TEST) {
        $.get(DEVICE_CMD_URL, callback);
    }
    else {
        $.ajax({ url: DEVICE_CMD_URL, data: data })
			.done(function (res) { callback(res); })
			.fail(function (xhr, textStatus, errorThrown) { me.onError(xhr.responseText); });
    }
};

/**
  * Gets the list of hardware device mappings.
  */
DeviceManager.prototype.getHardware = function(cb) {
	var me = this;
	this.ajax('interfaces', {}, function(res) {
		me.mappings = ['---'];
		var data = JSON.parse(res);
		for (var i = 0; i < data.length; i++) {
			me.mappings.push(data[i]);
		}
		if (typeof cb === 'function') {
			cb();
		}
	});	
};

/**
  * Sends a device change back to the host.
  *
  * @param id the device being changed
  * @param map the new mapping
  */
DeviceManager.prototype.mapDevice = function(id, map) {
//	alert('changing ' + id + ' to ' + map);
	this.ajax('map', { id: id, mappedAs: map });	
};

DeviceManager.prototype.enableDevice = function(id, enable) {
//	alert('changing ' + id + ' to ' + map);
	this.ajax('enable', { id: id, enabled: enable });	
};

/**
  * Tells the host to rescan for new devices.
  */
DeviceManager.prototype.refresh = function() {
	this.ajax('refresh');	
};

var deviceManager = new DeviceManager();


