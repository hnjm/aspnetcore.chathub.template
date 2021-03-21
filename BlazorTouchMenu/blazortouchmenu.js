window.addEventListener('DOMContentLoaded', () => {

    document.addEventListener('swiped-right', function (e) {

        document.getElementById('blazortouchmenuid').classList.remove('d-none');
        document.getElementById('blazortouchmenuid').classList.add('d-block');
    });

    document.addEventListener('swiped-left', function (e) {

        document.getElementById('blazortouchmenuid').classList.remove('d-block');
        document.getElementById('blazortouchmenuid').classList.add('d-none');
    });

});