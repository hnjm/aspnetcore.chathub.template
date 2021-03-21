export function initblazorvideo(dotnetobjref) {

    __obj = {

        blazorvideomap: function () {
            
            var __selfblazorvideomap = this;
            this.videoelementidprefix = '#video-element-id-';
            this.contextvideo = null;
            this.video = function (id) {

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
                    }
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
            this.newvideo = function (id) {

                try {

                    __selfblazorvideomap.contextvideo = new __selfblazorvideomap.video(id);
                    __selfblazorvideomap.contextvideo.initusermedia();
                }
                catch (ex) {

                    console.warn(ex);
                }
            };
        }
    }

    return new self.__obj.blazorvideomap();
};
