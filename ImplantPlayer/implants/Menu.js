

function debug(s) {
//	implant.print(s);
}


//
//	Changes the mode when the button is clicked.  The mode is shown
//	as green and the others as red.
//
function paintMode(mode, onColor, offColor) {
	if (onColor == null) onColor = 'green';
	if (offColor == null) offColor = 'red';
	for (var x = 0; x < implant.pads.width; x++) {
		var color = mode == x ? onColor : offColor;
		for (var y = 0; y < implant.pads.height; y++) {
			implant.pads.set(x, y, color);
		}
	}	
	debug('menu height is ' + implant.pads.height);
}


function repaint() {
	paintMode(implant.mode.current);
}

repaint();
implant.mode.on('modechanged', function(e) {
	debug('Detected mode change -- from event: ' + e.value + ' from property: ' + implant.mode.current);
	repaint();
});


implant.pads.on('press', function(e) {
	debug('pressed ' + e.x + ',' + e.y);
	paintMode(e.x, 'orange','red');
});


implant.pads.on('release', function(e) {
	implant.mode.current = e.x;
});
