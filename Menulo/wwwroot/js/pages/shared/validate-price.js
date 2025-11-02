document.addEventListener('DOMContentLoaded', function () {
    const priceInput = document.getElementById('priceInput');
    const createForm = document.getElementById('createForm') || document.querySelector('form');

    if (createForm && priceInput) {
        createForm.addEventListener('submit', function (event) {
            const cleanValue = priceInput.value.replace(/,/g, '');
            if (cleanValue.trim() === '') {
                priceInput.value = "0";
            } else {
                priceInput.value = cleanValue;
            }
        });
    }

    if (priceInput) {
        priceInput.addEventListener('blur', function () {
            if (this.value.trim() === '') {
                clearErrorMessage(this);
            }
        });
    }

    if (priceInput) {
        priceInput.addEventListener('blur', function () {
            const cleanValue = this.value.replace(/,/g, '');
            if (cleanValue.trim() === '') {
                this.value = '';
                clearErrorMessage(priceInput);
            } else {
                this.value = cleanValue;
            }
        });

        // Khi focus lại thì format lại
        priceInput.addEventListener('focus', function () {
            if (this.value && this.value !== '0') {
                formatAndValidatePrice(this);
            }
        });
    }
});

function allowOnlyNumbers(e) {
    const charCode = e.which ? e.which : e.keyCode;
    // Allow: backspace, delete, tab, escape, enter, period, and numbers
    if (
        [8, 9, 27, 13, 46].includes(charCode) ||
        (charCode >= 48 && charCode <= 57)
    ) {
        return true;
    }
    e.preventDefault();
    return false;
}

function formatAndValidatePrice(input) {
    // Lưu vị trí con trỏ
    const cursorPosition = input.selectionStart;
    const oldValue = input.value;

    // 1) Xóa hết ký tự không hợp lệ (chỉ giữ số và dấu .)
    let cleanValue = input.value.replace(/[^\d.]/g, '');

    // 2) Xử lý nhiều dấu chấm - chỉ giữ dấu chấm đầu tiên
    const dotIndex = cleanValue.indexOf('.');
    if (dotIndex !== -1) {
        cleanValue = cleanValue.substring(0, dotIndex + 1) +
            cleanValue.substring(dotIndex + 1).replace(/\./g, '');
    }

    // 3) Tách phần integer và decimal
    const parts = cleanValue.split('.');
    let intPart = parts[0] || '';
    let decPart = parts[1] || '';

    // 4) Giới hạn decimal chỉ 2 chữ số
    if (decPart.length > 2) {
        decPart = decPart.substring(0, 2);
    }

    // 5) Format integer với dấu phẩy
    const formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

    // 6) Tạo giá trị mới
    let newValue = formattedInt;
    if (parts.length > 1) {
        newValue += '.' + decPart;
    }

    // 7) Cập nhật input nếu có thay đổi
    if (input.value !== newValue) {
        input.value = newValue;

        // 8) Khôi phục vị trí con trỏ
        const commasAdded = (newValue.match(/,/g) || []).length - (oldValue.match(/,/g) || []).length;
        const newCursorPosition = Math.min(cursorPosition + commasAdded, newValue.length);

        setTimeout(() => {
            if (newCursorPosition >= 0) {
                input.setSelectionRange(newCursorPosition, newCursorPosition);
            }
        }, 0);
    }
}

function validateInput(inputElement) {
    const value = inputElement.value.trim();

    // Clear error cũ
    clearErrorMessage(inputElement);

    // Nếu rỗng thì coi như 0 - hợp lệ
    if (value === '') {
        return true;
    }

    // Lấy giá trị clean (không có dấu phẩy)
    const cleanValue = value.replace(/,/g, '');

    // Kiểm tra định dạng số
    if (!/^\d*\.?\d*$/.test(cleanValue)) {
        showErrorMessage(inputElement, "Chỉ được nhập số");
        return false;
    }

    // Kiểm tra số chữ số thập phân
    if (cleanValue.includes('.')) {
        const decimalPlaces = cleanValue.split('.')[1];
        if (decimalPlaces && decimalPlaces.length > 2) {
            showErrorMessage(inputElement, "Tối đa 2 chữ số thập phân");
            return false;
        }
    }

    // Kiểm tra giá trị
    const numValue = parseFloat(cleanValue);
    if (!isNaN(numValue)) {
        if (numValue < 0) {
            showErrorMessage(inputElement, "Giá tiền phải từ 0 trở lên");
            return false;
        }
        if (numValue > 999999999.99) {
            showErrorMessage(inputElement, "Giá quá lớn (tối đa 999,999,999.99)");
            return false;
        }
    }

    return true;
}

function showErrorMessage(input, message) {
    // Tìm span validation tương ứng
    const validationSpan = input.parentElement.querySelector('.text-danger');
    if (validationSpan) {
        validationSpan.textContent = message;
        validationSpan.style.display = 'block';
    }

    // Thêm class error cho input
    input.classList.add('is-invalid');
}

function clearErrorMessage(input) {
    // Xóa thông báo lỗi
    const validationSpan = input.parentElement.querySelector('.text-danger');
    if (validationSpan) {
        validationSpan.textContent = '';
        validationSpan.style.display = 'none';
    }

    // Xóa class error
    input.classList.remove('is-invalid');
}

// CSS cho styling
const style = document.createElement('style');
style.textContent = `
    .is-invalid {
        border-color: #dc3545;
        box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25);
    }
    
    .text-danger {
        color: #dc3545;
        font-size: 0.875em;
        margin-top: 0.25rem;
    }
`;
document.head.appendChild(style);