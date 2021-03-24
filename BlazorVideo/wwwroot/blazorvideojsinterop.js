export function initblazorvideo(dotnetobjref, id, type) {

    var __obj = {

        blazorvideomap: function (dotnetobjref, id, type) {
            
            var __selfblazorvideomap = this;

            this.locallivestreamelementidprefix = '#local-livestream-element-id-';
            this.audiosourcelocalid = '#local-livestream-audio-source-';
            this.videosourcelocalid = '#local-livestream-video-source-';

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

                var __selflocallivestream = this;

                this.videoelementid = __selfblazorvideomap.locallivestreamelementidprefix + id;
                this.getvideoelement = function () {
                    return document.querySelector(__selflocallivestream.videoelementid);
                };

                this.audiosourcelocalid = __selfblazorvideomap.audiosourcelocalid + id;
                this.getaudiosourcelocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.audiosourcelocalid);
                };
                this.videosourcelocalid = __selfblazorvideomap.videosourcelocalid + id;
                this.getvideosourcelocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.videosourcelocalid);
                };

                this.audioselect = this.getaudiosourcelocaldomelement();
                this.videoselect = this.getvideosourcelocaldomelement();

                this.vElement = this.getvideoelement();
                this.vElement.onloadedmetadata = function (e) {

                    __selflocallivestream.vElement.play();
                };
                this.vElement.autoplay = true;
                this.vElement.controls = true;
                this.vElement.muted = true;

                this.currentgotdevices = null;
                this.gotDevices = function (mediadeviceinfos) {

                    var audioselectchild = __selflocallivestream.audioselect.firstElementChild;
                    while (audioselectchild) {
                        __selflocallivestream.audioselect.removeChild(audioselectchild);
                        audioselectchild = __selflocallivestream.audioselect.firstElementChild;
                    }

                    var videoselectchild = __selflocallivestream.videoselect.firstElementChild;
                    while (videoselectchild) {
                        __selflocallivestream.videoselect.removeChild(videoselectchild);
                        videoselectchild = __selflocallivestream.videoselect.firstElementChild;
                    }

                    for (var i = 0; i < mediadeviceinfos.length; i++) {

                        var temp = i;
                        const deviceInfo = mediadeviceinfos[temp];
                        const option = document.createElement("option");
                        option.value = deviceInfo.deviceId;
                        if (deviceInfo.kind === "audioinput") {
                            option.text = deviceInfo.label || "microphone " + (__selflocallivestream.audioselect.length + 1);
                            __selflocallivestream.audioselect.appendChild(option);
                        } else if (deviceInfo.kind === "videoinput") {
                            option.text = deviceInfo.label || "camera " + (__selflocallivestream.videoselect.length + 1);
                            __selflocallivestream.videoselect.appendChild(option);
                        } else {
                            console.log("Found another kind of device: ", deviceInfo);
                        }
                    }
                };

                this.currentgetstream = null;
                this.getStream = function () {

                    var __selfgetstream = this;

                    this.constrains = {
                        audio: {
                            volume: { ideal: 0.5 },
                        },
                        video: {
                            width: { min: 320, ideal: 320, max: 320 },
                            height: { min: 240, ideal: 240, max: 240 },
                            frameRate: { ideal: 24 },
                            facingMode: { ideal: "user" },
                        }
                    };

                    this.constrains.audio['deviceId'] = { ideal: __selflocallivestream.audioselect.value };
                    this.constrains.video['deviceId'] = { ideal: __selflocallivestream.videoselect.value };

                    window.navigator.mediaDevices
                        .getUserMedia(this.constrains)
                        .then(function (mediastream) {

                            __selflocallivestream.vElement.srcObject = mediastream;

                            __selfgetstream.options = { mimeType: __selfblazorvideomap.videomimetypeobject.mimetype, audioBitsPerSecond: 420000, videoBitsPerSecond: 800000, ignoreMutedMedia: true };
                            __selfgetstream.recorder = new MediaRecorder(mediastream, __selfgetstream.options);

                            __selfgetstream.recorder.start();
                            __selfgetstream.recorder.ondataavailable = (event) => {

                                if (event.data.size > 0) {

                                    console.log(event.data);
                                    __selflocallivestream.broadcastvideodata(event.data);
                                }
                            };

                        })
                        .catch(function (ex) {
                            console.warn(ex);
                        });
                };

                this.initdevices = function () {

                    var promise = new Promise(function (resolve) {

                        window.navigator.mediaDevices.enumerateDevices()
                            .then(function (mediadeviceinfos) {

                                __selflocallivestream.currentgotdevices = new __selflocallivestream.gotDevices(mediadeviceinfos);
                            })
                            .catch(function (ex) {

                                console.warn(ex.message);
                            })
                            .finally(function () {

                                resolve();
                            });
                    });
                };
                this.initstream = function () {

                    __selflocallivestream.currentgetstream = new __selflocallivestream.getStream();
                };

                this.handleonchangeevent = async function () {

                    await __selflocallivestream.cancel();
                    __selflocallivestream.currentgetstream = new __selflocallivestream.getStream();
                };

                this.audioselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.audioselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

                this.videoselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.videoselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

            };
            this.initlocallivestream = async function () {

                try {
                    __selfblazorvideomap.contextlocallivestream = new __selfblazorvideomap.locallivestream();
                    await __selfblazorvideomap.contextlocallivestream.initdevices();
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.startbroadcasting = function () {

                try {

                    if (__selfblazorvideomap.contextlocallivestream != null) {

                        __selfblazorvideomap.contextlocallivestream.initstream();
                    }
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
