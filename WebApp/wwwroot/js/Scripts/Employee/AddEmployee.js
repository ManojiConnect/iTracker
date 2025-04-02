    function onCancelClick()
    {
        // Get the return URL parameters from the current URL
        const urlParams = new URLSearchParams(window.location.search);
    const returnPage = urlParams.get('returnPage') || 1;
    const searchTerm = urlParams.get('searchTerm') || '';
    const pageSize = urlParams.get('pageSize') || 10;

    // Construct the return URL
    const returnUrl = `/Employee/EmployeeList?currentPage=${returnPage}&searchTerm=${encodeURIComponent(searchTerm)}&pageSize=${pageSize}`;

    // Navigate to the employee list page
    window.location.href = returnUrl;
    }
    document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const addButton = document.querySelector("button[type='submit']");
    const inputs = form.querySelectorAll("input:not([type='hidden']), select");

    // Store initial server-side validation messages
    const initialValidationState = new Map();
    document.querySelectorAll('.text-invalid').forEach(span => {
        const input = span.closest('.d-flex').querySelector('input, select');
    if (input && span.textContent.trim()) {
        initialValidationState.set(input.id, span.textContent.trim());
        }
    });

    // Set initial button state based on server-side validation
    addButton.disabled = initialValidationState.size > 0;

    function validateField(field) {
        const errorSpan = field.parentElement?.querySelector(".text-invalid");
    if (!errorSpan) return true;

    // If field hasn't been modified by user, use server-side validation message
    if (!field.dataset.modified && initialValidationState.has(field.id)) {
        errorSpan.textContent = initialValidationState.get(field.id);
    return false;
        }

    let errorMessage = "";

    switch (field.id) {
            case "employee-number":
    if (!field.value.trim()) errorMessage = "Employee number is required.";
    break;
    case "employee-category":
    if (!field.value.trim()) errorMessage = "Employee category is required.";
    break;
    case "employee-firstname":
    if (!field.value.trim()) errorMessage = "First name is required.";
    break;
    case "employee-lastname":
    if (!field.value.trim()) errorMessage = "Last name is required.";
    break;
    case "employee-email":
    if (!field.value.trim()) {
        errorMessage = "Email is required.";
                } else if (!/^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1, 3}\.[0-9]{1, 3}\.[0-9]{1, 3}\.[0-9]{1, 3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(field.value)) {
        errorMessage = "Invalid email format.";
                }
        break;
        case "employee-contact":
        if (!field.value.trim()) {
            errorMessage = "Contact number is required.";
                } else if (!/^\+?[0-9]*$/.test(field.value)) {
            errorMessage = "Contact number can only contain numbers.";
                } else if (field.value.length > 15) {
            errorMessage = "Contact number cannot exceed 15 digits.";
                }
        break;
        case "employee-home-address":
        if (!field.value.trim()) errorMessage = "Home address is required.";
        break;
        }

        if (errorSpan) {
            errorSpan.textContent = errorMessage;
        }

        return !errorMessage;
    }

        function validateForm() {
            let isValid = true;
        inputs.forEach(input => {
            if (!validateField(input)) {
            isValid = false;
            }
        });

        addButton.disabled = !isValid;
    }

    // Attach validation to each input field
    inputs.forEach(input => {
            input.addEventListener("input", () => {
                input.dataset.modified = "true";
                validateField(input);
                validateForm();
            });

        if (input.tagName.toLowerCase() === 'select') {
            input.addEventListener("change", () => {
                input.dataset.modified = "true";
                validateField(input);
                validateForm();
            });
        }
    });

        // Validate on form submission
        form.addEventListener("submit", function (event) {
            inputs.forEach(input => {
                input.dataset.modified = "true";
            });

        let isValid = true;
        inputs.forEach(input => {
            if (!validateField(input)) {
            isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault();
        }
    });
});

