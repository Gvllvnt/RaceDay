// Live preview for image file inputs: <input type="file" data-preview="#imgId">
document.addEventListener('change', function (e) {
    var input = e.target;
    if (!input || input.type !== 'file' || !input.dataset.preview) return;

    var target = document.querySelector(input.dataset.preview);
    var file = input.files && input.files[0];
    if (!target || !file || !file.type.startsWith('image/')) return;

    target.src = URL.createObjectURL(file);
    target.classList.remove('d-none');

    if (input.dataset.previewHide) {
        var hide = document.querySelector(input.dataset.previewHide);
        if (hide) hide.classList.add('d-none');
    }
});

// Fade out success alerts after a few seconds (errors stay until dismissed).
window.addEventListener('load', function () {
    document.querySelectorAll('.alert-success').forEach(function (el) {
        setTimeout(function () {
            if (window.bootstrap) { bootstrap.Alert.getOrCreateInstance(el).close(); }
        }, 6000);
    });
});
