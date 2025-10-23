document.addEventListener('keydown', function (e) {
    if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return;

    if (e.altKey && e.shiftKey && e.key.toLowerCase() === 'g') {
        e.preventDefault();
        window.location.href = '/Home/Index';
    }

    if (e.altKey && e.shiftKey && e.key.toLowerCase() === 'p') {
        e.preventDefault();
        window.location.href = '/Products/Index';
    }

    if (e.altKey && e.shiftKey && e.key.toLowerCase() === 'c') {
        e.preventDefault();
        window.location.href = '/Cart/Index';
    }

    if (e.altKey && e.shiftKey && e.key.toLowerCase() === 'f') {
        e.preventDefault();
        window.location.href = '/Auth/ForgotPassword';
    }

    if (e.altKey && e.shiftKey && e.key.toLowerCase() === 'n') {
        e.preventDefault();
        window.location.href = '/Account/Profile';
    }

    if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'b') {
        e.preventDefault();
        window.history.back();
    }

    if (e.altKey && e.shiftKey && e.key === 'ArrowUp') {
        e.preventDefault();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    if (e.altKey && e.shiftKey && e.key === 'ArrowDown') {
        e.preventDefault();
        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
    }
});
