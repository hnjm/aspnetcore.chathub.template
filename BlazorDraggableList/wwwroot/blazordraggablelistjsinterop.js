export function initblazordraggablelist(dotnetobjref, elementId) {

    var __obj = {

        draggablelistmap: function (dotnetobjref, elementId) {

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

                dotnetobjref.invokeMethodAsync('OnDrop', parseInt(dragindex), parseInt(dropindex), elementId);
            });
            document.getElementById(elementId).addEventListener('dragend', function (event) {

                var dropzones = document.getElementsByClassName('dropzone');
                Array.prototype.forEach.call(dropzones, function (item) {

                    item.style.display = "none";
                    item.classList.remove('active-dropzone');
                });
            });
        },
    };

    return new __obj.draggablelistmap(dotnetobjref, elementId);
}