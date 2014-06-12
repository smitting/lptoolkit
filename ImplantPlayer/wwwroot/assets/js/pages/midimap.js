
var MIDIMAP_TEST = (document.location + '').indexOf('file') == 0;

if (MIDIMAP_TEST) {
    MIDIMAP_CMD_URL = 'testdata/midimap.json';
}
else {
    MIDIMAP_CMD_URL = '/settings/midimap';
}




//-------------------------------------------------------------------
function MidiMap() {
	var me = this;

    // load the modal window
    $.get('midimap_window.html', function (html) {
        $('body').append(html);
        me.$modal = $('#addMidiModal');


		me.getOutputList(function() {
			var $ddl = me.$modal.find('#ddlMidiDestination');
			$ddl.find('option').remove();
			for (var i = 0; i < me.midiOutput.length; i++) {
				var $option = $('<option>' + me.midiOutput[i] + '</option>');
				$ddl.append($option);
			}
		});
		
	    me.getImplantList(function() {
			var $ddl = me.$modal.find('#ddlOscSource');
			$ddl.find('option').remove();
			for (var i = 0; i < me.implants.length; i++) {
				var $option = $('<option>' + me.implants[i] + '</option>');
				$ddl.append($option);
			}			
		});
    });

    // jquery references to key elements
    this.$table = $('#midimap');
    this.$tbody = $('#midimap tbody');

    // the loaded data set
    this.items = [];

    // the current item being edited in the window
    this.editId = null;


    // load the list of available implant instances (after modal HTML is complete)
    this.implants = [];

	// load the list of available midi output devices (after modal HTML is complete)
	this.midiOutput = [];

    // load the latest data from the server 
    this.load();
	
}	

MidiMap.prototype.fillTable = function() {	
	this.$tbody.html('');
	for (var i = 0; i < this.items.length; i++) {
		var item = this.items[i];
		
		var $row = $('<tr>');
		$row.append('<td><a href="javascript:midimap.edit(\'' + item.id + '\')">Edit</a>');
		$row.append('<td>' + item.oscAddress + ' [' + item.oscValueFrom + '-' + item.oscValueTo + ']</td>');
		$row.append('<td>' + item.oscSource + '</td>');
		$row.append('<td>' + item.midiType + ' #' + item.midiNote + ' [' + item.midiValueFrom + '-' + item.midiValueTo + ']</td>');
		$row.append('<td>' + item.midiDestination + '</td>');				
		this.$tbody.append($row);
	}
	
	if (this.items.length == 0) {
		this.$tbody.append('<tr><td colspan="10">No OSC to MIDI maps have been defined.</td></tr>')
	}
	
};

MidiMap.prototype.add = function() {	
	this.$modal.find('.modal-title').html('Add MIDI Map');
	this.$modal.find('#btnSave').html('Add MIDI Map');
	this.fillEditWindow(this.createItem());
	this.$modal.modal('show');
};

MidiMap.prototype.edit = function(id) {
	this.$modal.find('.modal-title').html('Edit MIDI Map');
	this.$modal.find('#btnSave').html('Save Changes');
	this.fillEditWindow(this.getItemById(id));
	this.$modal.modal('show');
};

MidiMap.prototype.saveEdit = function() {

    // load item
    var item = this.getEditItem();
    if (item == null) {
        this.onError('BUG: could not load item being edited in window');
        return;
    }
		
	var $wnd = this.$modal;
	item.oscAddress = $wnd.find('#txtOscAddress').val();
	item.oscValueFrom = $wnd.find('#txtOscValueFrom').val();
	item.oscValueTo = $wnd.find('#txtOscValueTo').val();
	item.oscSource = $wnd.find('#ddlOscSource').val();
	item.midiType = $wnd.find('#ddlMidiType').val();
	item.midiNote = $wnd.find('#txtMidiNote').val();
	item.midiValueFrom = $wnd.find('#txtMidiValueFrom').val();
	item.midiValueTo = $wnd.find('#txtMidiValueTo').val();
	item.midiDestination = $wnd.find('#ddlMidiDestination').val();
		

    // add to data list if new item
    if (item.id == '*new*') {
        this.editId = item.id = 'midimap_' + Math.floor(Math.random() * 1000);
        this.items.push(item);
    }

	this.fillTable();
	this.save();
	$wnd.modal('hide');
};

MidiMap.prototype.fillEditWindow = function(item) {	
		
    this.editId = item.id;
	
	var $wnd = this.$modal;
	$wnd.find('#txtOscAddress').val(item.oscAddress);
	$wnd.find('#txtOscValueFrom').val(item.oscValueFrom);
	$wnd.find('#txtOscValueTo').val(item.oscValueTo);
	$wnd.find('#ddlOscSource').val(item.oscSource);
	$wnd.find('#ddlMidiType').val(item.midiType);
	$wnd.find('#txtMidiNote').val(item.midiNote);
	$wnd.find('#txtMidiValueFrom').val(item.midiValueFrom);
	$wnd.find('#txtMidiValueTo').val(item.midiValueTo);
	$wnd.find('#ddlMidiDestination').val(item.midiDestination);
};

MidiMap.prototype.createItem = function() {
	return { 
		id: '*new*',
		oscAddress: '', 
		oscValueFrom: 0.0,
		oscValueTo: 1.0,
		oscSource: 'Any Implant',
		midiType: 'CC',
		midiNote: '1',
		midiValueFrom: 0,
		midiValueTo: 127,
		midiDestination: 'All Midi'
	};
};


/**
  * Gets the list of devices we can output to.
  */
MidiMap.prototype.getOutputList = function (cb) {
    var me = this;
    this.ajax('outputdevices', null, function (res) {
        me.midiOutput = JSON.parse(res);
        if (typeof cb === 'function') {
            cb();
        }
    });
};

/**
  * Gets the list of running implants on server.
  */
MidiMap.prototype.getImplantList = function (cb) {
    var me = this;
    this.ajax('implants', null, function (res) {
        me.implants = JSON.parse(res);
        if (typeof cb === 'function') {
            cb();
        }
    });
};



/**
  * Returns an item from the data set in memory by id.
  *
  * @param id the item to load
  * @return the item if found, otherwise null
  */
MidiMap.prototype.getItemById = function (id) {
    for (var i = 0; i < this.items.length; i++) {
        if (this.items[i].id == id) {
            return this.items[i];
        }
    }
    return null;
}; 

/**
  * Returns the item being edited in the modal window.
  */
MidiMap.prototype.getEditItem = function () {
    if (this.editId == '*new*') {
        return this.createItem();
    }
    else {
        return this.getItemById(this.editId);
    }
};


/**
  * Replaces the entire table with a loading message.
  */
MidiMap.prototype.showLoading = function() {
	this.$tbody.html('<tr><td colspan="10">Loading current settings...</td></tr>');	
};

/**
  * Loads the current MIDI mappings from the server.
  */
MidiMap.prototype.load = function() {
	this.showLoading();
	var me = this;
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
MidiMap.prototype.ajax = function (cmd, data, callback) {
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
    if (MIDIMAP_TEST) {
        $.get(MIDIMAP_CMD_URL, callback);
    }
    else {
        $.ajax({ url: MIDIMAP_CMD_URL, data: data })
			.done(function (res) { callback(res); })
			.fail(function (xhr, textStatus, errorThrown) { me.onError(xhr.responseText); });
    }
};

/**
  * Saves all changes to the MIDI mappings to the server.
  */
MidiMap.prototype.save = function () {
    var data = this.getEditItem();
    this.ajax('save', data);
};



var midimap = new MidiMap();
midimap.load();


