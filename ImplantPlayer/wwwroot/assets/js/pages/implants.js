

var IMPLANT_TEST = (document.location + '').indexOf('file') == 0;

if (IMPLANT_TEST) {
    IMPLANT_CMD_URL = 'testdata/implants.json';
}
else {
    IMPLANT_CMD_URL = '/settings/implants';
}



//-------------------------------------------------------------------
function ImplantManager() {
    var me = this;

    // load the modal window
    $.get('implants_window.html', function (html) {
        $('body').append(html);
        me.$modal = $('#implantWindow');
    });

    // jquery references to key elements
    this.$table = $('#implants');
    this.$tbody = $('#implants tbody');

    // the loaded data set
    this.items = [];

    // the current item being edited in the window
    this.editId = null;

    // load the list of available implant files
    this.implantFiles = [];
    this.getImplantList();

	// load the list of available mapping devices
	this.deviceList = [];
	this.getDeviceList();


    // load the latest data from the server
    this.load();
}

/**
  * All errors are sent to this method.
  */
ImplantManager.prototype.onError = function (msg) {
    alert('Error: ' + msg);
};

/**
  * Rebuild the data table from the this.items
  */
ImplantManager.prototype.fillTable = function () {
    this.$tbody.html('');
    for (var i = 0; i < this.items.length; i++) {
        this.createItemRow(this.items[i]).appendTo(this.$tbody);
    }

    if (this.items.length == 0) {
        this.$tbody.append('<tr><td colspan="10">No implants are loaded.</td></tr>')
    }
};

/**
  * Converts one data item to a new <tr> tag.
  */
ImplantManager.prototype.createItemRow = function (item) {
	var me = this;

	var areaText = '';
	if (item.range != null) {
		areaText += item.range.deviceName;
		if (item.range.children != null) {
			for (var i = 0; i < item.range.children.length; i++) {
				areaText += ', ' + item.range.children[i].deviceName;
			}
		}
	}


	function tableButton(text, btnClass) {
		btnClass = btnClass || ''
		return $('<button class="btn btn-xs ' + btnClass + '">' + text + '</button>');		
	}


	// build implant actions
	var $actionCell = $('<td class="actions">');
	if (item.status == 'running') {
		tableButton('Stop', 'btn-danger').appendTo($actionCell).click(function() { me.stop(item.id); });				
	}
	else if (item.status == 'loaded') {
		tableButton('Run', 'btn-success').appendTo($actionCell).click(function() { me.run(item.id); });		
	}	
	tableButton('Remove', 'btn-warning').appendTo($actionCell).click(function() { me.unload(item.id); });
	tableButton('Reload', 'btn-warning').appendTo($actionCell).click(function() { me.reload(item.id); });
	tableButton('Source', 'btn-info').appendTo($actionCell).click(function() { me.textEditor(item.id); });
	
	// build edit cell
	var $editCell = $('<td>');
	$('<a>Edit</a>').appendTo($editCell).click(function() { me.edit(item.id); });

	// build row
    var $row = $('<tr>');
    $editCell.appendTo($row);
    $('<td>' + item.mode + '</td>').appendTo($row);
    $('<td>' + item.vpath + '</td>').appendTo($row);
    $('<td>' + areaText + '</td>').appendTo($row);
    $('<td>' + item.oscFormat + '</td>').appendTo($row);
	var $tdStatus = $('<td class="status">').appendTo($row);
	$actionCell.appendTo($row);
	
	if (item.status == 'error') {
		$('<a>' + item.status + '</a>')
			.appendTo($tdStatus)
			.tooltip({
				title: 'Error: ' + item.error,
				placemenu: 'bottom'
			});
	}
	else {
		$('<b>' + item.status + '</b>').appendTo($tdStatus);
	}
	
	if (item.status == 'error') {
	   
	}

    return $row;
};

/**
  * Displays the modal window for loading a new item.
  */
ImplantManager.prototype.add = function () {
    this.$modal.find('.modal-title').html('Load New Implant');
    this.$modal.find('#btnSave').html('Load Implant');
    this.fillEditWindow(this.createItem());
    this.$modal.modal('show');
};

/**
  * Displays the modal window ready to edit an item.
  *
  * @param id the item to be edited.
  */
ImplantManager.prototype.edit = function (id) {
    this.$modal.find('.modal-title').html('Implant Settings');
    this.$modal.find('#btnSave').html('Save Changes');
    this.fillEditWindow(this.getItemById(id));
    this.$modal.modal('show');
};

/**
  * Returns an item from the data set in memory by id.
  *
  * @param id the item to load
  * @return the item if found, otherwise null
  */
