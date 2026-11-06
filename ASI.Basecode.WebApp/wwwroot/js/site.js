$(function () {
    const token = document.querySelector('meta[name="RequestVerificationToken"]')?.content;
    if (token) $.ajaxSetup({ headers: { RequestVerificationToken: token } });

    // Single host for the read-only deactivation preview dialog. The partial returns a
    // complete <dialog> so the native element supplies Escape, focus trapping and restore.
    const previewHost = document.createElement('div');
    previewHost.id = 'deactivation-preview-host';
    document.body.appendChild(previewHost);

    function clearError() {
        document.getElementById('deactivation-preview-error')?.remove();
    }

    function showError(message) {
        clearError();
        const notice = document.createElement('div');
        notice.id = 'deactivation-preview-error';
        notice.className = 'notice notice-error';
        notice.setAttribute('role', 'alert');
        notice.textContent = message;
        const main = document.getElementById('main-content');
        (main || document.body).prepend(notice);
    }

    let previewRequestId = 0;

    document.addEventListener('click', function (event) {
        const trigger = event.target.closest('[data-deactivation-preview]');
        if (!trigger) return;

        event.preventDefault();
        const requestId = ++previewRequestId;
        clearError();
        previewHost.innerHTML = '';

        fetch(trigger.dataset.deactivationPreview, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            credentials: 'same-origin'
        })
            .then(function (response) {
                if (requestId !== previewRequestId) return null;
                if (!response.ok) throw new Error('Deactivation preview request failed.');
                return response.text();
            })
            .then(function (html) {
                if (html === null || requestId !== previewRequestId) return;
                previewHost.innerHTML = html;
                const dialog = previewHost.querySelector('dialog');
                if (!dialog) throw new Error('Deactivation preview dialog was not returned.');
                dialog.addEventListener('close', function () {
                    previewHost.innerHTML = '';
                    trigger.focus();
                }, { once: true });
                dialog.showModal();
            })
            .catch(function () {
                if (requestId !== previewRequestId) return;
                previewHost.innerHTML = '';
                showError('Unable to load the deactivation preview. Please try again.');
            });
    });
});
