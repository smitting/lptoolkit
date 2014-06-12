
// TODO: just repeatedly request for the bitmap

var GUI_AJAX_URL = '/settings/gui';

$(document).ready(function() {
	getNextBitmap();
	
	// pass mouse data over
	$('#gui').on('mousedown', function(e) {
//		console.log('down(' + e.offsetX + ',' + e.offsetY + ')');
		 $.ajax({ url: GUI_AJAX_URL, data: { cmd: 'down', x: e.offsetX, y: e.offsetY } })
		e.preventDefault();
	});
	$('#gui').on('mousemove', function(e) {
//		console.log('move(' + e.offsetX + ',' + e.offsetY + ')');
		$.ajax({ url: GUI_AJAX_URL, data: { cmd: 'move', x: e.offsetX, y: e.offsetY } })
		e.preventDefault();
	});
	$('#gui').on('mouseup', function(e) {
//		console.log('up(' + e.offsetX + ',' + e.offsetY + ')');
		$.ajax({ url: GUI_AJAX_URL, data: { cmd: 'up', x: e.offsetX, y: e.offsetY } })
		e.preventDefault();
	});
	
	
});

// repeatedly asks for new bitmaps from the server
var iteration = 0;
function getNextBitmap() {
	iteration++;
	
	
	 $.ajax({ url: GUI_AJAX_URL })
			.done(function (res) { 
				if (res.length > 0) {
					$('#gui').attr('src', 'data:image/png;base64,' + res);
				}
				setTimeout(getNextBitmap, 1);
				})
			.fail(function (xhr, textStatus, errorThrown) { console.error(xhr.responseText); });
	
}