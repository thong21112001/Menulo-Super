// Chỉ cho phép nhập số
function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    // Cho phép: backspace, delete, tab, escape, enter
    if (charCode == 46 || charCode == 8 || charCode == 9 || charCode == 27 || charCode == 13 ||
        // Cho phép Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
        (charCode == 65 && evt.ctrlKey === true) ||
        (charCode == 67 && evt.ctrlKey === true) ||
        (charCode == 86 && evt.ctrlKey === true) ||
        (charCode == 88 && evt.ctrlKey === true)) {
        return true;
    }
    // Chỉ cho phép số (0-9)
    if (charCode < 48 || charCode > 57) {
        showSweetAlert('error', 'Chỉ được nhập số nguyên từ 1 đến 50!');
        return false;
    }
    return true;
}

// Validate input khi người dùng nhập
function validateTableQuantity(input) {
    var value = input.value;

    // Xóa các ký tự không phải số
    var cleanValue = value.replace(/[^0-9]/g, '');

    // Nếu có ký tự không hợp lệ bị xóa
    if (value !== cleanValue) {
        input.value = cleanValue;
        showSweetAlert('error', 'Chỉ được nhập số nguyên từ 1 đến 50!');
        return;
    }

    // Xóa số 0 ở đầu (trừ khi chỉ có 1 số 0)
    if (cleanValue.length > 1 && cleanValue.charAt(0) === '0') {
        cleanValue = cleanValue.substring(1);
        input.value = cleanValue;
    }

    // Validate range nếu không rỗng
    if (cleanValue !== '') {
        var num = parseInt(cleanValue);
        if (num < 1) {
            showSweetAlert('warning', 'Số lượng bàn phải lớn hơn 0!');
            input.classList.add('is-invalid');
        } else if (num > 50) {
            showSweetAlert('warning', 'Số lượng bàn không được vượt quá 50!');
            input.classList.add('is-invalid');
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        }
    } else {
        input.classList.remove('is-invalid', 'is-valid');
    }
}

// Xử lý paste
function handlePaste(event) {
    setTimeout(function () {
        var pastedText = event.target.value;

        // Kiểm tra nếu paste chứa ký tự không hợp lệ
        if (!/^[0-9]*$/.test(pastedText)) {
            showSweetAlert('error', 'Dữ liệu paste chứa ký tự không hợp lệ! Chỉ được nhập số nguyên từ 1 đến 50.');
        }

        validateTableQuantity(event.target);
    }, 10);
    return true;
}

// Hiển thị SweetAlert
function showSweetAlert(icon, message) {
    Swal.fire({
        icon: icon,
        title: message,
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true
    });
}

// Validate form trước khi submit
document.getElementById('createForm').addEventListener('submit', function (e) {
    var input = document.querySelector('input[name="TableQuantity"]');
    var value = input.value.trim();

    // Nếu rỗng thì OK (sẽ tự động tạo 1 bàn)
    if (value === '') {
        return true;
    }

    // Kiểm tra nếu chứa ký tự không phải số
    if (!/^[0-9]+$/.test(value)) {
        e.preventDefault();
        Swal.fire({
            icon: 'error',
            title: 'Lỗi dữ liệu',
            text: 'Số lượng bàn chỉ được nhập số nguyên từ 1 đến 50!',
            confirmButtonText: 'OK'
        }).then(() => {
            input.focus();
        });
        return false;
    }

    var num = parseInt(value);
    if (isNaN(num) || num < 1 || num > 50) {
        e.preventDefault();
        Swal.fire({
            icon: 'error',
            title: 'Số lượng không hợp lệ',
            text: 'Số lượng bàn phải từ 1 đến 50!',
            confirmButtonText: 'OK'
        }).then(() => {
            input.focus();
        });
        return false;
    }

    return true;
});

// Hiển thị thông báo khi trang load (nếu có lỗi validation từ server)
document.addEventListener('DOMContentLoaded', function () {
    var errorSpans = document.querySelectorAll('span.text-danger');
    errorSpans.forEach(function (span) {
        if (span.textContent.trim() !== '' && span.textContent.includes('bàn')) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi validation',
                text: span.textContent,
                confirmButtonText: 'OK'
            });
        }
    });
});