/*
This is testing interactions between the GUI, KNOBS, and PADS.

Also scrolling of pads.

*/

var values = [];
for (var i = 0; i < implant.pads.width; i++) {
	values.push(0);
}

implant.gui.on('paint', function() {
	implant.gui.beginVertical();
	implant.gui.spacer(50);
		implant.gui.beginHorizontal();
		for (var i = 0; i < values.length - 1; i++) {
			implant.gui.spacer(10);
			var oldvalue = values[i];
			values[i] = implant.gui.knob(values[i], 0.0, 1.0);
			if (values[i] != oldvalue) {
				paintColumn(i);
			}
		}
		
		var faderIndex = values.length - 1;
		var old = values[faderIndex];
		values[faderIndex] = implant.gui.verticalFader(values[faderIndex],0.0,1.0);
		if (values[faderIndex] != old) {
			paintColumn(faderIndex);
		}
		implant.gui.endHorizontal();
	implant.gui.endVertical();

});

function paintColumn(i) {
	implant.print('painting ' + i);
	if (i >= values.length) return;
	
	var maxY = implant.pads.height;
	
	var y = values[i] * maxY;
	for (var j = 0; j <= maxY; j++) {
		var c = j <= y ? 'yellow' : 'off';
		implant.pads.set(i, maxY-j, c);
	}	
}

function repaint() {
	for (var i = 0; i < implant.pads.width; i++) {
		paintColumn(i);
	}
}

implant.mode.on('modechanged', function(e) {	
	if (e.value == implant.assignedMode) {
		implant.print('repainting because of mode change');
		repaint();		
	}
});

implant.knobs.on('change', function(e) {	
	// the right knobs scroll
//	implant.print('knob change x=' + e.x + ' y=' + e.y + ' value=' + e.value);
	
	
	if (e.x == 7) {
		implant.pads.scrollTo(e.value / 8,0);
		return;
	}
	
	
	// otherwise control a value
	if (e.x < values.length) {
		var old = values[e.x];
		values[e.x] =  quantize(e.value / 127);
		if (old != values[e.x]) {
			paintColumn(e.x);
			implant.gui.repaint();	
		}	
	}
});

function quantize(v) {
	var maxY = implant.pads.height;	
	return Math.floor(v * maxY) / maxY;
}

implant.pads.on('press', function(e) {
	implant.print('pad press x=' + e.x + ' y=' + e.y);
	
	if (e.x < values.length) {
		var maxY = implant.pads.height;
		values[e.x] =  (maxY - e.y) / maxY;
		paintColumn(e.x);
		implant.gui.repaint();		
	}
});