ImplantManager.prototype.getItemById = function (id) {
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
ImplantManager.prototype.getEditItem = function () {
    if (this.editId == '*new*') {
        return this.createItem();
    }
    else {
        return this.getItemById(this.editId);
    }
};

/**
  * Saves changes and closes the modal window.
  */
ImplantManager.prototype.saveEdit = function () {

    // load item
    var item = this.getEditItem();
    if (item == null) {
        this.onError('BUG: could not load item being edited in window');
        return;
    }

// TODO: need to handle all children in the rangemap

	var $table = this.$modal.find('#mappingTable tbody');
	
	var isFirst = true;
	$table.find('tr').each(function() {
		var $this = $(this);
		var range = {};
	    range.x = $this.find('#x').val();
	    range.y = $this.find('#y').val();
	    range.virtualX = $this.find('#vx').val();
	    range.virtualY = $this.find('#vy').val();
	    range.width = $this.find('#w').val();
	    range.height = $this.find('#h').val();
		range.deviceName = $this.find('#device').val();
		range.children = [];
		
		if (isFirst) {
			item.range = range;
			isFirst = false;	
		}
		else {
			item.range.children.push(range);
		}		
		
	});


    // apply changes
    item.vpath = this.$modal.find('#ddlPath').val();
	item.mode = this.$modal.find('#txtMode').val();
    item.oscFormat = this.$modal.find('#txtOscFormat').val();

    // add to data list if new item
    if (item.id == '*new*') {
        this.editId = item.id = 'implant_' + Math.floor(Math.random() * 1000);
        this.items.push(item);
    }

    // update the UI and server
    this.save();
};

	
/**
  * Fills the modal window with the settings from an item.
  *
  * @param item the data to fill in the window
  */
ImplantManager.prototype.fillEditWindow = function (item) {
    this.editId = item.id;

	// test putting range map steps into a table
	var $table = this.$modal.find('#mappingTable tbody');
	$table.find('tr').remove();
	
	var me = this;
	function deviceDropDown($ddlDevice, deviceName) {
		$ddlDevice.find('option').remove();
	
		// fill the device list
		var hasDevice = false;
		for (var i = 0; i < me.deviceList.length; i++) {		
			$ddlDevice.append('<option>' + me.deviceList[i] + '</option>');
			if (me.deviceList[i] == deviceName) {
				hasDevice = true;
			}
		}
	
		// select device, add mapped device if not in list
		if (hasDevice == false) {
			$ddlDevice.append('<option>' + deviceName + '</option>');
		}
		$ddlDevice.val(deviceName);		
	}
	

	function mappingItemRow(range, index) {
		var $tr = $('<tr>').attr('id', 'range_' + index);		
		
		var $ddl = $('<select id="device">');
		deviceDropDown($ddl, range.deviceName);
		
		var $btn = $('<a>').html('x');//.addClass('btn btn-xs btn-danger');
		$btn.click(function() {
			me.removeMapping(index);
		});

		$('<td>').append($ddl).appendTo($tr);
//		$ddl.appendTo($tr);
//		$('<td>').html(range.deviceName).appendTo($tr);
		
		$('<td><input id="x" value="' + range.x + '"></td>').appendTo($tr);
		$('<td><input id="y" value="' + range.y + '"></td>').appendTo($tr);
		$('<td><input id="vx" value="' + range.virtualX + '"></td>').appendTo($tr);
		$('<td><input id="vy" value="' + range.virtualY + '"></td>').appendTo($tr);
		$('<td><input id="w" value="' + range.width + '"></td>').appendTo($tr);
		$('<td><input id="h" value="' + range.height + '"></td>').appendTo($tr);
		$('<td>').append($btn).appendTo($tr);
		return $tr;
	}
	

	
	$table.append(mappingItemRow(item.range, 0));
	if (item.range != null && item.range.children != null) {
		for (var i = 0; i < item.range.children.length; i++) {
			$table.append(mappingItemRow(item.range.children[i], i + 1));
		}
	}
	
	


	// TODO: need to support all steps in the range map	
	var deviceName = item.range.deviceName;
	var $ddlDevice = this.$modal.find('#ddlDevice');
	deviceDropDown($ddlDevice, deviceName);
	
	
	// fill other range properties
    this.$modal.find('#txtX').val(item.range.x);
    this.$modal.find('#txtY').val(item.range.y);
    this.$modal.find('#txtVX').val(item.range.virtualX);
    this.$modal.find('#txtVY').val(item.range.virtualY);
    this.$modal.find('#txtWidth').val(item.range.width);
    this.$modal.find('#txtHeight').val(item.range.height);



	// fill rest of window
    this.$modal.find('#txtMode').val(item.mode);
    this.$modal.find('#txtStatus').val(item.status);



    var $vpath = this.$modal.find('#ddlPath');
    $vpath.find('option').remove();
    for (var i = 0; i < this.implantFiles.length; i++) {
        $('<option>' + this.implantFiles[i] + '</option>').appendTo($vpath);
    }
    $vpath.val(item.vpath);

};

/**
  * Creates a new blank item.
  *
  * @returns a new item with id of '*new*'
  */
ImplantManager.prototype.createItem = function () {
    return {
        "id": "*new*",
        "vpath": "",
		"oscFormat" : "/fader/{x}",
		"mode": 0,
        "range": { "x": 0, "y": 0, "width": 7, "height": 7, "virtualX": 0, "virtualY": 0, children: [] },
        "status": "not loaded"
    }
};



/**
  * Adds a new device mapping to the display.  Does not save.
  */
ImplantManager.prototype.addMapping = function () {
	var item = this.getItemById(this.editId);
	item.range.children.push({ x: 0, y: 0, width: 0, height: 0, virtualX: 0, virtualY: 0 });
	this.fillEditWindow(item);
};

/**
  * Removes a device mapping from the display.  Does not save.
  * i=0 means the main range, while each of its children as represented
  * and their index + 1.
  */
ImplantManager.prototype.removeMapping = function(i) {	
	this.$modal.find('#range_' + i).remove();
};



/**
  * Replaces the entire table with a loading message.
  */
ImplantManager.prototype.showLoading = function () {
    this.$tbody.html('<tr><td colspan="10">Loading current settings...</td></tr>');
};

/**
  * Loads the current MIDI mappings from the server.
  */
ImplantManager.prototype.load = function () {
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
ImplantManager.prototype.ajax = function (cmd, data, callback) {
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
    if (IMPLANT_TEST) {
        $.get(IMPLANT_CMD_URL, callback);
    }
    else {
        $.ajax({ url: IMPLANT_CMD_URL, data: data })
			.done(function (res) { callback(res); })
			.fail(function (xhr, textStatus, errorThrown) { me.onError(xhr.responseText); });
    }
};

/**
  * Saves all changes to the MIDI mappings to the server.
  */
ImplantManager.prototype.save = function () {

    var item = this.getEditItem();


	//
	// Returns a string like the following format:
	//	Device|x|y|vx|vy|w|h
	//
	function flattenRange(range) {
		var parts = [];
		parts.push(range.deviceName);
		parts.push(range.x);
		parts.push(range.y);
		parts.push(range.virtualX);
		parts.push(range.virtualY);
		parts.push(range.width);
		parts.push(range.height);
		return parts.join('|');		
	}
	

    // flattening the data for now... it's just easier that way
    var data = {};
    data.id = item.id;
    data.vpath = item.vpath;
    data.status = item.status;
	data.mode = item.mode;
	data.oscFormat = item.oscFormat;
	
	if (item.range != null) {
		data.range = flattenRange(item.range);
		if (item.range.children != null) {
			for (var i = 0; i < item.range.children.length; i++) {
				data['range_' + i] = flattenRange(item.range.children[i]);
			}
		}
	}
    this.ajax('save', data);
};

/**
  * Gets the list of implants on the server.
  */
ImplantManager.prototype.getImplantList = function (cb) {
    var me = this;
    this.ajax('implantlist', null, function (res) {
        me.implantFiles = JSON.parse(res);
        if (typeof cb === 'function') {
            cb();
        }
    });
};


/**
  * Gets the list of devices for implants on the server.
  */
ImplantManager.prototype.getDeviceList = function (cb) {
    var me = this;
    this.ajax('devicelist', null, function (res) {
        me.deviceList = JSON.parse(res);
        if (typeof cb === 'function') {
            cb();
        }
    });
};

/**
  * Tells the host program to start running the current implant.
  */
ImplantManager.prototype.run = function (id) {
	id = id || this.editId;
    this.ajax('run', { id: id });
};

/**
  * Tells the host program to stop the selected implant.
  */
ImplantManager.prototype.stop = function (id) {
	id = id || this.editId;
    this.ajax('stop', { id: id });
};

/**
  * Unloads the implant currently displayed in the modal window.
  */
ImplantManager.prototype.unload = function (id) {
	id = id || this.editId;
    this.ajax('unload', { id: id });
};

/**
  * Reloads the implant from disk.
  */
ImplantManager.prototype.reload = function (id) {
	id = id || this.editId;
    this.ajax('reload', { id: id });
};

/**
  * Launches a text editor to edit the javascript for this implant
  * on the host computer.
  */
ImplantManager.prototype.textEditor = function (id) {
	id = id || this.editId;
    this.ajax('texteditor', { id: id });
};




var implantManager = new ImplantManager();


