export function initblazorvideo(dotnetobjref, id, type) {

    var __obj = {

        blazorvideomap: function (dotnetobjref, id, type) {
            
            var __selfblazorvideomap = this;
            this.videoelementidprefix = '#video-element-id-';
            this.videomimetypeobject = {

                get mimetype() {

                    var userAgent = navigator.userAgent.toLowerCase();

                    if (userAgent.indexOf('chrome') > -1 ||
                        userAgent.indexOf('firefox') > -1 ||
                        userAgent.indexOf('opera') > -1 ||
                        userAgent.indexOf('safari') > -1 ||
                        userAgent.indexOf('msie') > -1 ||
                        userAgent.indexOf('edge') > -1) {

                        return 'video/webm;codecs=opus,vp8';
                    }
                    else {

                        console.warn('using unknown browser'); return 'video/webm;codecs=opus,vp8';
                    }
                }
            };
            this.contextlocallivestream = null;
            this.locallivestream = function () {

                var __selfvideo = this;

                this.constrains = {
                    audio: {
                        volume: { ideal: 0.5 },
                    },
                    video: {
                        width: { min: 320, ideal: 320, max: 320 },
                        height: { min: 240, ideal: 240, max: 240 },
                        frameRate: { ideal: 24 },
                        facingMode: { ideal: "user" },
                    },
                };

                this.videoelementid = __selfblazorvideomap.videoelementidprefix + id;
                this.getvideoelement = function () {
                    return document.querySelector(__selfvideo.videoelementid);
                };

                this.vElement = this.getvideoelement();
                this.vElement.onloadedmetadata = function (e) {

                    __selfvideo.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;

                this.initusermedia = async function () {

                    window.navigator.mediaDevices.getUserMedia(__selfvideo.constrains)
                        .then(function (stream) {

                            __selfvideo.vElement.srcObject = stream;
                            __selfvideo.vElement.onloadedmetadata = function (e) {
                                __selfvideo.vElement.play();
                            };
                        })
                        .catch(function (err) {

                            console.error(err);
                        });
                };
            };
            this.newlocallivestream = function () {

                try {

                    __selfblazorvideomap.contextlocallivestream = new __selfblazorvideomap.locallivestream();
                    __selfblazorvideomap.contextlocallivestream.initusermedia();
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.base64toblob = function (base64str) {

                var bytestring = atob(base64str.split('base64,')[1]);
                var arraybuffer = new ArrayBuffer(bytestring.length);

                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }

                var blob = new Blob([arraybuffer], { type: __selfblazorvideomap.videomimetypeobject.mimetype });
                return blob;
            };
        }
    }

    return new __obj.blazorvideomap(dotnetobjref, id, type);
};
