    document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const emailInput = document.getElementById("exampleInputEmail1");
    const passwordInput = document.getElementById("password");
    const emailSpan = document.getElementById("emailSpan");
    const passwordSpan = document.getElementById("passwordSpan");

    // Store initial server-side validation messages
    const initialValidationState = new Map();
    document.querySelectorAll('.text-invalid').forEach(span => {
        const input = span.closest('.d-flex')?.querySelector('input, select');
    if (input && span.textContent.trim()) {
        initialValidationState.set(input.id, span.textContent.trim());
        }
    });

    function showError(input, span, message) {
        if (!span) {
        console.error("Span element not found for input:", input);
    return;
        }

    span.textContent = message;
    span.dataset.jsError = message ? "true" : ""; // Mark as JS error if there's a message
    }

    function clearError(span) {
        if (span) {
        span.textContent = "";
    span.dataset.jsError = "";
        }
    }

    function validateField(input, span) {
        const value = input.value.trim();
    let error = "";

    // If the field hasn't been modified, keep the server-side error
    if (!input.dataset.modified && initialValidationState.has(input.id)) {
        span.textContent = initialValidationState.get(input.id);
    return false;
        }

    if (input.id === "exampleInputEmail1") {
            if (!value) {
        error = "Email is required.";
            } else if (!/^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(value)) {
        error = "Please enter a valid email address.";
            }
        } else if (input.id === "password") {
            if (!value) {
        error = "Password is required.";
            } else if (value.length < 10) {
        error = "The length of Password must be at least 10 characters.";
            } else if (!/[A-Z]/.test(value)) {
        error = "Password must contain at least one uppercase letter.";
            } else if (!/[a-z]/.test(value)) {
        error = "Password must contain at least one lowercase letter.";
            } else if (!/[0-9]/.test(value)) {
        error = "Password must contain at least one number.";
            }
        }

    if (error) {
        showError(input, span, error);
    return false;
        }

    clearError(span);
    return true;
    }

    emailInput.addEventListener("input", () => {
        emailInput.dataset.modified = "true"; // Mark the field as modified
    clearError(emailSpan); // Clear server-side error
    validateField(emailInput, emailSpan);
    });

    passwordInput.addEventListener("input", () => {
        passwordInput.dataset.modified = "true"; // Mark the field as modified
    clearError(passwordSpan); // Clear server-side error
    validateField(passwordInput, passwordSpan);
    });

    form.addEventListener("submit", function (e) {
        // Mark all fields as modified on form submission
        emailInput.dataset.modified = "true";
    passwordInput.dataset.modified = "true";

    const isEmailValid = validateField(emailInput, emailSpan);
    const isPasswordValid = validateField(passwordInput, passwordSpan);

    if (!isEmailValid || !isPasswordValid) {
        e.preventDefault();
        }
    });
});