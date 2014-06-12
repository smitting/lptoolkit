//
// File:    DrumSequencer.js
// Author:  Scott Mitting
// Date:    2014-04-24
// Abstract:
//  Simple drum sequencer that stores which beats to trigger a drum
//  upon.  Each row represents a different track.
//
// TODO:
//  Make double clicks provide "emphasis" in a different color
//  Allow tracks to be longer than 8 beats via mode buttons
//  Allow separate pattern selection for current playback and current edit via mode button
//  Optionally display current beat and pattern playback via overlays
//

/**
  * Initializes the variable this.grid to store the current value
  * of all the grid.
  *
  * TODO: this information should be available from the launchpad IO
  * api itself to see the current color.
  *
  */

function initGrid() {
    implant.grid = [];
    for (var i = 0; i < 8; i++) {
        implant.grid[i] = [];
        for (var j = 0; j < 8; j++) {
            implant.grid[i][j] = false;
        }
    }
    // also set colors
}

// sets all colors to match grid
function reset() {
    for (var i = 0; i < implant.pads.height; i++) {
        for (var j = 0; j < implant.pads.width; j++) {
            var color = implant.grid[i, j] == true ? 'yellow' : 'off';
            implant.pads.set(i, j, color);
        }
    }
}





initGrid();
reset();


this.pads.on('press', function (e) {
    if (implant.grid[e.x][e.y]) {
        implant.grid[e.x][e.y] = false;
        implant.pads.set(e.x, e.y, 'off');


        implant.osc.set(e.x, e.y, 0);
    }
    else {
        implant.grid[e.x][e.y] = true;
        implant.pads.set(e.x, e.y, 'yellow');

        implant.osc.set(e.x, e.y, 1);
    }
});



