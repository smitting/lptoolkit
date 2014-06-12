
// Paint with color palette on right.

var currentColor = 'red';

var colors = ['red','redorange','orange','amber','yellow','yellowgreen','green','off'];


function drawPalette() {
	var x = implant.pads.width - 1;
	for (var y = 0; y < colors.length; y++) {
		implant.pads.set(x, y, colors[y]);
	}
}
drawPalette();

function chooseColor(y) {
	if (y >= 0 && y < colors.length) {
		currentColor = colors[y];
	}
}

function draw(x,y) {
	implant.pads.set(x,y,currentColor);
}

implant.pads.on('press', function(e) {
	if (e.x == implant.pads.width - 1) {
		chooseColor(e.y);
	}
	else {
		draw(e.x,e.y);
	}
});
