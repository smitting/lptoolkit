//
// Builds the top navigation on all configuration screens.
//

function SettingsNavBar() {
	
	this.title = 'LPToolKit Settings';
	this.returnUrl = 'index.html';
	this.items = [
		{ url: 'midimap.html', title: 'OSC to MIDI' },
		{ url: 'devices.html', title: 'Device Manager' },
		{ url: 'implants.html', title: 'Implants' },
		{ url: 'logs.html', title: 'Logs' },
		{ url: 'lagtime.html', title: 'Lag' }
	];
	
	


	this.build();
}

SettingsNavBar.prototype.build = function() {
	
	// remove the menu if it already exists
	$('#panelTopNav').remove();
	
	// build the general structure
	var $navbar = $('<div class="navbar navbar-inverse navbar-fixed-top" role="navigation" id="panelTopNav">');
	var $container = $('<div class="container-fluid">').appendTo($navbar);
	var $header = $('<div class="navbar-header">').appendTo($container)
	$header.append('\
	  <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">\
        <span class="sr-only">Toggle navigation</span>\
        <span class="icon-bar"></span>\
        <span class="icon-bar"></span>\
        <span class="icon-bar"></span>\
      </button>');
	$header.append('<a class="navbar-brand" href="' + this.returnUrl + '">' + this.title + '</a>');
	
	var $menuCollapse = $('<div class="navbar-collapse collapse">').appendTo($container);
	var $menuItems = $('<ul class="nav navbar-nav navbar-right">').appendTo($menuCollapse);
	
	// insert the menu items
	for (var i = 0; i < this.items.length; i++) {
		var item = this.items[i];
		var $item = $('<li>');
		var $a = $('<a>');
		$a.html(item.title);
		$a.attr('href', item.url);
		$item.append($a);
		$menuItems.append($item);
	}
		
	// highlight the current screen
	
	
	// add to the top of the body tag	
	$('body').prepend('<br/><br/><br/>');
	$('body').prepend($navbar);	
};

new SettingsNavBar();