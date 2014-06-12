

var fakeData = [
	{
		source: { head: 'System', text: 'Load Implant '},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Server Init' },
				implant: { head: 'Implant', text: 'MyImplant.js' }				
			}			
		],	
	},
	{
		source: { head: 'LaunchPad', text: 'Note ON 0 = 127'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'PadPress(0,0)' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,0) = green' },
						target: { head: 'LaunchPad', text: 'Note ON 0 = 40' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,1) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 16 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,2) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 32 = 12' }
					},			
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,3) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 48 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,4) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 64 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,5) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 80 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,6) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 96 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,7) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 112 = 12' }
					}					
				]
			}			
		]
	},
	{
		source: { head: 'LaunchPad', text: 'Note ON 0 = 0'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'PadRelease(0,0)' },
				implant: { head: 'Implant', text: 'MyImplant.js' }
			}
		]
	},
	
	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 0' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/32/1' },
						target: { head: 'Battery 3', text: 'Note On 32 = 127' }
					}
				]
			}
		]
	},
	
	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 4' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/32/0' },
						target: { head: 'Battery 3', text: 'Note Off 32 = 0' }
					}
				]
			}
		]
	},

	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 96' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/32/1' },
						target: { head: 'Battery 3', text: 'Note On 32 = 127' }
					}
				]
			}
		]
	},
	
	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 100' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/32/0' },
						target: { head: 'Battery 3', text: 'Note Off 32 = 0' }
					}
				]
			}
		]
	},
	{
		source: { head: 'LaunchPad', text: 'Note ON 16 = 127'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'PadPress(0,1)' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,0) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 0 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,1) = green' },
						target: { head: 'LaunchPad', text: 'Note ON 16 = 40' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,2) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 32 = 12' }
					},			
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,3) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 48 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,4) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 64 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,5) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 80 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,6) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 96 = 12' }
					},
					{ 
						outevent: { head: 'XYEvent', text: 'pads(0,7) = off' },
						target: { head: 'LaunchPad', text: 'Note ON 112 = 12' }
					}					
				]
			}			
		]
	},
	{
		source: { head: 'LaunchPad', text: 'Note ON 16 = 0'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'PadRelease(0,1)' },
				implant: { head: 'Implant', text: 'MyImplant.js' }
			}
		]
	},
	
	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 192' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/33/1' },
						target: { head: 'Battery 3', text: 'Note On 33 = 127' }
					}
				]
			}
		]
	},
	
	{
		source: { head: 'System', text: 'BeatSync'},
		input: [
			{
				inevent: { head: 'ImplantEvent', text: 'Tick96 = 196' },
				implant: { head: 'Implant', text: 'MyImplant.js' },
				output: [
					{
						outevent: { head: 'OSCEvent', text: '/note/33/0' },
						target: { head: 'Battery 3', text: 'Note Off 33 = 0' }
					}
				]
			}
		]
	}
	
			
];

function isArray(obj) {
	return obj && typeof obj.length === 'number';
}

function makeArray(obj) {
	return isArray(obj) ? obj : [ obj ];
}

//
// Converts the branching sets of events into an array of data nodes
// of type { source, input, implant, output, target } with each property
// having the type { head, text}.
//
function toGrid(data) {
	var ret = [];
	data = makeArray(data);
	for (var i = 0; i < data.length; i++) {
		var item = data[i];
//		var row = { source: item.source };
		item.input = makeArray(item.input);
		
		for (var inputIndex = 0; inputIndex < item.input.length; inputIndex++) {
			var inputItem = item.input[inputIndex];			
			if (inputItem.output == null) {				
				var newItem = {};
				newItem.implant = inputItem.implant;
				newItem.input = inputItem.inevent;
				newItem.source = item.source;
				ret.push(newItem);
			}
			else {
				inputItem.output = makeArray(inputItem.output);
				for (var outputIndex = 0; outputIndex < inputItem.output.length; outputIndex++) {
					var outputItem = inputItem.output[outputIndex];
					outputItem.target = makeArray(outputItem.target);
					for (var targetIndex = 0; targetIndex < outputItem.target.length; targetIndex++) {
						var targetItem = outputItem.target[targetIndex];
					
						var newItem = {};
						newItem.target = targetItem;
						if (targetIndex == 0) {
							newItem.output = outputItem.outevent;
							if (outputIndex == 0) {
								newItem.implant = inputItem.implant;
								newItem.input = inputItem.inevent;
								if (inputIndex == 0) {
									newItem.source = item.source;
								}
							}
						}
						ret.push(newItem);
					}
				}
			}
		}		
	}
	return ret;	
}

//
// Builds data converted to a grid as bootstrap rows
//
function buildGrid(grid) {
	var $ret = $('<div>');
	var parts = [ 'source', 'input', 'implant', 'output', 'target'];
	for (var y = 0; y < grid.length; y++) {
		var item = grid[y];
		var $row = $('<div>').addClass('row');		
		for (var x = 0; x < parts.length; x++) {
			var part = parts[x];
			var $cell = $('<div>').addClass('col-sm-2');
			if (item.hasOwnProperty(part)) {
				buildNode(item[part], part).appendTo($cell);
			}
			$cell.appendTo($row);			
		}
		$row.appendTo($ret);
	}
	return $ret;
}


function buildNode(node, nodeType) {
	var $ret = $('<div>');
	$ret.addClass('node');
	if (nodeType) {
		$ret.addClass(nodeType);	
	} 
	if (node) {
		$('<div>').addClass('type').html(node.head).appendTo($ret);
		$('<span>').html(node.text).appendTo($ret);
	}
	return $ret;
}

var html = buildGrid(toGrid(fakeData)).html();
$('#activity').append(html);
