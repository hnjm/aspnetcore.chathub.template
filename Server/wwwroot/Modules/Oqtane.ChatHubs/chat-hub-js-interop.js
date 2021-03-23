window.addEventListener('DOMContentLoaded', () => {

    window.scroll = {

        scrollToBottom: function (elementId) {

            var messagewindow = document.querySelector(elementId);
            if (messagewindow !== null) {

                var lastchild = messagewindow.lastElementChild;
                var lastchildheight = lastchild.offsetHeight;
                var tolerance = 100;

                if (messagewindow.scrollTop + messagewindow.offsetHeight + lastchildheight + tolerance >= messagewindow.scrollHeight) {

                    messagewindow.scrollTo({
                        top: messagewindow.scrollHeight,
                        left: 0,
                        behavior: 'smooth'
                    });
                }
            }
        }
    };

    window.showchathubscontainer = function () {

        var chathubscontainer = document.querySelector('.chathubs-container');
        chathubscontainer.style.transition = "opacity 0.24s";
        chathubscontainer.style.opacity = "1";
    };

    window.cookies = {

        getCookie: function (cname) {
            var name = cname + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return "";
        },
        setCookie: function (cname, cvalue, expirationdays) {
            var d = new Date();
            d.setTime(d.getTime() + (expirationdays * 24 * 60 * 60 * 1000));
            var expires = "expires=" + d.toUTCString();
            document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
        },
    };

    window.__initjs = function (videojsdotnetobj, draggablejsdotnetobj, resizejsdotnetobj, fileuploadjsdotnetobj) {

        __obj = {

            videoservice: videojsdotnetobj,
            videolocalid: '#chathubs-video-local-',
            videoremoteid: '#chathubs-video-remote-',
            audiosourcelocalid: '#chathubs-audio-source-local-',
            videosourcelocalid: '#chathubs-video-source-local-',
            canvaslocalid: '#chathubs-canvas-local-',
            canvasremoteid: '#chathubs-canvas-remote-',
            videomimetypeobject: {

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
            },
            livestreams: [],
            locallivestream: function (roomId) {

                var __selflocallivestream = this;

                this.videolocalid = self.__obj.videolocalid + roomId;
                this.getvideolocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.videolocalid);
                };

                this.audiosourcelocalid = self.__obj.audiosourcelocalid + roomId;
                this.getaudiosourcelocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.audiosourcelocalid);
                };

                this.videosourcelocalid = self.__obj.videosourcelocalid + roomId;
                this.getvideosourcelocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.videosourcelocalid);
                };

                this.canvaslocalid = self.__obj.canvaslocalid + roomId;
                this.getcanvaslocaldomelement = function () {
                    return document.querySelector(__selflocallivestream.canvaslocalid);
                };

                this.audioselect = this.getaudiosourcelocaldomelement();
                this.videoselect = this.getvideosourcelocaldomelement();

                this.vElement = this.getvideolocaldomelement();
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
                this.gotDevices = function(mediadeviceinfos) {

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

                            __selfgetstream.options = { mimeType: self.__obj.videomimetypeobject.mimetype, audioBitsPerSecond: 420000, videoBitsPerSecond: 800000, ignoreMutedMedia: true };
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

                window.navigator.mediaDevices.enumerateDevices()
                    .then(function (mediadeviceinfos) {
                        __selflocallivestream.currentgotdevices = new __selflocallivestream.gotDevices(mediadeviceinfos);
                    })
                    .then(function () {
                        __selflocallivestream.currentgetstream = new __selflocallivestream.getStream();
                    })
                    .catch(function (ex) {
                        console.warn(ex.message);
                    });

                this.handleonchangeevent = async function () {

                    await __selflocallivestream.cancel();
                    __selflocallivestream.currentgetstream = new __selflocallivestream.getStream();
                };

                this.audioselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.audioselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

                this.videoselect.removeEventListener("change", __selflocallivestream.handleonchangeevent);
                this.videoselect.addEventListener("change", __selflocallivestream.handleonchangeevent);

                this.startsequence = function () {

                    try {

                        if (__selflocallivestream.currentgetstream !== null && __selflocallivestream.currentgetstream.recorder.state === 'inactive' || __selflocallivestream.currentgetstream.recorder.state === 'paused') {

                            __selflocallivestream.currentgetstream.recorder.start();
                        }
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };
                this.stopsequence = function () {

                    try {

                        if (__selflocallivestream.currentgetstream !== null && __selflocallivestream.currentgetstream.recorder.state === 'recording' || __selflocallivestream.currentgetstream.recorder.state === 'paused') {

                            __selflocallivestream.currentgetstream.recorder.stop();
                        }
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

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

                        self.__obj.videoservice.invokeMethodAsync('OnDataAvailable', dataURI, roomId, 'video').then(obj => {
                            console.log(obj.msg);
                        });
                    };
                    reader.readAsDataURL(sequence);
                };

                this.drawimage = function () {

                    try {

                        var videoElement = __selflocallivestream.getvideolocaldomelement();
                        var canvasElement = __selflocallivestream.getcanvaslocaldomelement();
                        var ctx = this.getcanvaslocaldomelement().getContext('2d');

                        ctx.drawImage(videoElement, 0, 0, 300, 150, 0, 0, 300, 150);
                        var dataURI = canvasElement.toDataURL('image/jpeg', 0.42);
                        __selflocallivestream.broadcastsnapshot(dataURI, 'image');
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };
                this.broadcastsnapshot = function (dataURI, dataType) {

                    self.__obj.videoservice.invokeMethodAsync('OnDataAvailable', dataURI, roomId, dataType).then(obj => {
                        console.log(obj.msg);
                    });
                };

                this.pauselivestreamtask = function () {

                    self.__obj.videoservice.invokeMethodAsync('PauseLivestreamTask', roomId);
                },
                this.continuelivestreamtask = function () {

                    self.__obj.videoservice.invokeMethodAsync('ContinueLivestreamTask', roomId);
                },
                this.cancel = function () {

                    var promise = new Promise(function (resolve) {

                        try {

                            if (__selflocallivestream.currentgetstream !== null) {

                                if (__selflocallivestream.currentgetstream.recorder !== undefined) {

                                    __selflocallivestream.currentgetstream.recorder.stream.getTracks().forEach(track => track.stop());
                                    __selflocallivestream.currentgetstream.recorder.stop();
                                }

                                __selflocallivestream.currentgetstream = null;
                                delete __selflocallivestream.currentgetstream;
                            }

                            __selflocallivestream.vElement.srcObject = null;
                            resolve();                            
                        }
                        catch (err) {
                            console.error(err);
                        }
                    });

                    return promise;
                };

            },
            remotelivestream: function (roomId) {

                var __selfremotelivestream = this;

                this.videoremoteid = self.__obj.videoremoteid + roomId;
                this.getvideoremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.videoremoteid);
                };

                this.canvasremoteid = self.__obj.canvasremoteid + roomId;
                this.getcanvasremotedomelement = function () {
                    return document.querySelector(__selfremotelivestream.canvasremoteid);
                };

                this.remotemediasequences = [];

                this.mediasource = new MediaSource();
                this.sourcebuffer = undefined;

                this.mediasource.addEventListener('sourceopen', function (event) {

                    if (!('MediaSource' in window) || !(window.MediaSource.isTypeSupported(self.__obj.videomimetypeobject.mimetype))) {

                        console.error('Unsupported MIME type or codec: ', self.__obj.videomimetypeobject.mimetype);
                    }

                    __selfremotelivestream.sourcebuffer = __selfremotelivestream.mediasource.addSourceBuffer(__obj.videomimetypeobject.mimetype);
                    __selfremotelivestream.sourcebuffer.mode = 'sequence';

                    __selfremotelivestream.sourcebuffer.addEventListener('updatestart', function (e) {});
                    __selfremotelivestream.sourcebuffer.addEventListener('update', function (e) {});
                    __selfremotelivestream.sourcebuffer.addEventListener('updateend', function (e) {

                        if (e.currentTarget.buffered.length === 1) {

                            var timestampOffset = __selfremotelivestream.sourcebuffer.timestampOffset;
                            var end = e.currentTarget.buffered.end(0);
                        }
                    });
                });
                this.mediasource.addEventListener('sourceended', function (event) { console.log("on media source ended"); });
                this.mediasource.addEventListener('sourceclose', function (event) { console.log("on media source close"); });

                this.video = this.getvideoremotedomelement();
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

                this.drawimage = function (base64str) {

                    try {
                        var canvas = __selfremotelivestream.getcanvasremotedomelement();
                        var ctx = canvas.getContext("2d");

                        var image = new Image();
                        image.onload = function () {
                            ctx.drawImage(image, 0, 0);
                        };
                        image.src = base64str;
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                };

                this.appendbuffer = async function (base64str) {

                    try {

                        console.log(base64str);
                        var blob = self.__obj.base64toblob(base64str);

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

                this.cancel = function () {

                };
            },
            getlivestream: function (roomId) {

                return self.__obj.livestreams.find(item => item.id === roomId);
            },
            addlivestream: function (obj) {

                var item = self.__obj.getlivestream(obj.id);
                if (item === undefined) {

                    self.__obj.livestreams.push(obj);
                }
            },
            removelivestream: function (roomId) {

                //self.__obj.livestreams = self.__obj.livestreams.filter(item => item.id !== roomId);
                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    self.__obj.livestreams.splice(self.__obj.livestreams.indexOf(livestream), 1);
                }
            },
            startbroadcasting: function (roomId) {

                var livestream = new self.__obj.locallivestream(roomId);
                var livestreamdicitem = {
                    id: roomId,
                    item: livestream,
                };

                self.__obj.addlivestream(livestreamdicitem);
            },
            startstreaming: function (roomId) {

                var livestream = new self.__obj.remotelivestream(roomId);
                var livestreamdicitem = {
                    id: roomId,
                    item: livestream,
                };

                self.__obj.addlivestream(livestreamdicitem);
            },
            startsequence: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined && livestream.item instanceof self.__obj.locallivestream) {

                    livestream.item.startsequence();
                }
            },
            stopsequence: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined && livestream.item instanceof self.__obj.locallivestream) {

                    livestream.item.stopsequence();
                }
            },
            drawimage: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.locallivestream) {

                        livestream.item.drawimage();
                    }
                }
            },
            appendbuffer: function (dataURI, roomId, dataType) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    if (livestream.item instanceof self.__obj.remotelivestream) {

                        if (dataType === 'video') {

                            livestream.item.appendbuffer(dataURI);
                        }
                        else if (dataType === 'image') {

                            livestream.item.drawimage(dataURI);
                        }
                    }
                    else if (livestream.item instanceof self.__obj.locallivestream) {

                        console.warn("this local livestream should actually not retrieve these data from the server hub");
                    }
                }
                else if (livestream === undefined) {

                    console.warn("these data are sent into the wild thats not oki");
                }
            },
            closelivestream: function (roomId) {

                var livestream = self.__obj.getlivestream(roomId);
                if (livestream !== undefined) {

                    livestream.item.cancel();
                    self.__obj.removelivestream(roomId);
                }
            },
            base64toblob: function (base64str) {

                var bytestring = atob(base64str.split('base64,')[1]);
                var arraybuffer = new ArrayBuffer(bytestring.length);

                var bytes = new Uint8Array(arraybuffer);
                for (var i = 0; i < bytestring.length; i++) {
                    bytes[i] = bytestring.charCodeAt(i);
                }

                var blob = new Blob([arraybuffer], { type: self.__obj.videomimetypeobject.mimetype });
                return blob;
            },
            draggableservice: draggablejsdotnetobj,
            initdraggable: function (elementId) {

                document.getElementById(elementId).addEventListener('dragstart', function (event) {

                    event.dataTransfer.effectAllowed = "move";

                    var id = event.target.id;
                    var arr = id.split('-');
                    var dragstartindex = arr[arr.length - 1];

                    var exceptDropzone = '.dropzone-' + dragstartindex;
                    var dropzones = document.querySelectorAll('.dropzone:not(' + exceptDropzone + ')');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "block";
                    });

                    event.dataTransfer.setData("index", dragstartindex);
                });
                document.getElementById(elementId).addEventListener('dragenter', function (event) {

                    event.target.classList.add('active-dropzone');
                });
                document.getElementById(elementId).addEventListener('dragleave', function (event) {

                    event.target.classList.remove('active-dropzone');
                });
                document.getElementById(elementId).addEventListener('dragover', function (event) {

                    event.preventDefault();
                    event.dataTransfer.dropEffect = 'move';
                });
                document.getElementById(elementId).addEventListener('drop', function (event) {

                    event.preventDefault();

                    var dragindex = event.dataTransfer.getData('index');

                    var id = event.target.id;
                    var arr = id.split('-');
                    var dropindex = arr[arr.length - 1];

                    self.__obj.draggableservice.invokeMethodAsync('OnDrop', parseInt(dragindex), parseInt(dropindex), elementId);
                });
                document.getElementById(elementId).addEventListener('dragend', function (event) {

                    var dropzones = document.getElementsByClassName('dropzone');
                    Array.prototype.forEach.call(dropzones, function (item) {

                        item.style.display = "none";
                        item.classList.remove('active-dropzone');
                    });
                });
            },
            resizeservice: resizejsdotnetobj,
            browserResize: {

                getInnerHeight: function () {

                    return window.innerHeight;
                },
                getInnerWidth: function () {

                    return window.innerWidth;
                },
                registerResizeCallback: function () {

                    var resizeTimer;
                    window.addEventListener('resize', function (e) {

                        clearTimeout(resizeTimer);
                        resizeTimer = setTimeout(() => {

                            self.__obj.browserResize.resized();
                        }, 200);
                    });
                },
                resized: function () {

                    self.__obj.resizeservice.invokeMethodAsync('OnBrowserResize');
                },
            },
            fileuploadservice: fileuploadjsdotnetobj,
            initfileuploaddropzone: function (inputFileId, elementId) {

                document.getElementById(elementId).addEventListener('dragover', function (event) {

                    event.stopPropagation();
                    event.preventDefault();
                    event.dataTransfer.dropEffect = 'copy';
                });
                document.getElementById(elementId).addEventListener('drop', function (event) {

                    event.stopPropagation();
                    event.preventDefault();

                    var files = event.dataTransfer.files;
                    var inputFileElement = document.getElementById(inputFileId);
                    inputFileElement.files = files;

                    var event = document.createEvent("UIEvents");
                    event.initUIEvent("change", true, true);
                    inputFileElement.dispatchEvent(event);

                    self.__obj.fileuploadservice.invokeMethodAsync('OnDrop', elementId);
                });
            },

        };
    };

    window.__init = function (videojsdotnetobj, draggablejsdotnetobj, resizejsdotnetobj, fileuploadjsdotnetobj) {

        return storeObjectRef(
            new window.__initjs(
                videojsdotnetobj,
                draggablejsdotnetobj,
                resizejsdotnetobj,
                fileuploadjsdotnetobj,
            )
        );
    };

    var jsObjectRefs = {};
    var jsObjectRefId = 0;
    const jsRefKey = '__jsObjectRefId';
    function storeObjectRef(obj) {
        var id = jsObjectRefId++;
        jsObjectRefs[id] = obj;
        var jsRef = {};
        jsRef[jsRefKey] = id;
        return jsRef;
    };

});
