
// TESTING cluster OSC messages
implant.osc.on('change', function(e) {
	implant.print('OSC change event: ' + e.address + ' ' + JSON.stringify(e));
});

implant.print('cluster test running');
