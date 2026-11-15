// Tarayıcı doğrulaması kullanılabilirlik içindir; sunucu doğrulaması esastır.
document.querySelectorAll('form[method="get"]').forEach(form => {
    const from = form.querySelector('[name="from"]');
    const to = form.querySelector('[name="to"]');
    if (!from || !to) return;
    const validate = () => {
        to.setCustomValidity(from.value && to.value && from.value > to.value
            ? 'Bitiş tarihi başlangıç tarihinden önce olamaz.' : '');
    };
    from.addEventListener('input', validate);
    to.addEventListener('input', validate);
    validate();
});
