export function initblazorvideo(dotnetobjref, id, type) {

    var __obj = {

        blazorvideomap: function (dotnetobjref, id, type) {
            
            var __selfblazorvideomap = this;

            this.locallivestreamelementidprefix = '#local-livestream-element-id-';
            this.remotelivestreamelementidprefix = '#remote-livestream-element-id-';

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

            this.livestreams = [];
            this.getlivestream = function (roomId) {

                return __selfblazorvideomap.livestreams.find(item => item.id === roomId);
            };
            this.addlivestream = function (obj) {

                var item = __selfblazorvideomap.getlivestream(obj.id);
                if (item === undefined) {

                    __selfblazorvideomap.livestreams.push(obj);
                }
            };
            this.removelivestream = function (roomId) {

                //__selfblazorvideomap.livestreams = __selfblazorvideomap.livestreams.filter(item => item.id !== roomId);
                var livestream = __selfblazorvideomap.getlivestream(roomId);
                if (livestream !== undefined) {

                    __selfblazorvideomap.livestreams.splice(__selfblazorvideomap.livestreams.indexOf(livestream), 1);
                }
            };

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

                this.vElement.addEventListener("pause", function () {

                    __selflocallivestream.pauselivestreamtask();
                });

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

                    this.recorder = null;
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

                                if (__selflocallivestream.currentgotdevices !== null) {

                                    delete __selflocallivestream.currentgotdevices;
                                    __selflocallivestream.currentgotdevices = null;
                                }

                                __selflocallivestream.currentgotdevices = new __selflocallivestream.gotDevices(mediadeviceinfos);
                            })
                            .catch(function (ex) {

                                console.warn(ex.message);
                            });

                        resolve();
                    });

                    return promise;
                };
                this.initstream = function () {

                    if (__selflocallivestream.currentgetstream !== null) {

                        __selflocallivestream.currentgetstream = null;
                        delete __selflocallivestream.currentgetstream;
                    }

                    __selflocallivestream.currentgetstream = new __selflocallivestream.getStream();
                };

                this.startsequence = function () {

                    try {

                        if (__selflocallivestream.currentgetstream?.recorder?.state === 'inactive' || __selflocallivestream.currentgetstream?.recorder?.state === 'paused') {

                            __selflocallivestream.currentgetstream.recorder.start();
                        }
                    }
                    catch (ex) {

                        console.warn(ex);
                    }
                };
                this.stopsequence = function () {

                    try {

                        if (__selflocallivestream.currentgetstream?.recorder?.state === 'recording' || __selflocallivestream.currentgetstream?.recorder?.state === 'paused') {

                            __selflocallivestream.currentgetstream.recorder.stop();
                        }
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

                this.handleonchangeevent = async function () {

                    await __selflocallivestream.cancel();
                    __selflocallivestream.initstream();
                };

                this.audioselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.audioselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

                this.videoselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.videoselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

                this.broadcastvideodata = function (sequence) {

                    var reader = new FileReader();
                    reader.onloadend = async function (event) {

                        var dataURI = event.target.result;

                        var totalBytes = Math.ceil(event.total * 8 / 6);
                        var totalKiloBytes = Math.ceil(totalBytes / 1024);
                        if (totalKiloBytes >= 500) {

                            console.warn('data uri too large to broadcast >= 500kb');
                            return;
                        }

                        dotnetobjref.invokeMethodAsync('OnDataAvailable', dataURI, id);
                    };
                    reader.readAsDataURL(sequence);
                };
                this.pauselivestreamtask = function () {

                    dotnetobjref.invokeMethodAsync('PauseLivestreamTask', id);
                };
                this.continuelivestreamtask = function () {

                    dotnetobjref.invokeMethodAsync('ContinueLivestreamTask', id);
                };

                this.cancel = function () {

                    var promise = new Promise(function (resolve) {

                        try {

                            __selflocallivestream.currentgetstream.recorder?.stream.getTracks().forEach(track => track.stop());
                            __selflocallivestream.currentgetstream.recorder?.stop();

                            delete __selflocallivestream.currentgetstream;
                            __selflocallivestream.currentgetstream = null;
                            
                            delete __selflocallivestream.vElement.srcObject;
                            __selflocallivestream.vElement.srcObject = null;
                            
                            resolve();
                        }
                        catch (err) {
                            console.warn(err);
                        }
                    });

                    return promise;
                };

            };
            this.initlocallivestream = function () {

                try {

                    __selfblazorvideomap.getlivestream(id);
                    if (livestream === undefined) {

                        var livestream = new __selfblazorvideomap.locallivestream();
                        var livestreamdicitem = {

                            id: id,
                            item: livestream,
                        };

                        __selfblazorvideomap.addlivestream(livestreamdicitem);
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.initdeviceslocallivestream = async function () {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(id);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.locallivestream) {

                        await livestream.item.initdevices();
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.startbroadcastinglocallivestream = async function () {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(id);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.locallivestream) {

                        await livestream.item.initstream();
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.startsequencelocallivestream = function () {

                var livestream = __selfblazorvideomap.getlivestream(id);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.locallivestream) {

                    livestream.item.startsequence();
                }
            };
            this.stopsequencelocallivestream = function () {

                var livestream = __selfblazorvideomap.getlivestream(id);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.locallivestream) {

                    livestream.item.stopsequence();
                }
            };
            this.closelocallivestream = async function () {

                var livestream = __selfblazorvideomap.getlivestream(id);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.locallivestream) {

                    livestream.item.audioselect.removeEventListener("change", livestream.item.handleonchangeevent);
                    livestream.item.videoselect.removeEventListener("change", livestream.item.handleonchangeevent);

                    await livestream.item.cancel();
                    __selfblazorvideomap.removelivestream(id);
                }
            };

            this.remotelivestream = function () {

                var __selfremotelivestream = this;

                this.videoelementid = __selfblazorvideomap.remotelivestreamelementidprefix + id;
                this.getvideoelement = function () {
                    return document.querySelector(__selfremotelivestream.videoelementid);
                };

                this.remotemediasequences = [];
                this.mediasource = new MediaSource();
                this.sourcebuffer = undefined;

                this.mediasource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(__selfblazorvideomap.videomimetypeobject.mimetype))) {

                        console.error('Unsupported MIME type or codec: ', __selfblazorvideomap.videomimetypeobject.mimetype);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediasource.addSourceBuffer(__selfblazorvideomap.videomimetypeobject.mimetype);
                    __selfremotelivestream.sourcebuffer.mode = 'sequence';

                    __selfremotelivestream.sourcebuffer.addEventListener('updatestart', function (e) { });
                    __selfremotelivestream.sourcebuffer.addEventListener('update', function (e) { });
                    __selfremotelivestream.sourcebuffer.addEventListener('updateend', function (e) {

                        if (e.currentTarget.buffered.length === 1) {

                            var timestampOffset = __selfremotelivestream.sourcebuffer.timestampOffset;
                            var end = e.currentTarget.buffered.end(0);
                        }
                    });
                });
                this.mediasource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediasource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoelement();
                this.video.controls = true;
                this.video.autoplay = true;
                this.video.preload = 'auto';
                this.video.muted = true;

                try {
                    this.video.srcObject = this.mediasource;
                } catch (ex) {
                    console.warn(ex);
                    this.video.src = URL.createObjectURL(this.mediasource);
                }

                this.appendbuffer = function (base64str) {

                    try {

                        console.log(base64str);
                        var blob = __selfblazorvideomap.base64toblob(base64str);

                        var reader = new FileReader();
                        reader.onloadend = function (event) {

                            var timeDiff = __selfremotelivestream.sourcebuffer.timestampOffset - __selfremotelivestream.video.currentTime;
                            if (timeDiff > 1) {
                                __selfremotelivestream.video.currentTime = __selfremotelivestream.sourcebuffer.timestampOffset - 0.42;

                                if (__selfremotelivestream.video.paused) {

                                    __selfremotelivestream.video.play();
                                }
                            }
                            console.log('time diff: ' + timeDiff);

                            __selfremotelivestream.remotemediasequences.push(reader.result);

                            if (__selfremotelivestream.remotemediasequences.length >= 2) {

                                __selfremotelivestream.remotemediasequences.shift();
                            }

                            if (!__selfremotelivestream.sourcebuffer.updating && __selfremotelivestream.mediasource.readyState === 'open') {

                                var item = __selfremotelivestream.remotemediasequences.shift();
                                __selfremotelivestream.sourcebuffer.appendBuffer(new Uint8Array(item));
                            }
                        }
                        reader.readAsArrayBuffer(blob);
                    }
                    catch (ex) {
                        console.error(ex);
                    }
                };
            };
            this.initremotelivestream = function () {

                try {

                    __selfblazorvideomap.getlivestream(id);
                    if (livestream === undefined) {

                        var livestream = new __selfblazorvideomap.remotelivestream();
                        var livestreamdicitem = {

                            id: id,
                            item: livestream,
                        };

                        __selfblazorvideomap.addlivestream(livestreamdicitem);
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.appendbufferremotelivestream = function (base64str) {

                try {

                    var livestream = __selfblazorvideomap.getlivestream(id);
                    if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                        livestream.item.appendbuffer(base64str);
                    }
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
            this.closeremotelivestream = async function () {

                var livestream = __selfblazorvideomap.getlivestream(id);
                if (livestream !== undefined && livestream.item instanceof __selfblazorvideomap.remotelivestream) {

                    __selfblazorvideomap.removelivestream(id);
                }
            };
        }
    }

    return new __obj.blazorvideomap(dotnetobjref, id, type);
};
