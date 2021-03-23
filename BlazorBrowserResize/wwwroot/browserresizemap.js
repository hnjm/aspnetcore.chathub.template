export class browserresizemap {

    constructor(dotnetobjref) {

        this.dotnetobjref = dotnetobjref;
    }

    getInnerHeight () {

        return window.innerHeight;
    };
    getInnerWidth () {

        return window.innerWidth;
    };
    registerResizeCallback () {

        var __this = this;
        var resizeTimer;
        window.addEventListener('resize', function (e) {

            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {

                __this.resized();
            }, 200);
        });
    };
    resized () {

        this.dotnetobjref.invokeMethodAsync('OnBrowserResize');
    };
}