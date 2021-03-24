export function initfileupload(dotnetobjref, inputFileId, elementId) {

    var __obj = {

        fileuploadmap: function (dotnetobjref, inputFileId, elementId) {

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

                dotnetobjref.invokeMethodAsync('OnDrop', elementId);
            });
        },
    };

    return new __obj.fileuploadmap(dotnetobjref, inputFileId, elementId);
}
