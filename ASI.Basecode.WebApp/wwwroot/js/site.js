$(function () {
    const token = document.querySelector('meta[name="RequestVerificationToken"]')?.content;
    if (token) $.ajaxSetup({ headers: { RequestVerificationToken: token } });

    document.addEventListener('click', function (event) {
        const opener = event.target.closest('[data-dialog-open]');
        if (opener) {
            const dialog = document.getElementById(opener.dataset.dialogOpen);
            if (!dialog) return;

            for (const [key, value] of Object.entries(opener.dataset)) {
                if (!key.startsWith('dialogValue')) continue;
                const field = key.slice('dialogValue'.length);
                const name = field.charAt(0).toLowerCase() + field.slice(1);
                const target = dialog.querySelector('[data-dialog-field="' + name + '"]');
                if (!target) continue;
                if (target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement) {
                    target.value = value;
                } else {
                    target.textContent = value;
                }
            }

            dialog.querySelectorAll('textarea').forEach(function (textarea) { textarea.value = ''; });
            dialog.showModal();
            dialog.querySelector('textarea, input:not([type="hidden"])')?.focus();
            return;
        }

        const closer = event.target.closest('[data-dialog-close]');
        if (closer) closer.closest('dialog')?.close();
    });

    document.addEventListener('click', function (event) {
        if (event.target instanceof HTMLDialogElement) event.target.close();
    });

    document.addEventListener('submit', function (event) {
        const submit = event.target.querySelector('button[type="submit"]');
        if (!submit) return;
        const label = submit.dataset.submittingLabel;
        submit.disabled = true;
        if (label) submit.textContent = label;
    });

    const reopen = document.querySelector('dialog[data-reopen="true"]');
    if (reopen) {
        reopen.querySelector('[data-dialog-field="id"]').value = reopen.dataset.reopenId;
        reopen.querySelector('textarea[name="reason"]').value = reopen.dataset.reopenReason ?? '';
        reopen.showModal();
        reopen.querySelector('textarea, input:not([type="hidden"])')?.focus();
    }
});
