
    var knobs = [
        { $jq: $('#knob1'), offset: 0 },
        { $jq: $('#knob2'), offset: 1.5 },
        { $jq: $('#knob3'), offset: 4 },
        { $jq: $('#knob4'), offset: 5 },
        { $jq: $('#knob5'), offset: 0 },
        { $jq: $('#knob6'), offset: 1.5 },
        { $jq: $('#knob7'), offset: 4 },
        { $jq: $('#knob8'), offset: 5 }
    ];

    var $knob1 = $('#knob1');

    var speed = 1000;
    var minY = 5;
    var maxY = 170;

    var oldValue = [];

    setInterval(function () {

        for (var i = 0; i < knobs.length; i++) {
            if (knobs[i].value != null) {
                var y = minY + (maxY - minY) * knobs[i].value;
                if (oldValue[i] != y) {
                    knobs[i].$jq.animate({ top: y }, 100);
                    oldValue[i] = y;
                }
            }
            else {
                var now = new Date().getTime() / speed;
                var theta = Math.sin(now + knobs[i].offset);
                var p = theta * 0.5 + 0.5;
                var y = minY + (maxY - minY) * p;
                knobs[i].$jq.css({ top: y });
            }
        }
        


    }, 30);


// TODO: this should be part of a library of controls for 
// displaying different data types on the screen
$(document).ready(function() {
    var $lp = $('.launchpad');
    for (var y = 0; y < 8; y++) {
        var $row = $('<div class="row">');
        $row.attr('id','row_' + y);
        for (var x = 0; x < 8; x++) {
            var $cell = $('<div class="col">');
            $cell.attr('id','col_' + x);
            $row.append($cell);
        }
        $lp.append($row);
    }
});


function onAjaxData(json) {

    // set the fader values if we can
    for (var key in json) {
        if (key.indexOf('/fader/') == 0) {
            var num = Number(key.substring(7));
            var value = json[key];
            if (num >= 0 && num < knobs.length) {
                knobs[num].value = 1-value;
            }
        }
        else if (key.indexOf('/launchpad/') == 0) {
            var parts = key.substring(1).split('/');
            if (parts.length >= 3) {
                var value = json[key];
                var x = parts[1];
                var y = parts[2];


                var $row = $('#row_' + y);
                var $cell = $row.find('#col_' + x);


                if (value == 1) {
                    $cell.addClass('green');
                }
                else {
                    $cell.removeClass('green');
                }
                if (value == 2) {
                    $cell.addClass('red');
                }
                else {
                    $cell.removeClass('red');
                }
            }

        }
        else {
            console.log('unknown: ' + key);
        }
    }
}


var lpajax = new LaunchPadAjax(onAjaxData, true);

// show data from ajax class
var $txtRequestWait = $('#txtRequestWait');
var $txtLastData = $('#txtLastData');
var $txtTimestamp = $('#txtTimestamp');
setInterval(function () {
    $txtTimestamp.html(lpajax.serverTimestamp);
    $txtRequestWait.html(String((lpajax.requestDurationMsec()) / 1000));
    $txtLastData.html(String((lpajax.timeSinceDataMsec()) / 1000));
}, 1);

