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

});
