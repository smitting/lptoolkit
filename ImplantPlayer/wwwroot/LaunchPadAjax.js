
/**
  * This object maintains continual push requests to the LPToolKit
  * server.  There will always be one open request that the server
  * pauses until new data is available.
  *
  * TODO: use a callback to send data back to the main javascript.
  */
var LaunchPadAjax = (function ($) {

    var SERVER_URL = '/';

    /**
      * Constructor initializes the timestamps and measurements for
      * push mode.  Optionally starts the processing immediately if
      * true is passed to the optional autoStart parameter.
      *
      * @param callback the function to call with new data
      * @param autoStart optional argument to automatically start the thread.
      */
    function LPAjax(callback, autoStart) {
        // init
        this.running = false;
        this.serverTimestamp = -1;
        this.lastRequestStart = null;
        this.lastRequestDone = null;
        this.dataCallback = callback;

        // auto-start as specified
        if (autoStart) {
            this.start();
        }
    }

    /**
      * Starts requesting data from the server via push requests.
      */
    LPAjax.prototype.start = function () {
        this.running = true;
        this.startPushRequest();
    };

    /**
      * Stops requesting push data from the server.
      */
    LPAjax.prototype.stop = function () {
        this.running = false;
        // TODO: some how end the current request early.
    };

    /**
      * Returns how long the current request has been running, or
      * -1 if no request is running.  Result is in milliseconds.
      */
    LPAjax.prototype.requestDurationMsec = function () {
        if (this.lastRequestStart != null) {
            var now = new Date().getTime();
            return now - this.lastRequestStart;
        }
        return -1;
    };

    /**
      * Returns how long since we've received new data in msec, or
      * -1 if we have never received data.
      */
    LPAjax.prototype.timeSinceDataMsec = function () {
        if (this.lastRequestDone != null) {
            var now = new Date().getTime();
            return now - this.lastRequestDone;
        }
        return -1;
    };

    /**
      * Issues an ajax request to the LPToolKit server that does not
      * send a response until data has changed on the server.  This
      * simulates having the server send a request to the client,
      * i.e. a server push.
      *
      * Automatically starts the next push request as data is received
      * until Stop() is called.
      */
    LPAjax.prototype.startPushRequest = function () {
        var me = this;

        me.lastRequestStart = new Date().getTime();
        $.ajax(SERVER_URL, { data: { ajax: true, ts: me.serverTimestamp }, dataType: 'text' })
            .done(function (res) {
                me.lastRequestStart = null;
                me.lastRequestDone = new Date().getTime();

                try {
                    var packet = JSON.parse(res);
                    if (!packet) return;

                    me.serverTimestamp = packet.timestamp;
                    if (!packet.data) return;

                    if (me.dataCallback != null) {
                        me.dataCallback(packet.data);
                    }
                }
                catch (ex) {

                }
                if (me.running) {
                    me.startPushRequest();
                }
            })
            .error(function () {
                // TODO: better error handling
                console.log('error');
                if (me.running) {
                    me.startPushRequest();
                }
            });
    };

    return LPAjax;
})(jQuery);
