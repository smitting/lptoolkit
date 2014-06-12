
// Test blinking a button really fast.

var blinkX = -1;
var blinkY = -1;

this.launchPad.on('press', function(e) {

   blinkX = e.x;
   blinkY = e.y;  

});

this.launchPad.on('release', function(e) {

  blinkX = -1;
  blinkY = -1;
  
});

var counter = 0;

implant.launchPad.setInterval(function() {  
  if (blinkX > -1) {
   counter++;
   var color = (counter % 2) == 0 ? 'red' : 'yellow';
   implant.launchPad.setColor(blinkX, blinkY, color)
  }

},100);