document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const emailInput = document.getElementById("Input_Email"); // Changed to match ASP.NET Core generated ID
    const emailSpan = document.getElementById("forgotPasswordEmailSpan");

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
    span.dataset.jsError = message ? "true" : "";
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

    if (!input.dataset.modified && initialValidationState.has(input.id)) {
        span.textContent = initialValidationState.get(input.id);
    return false;
        }

    if (!value) {
        error = "Email is required.";
    } else if (!/^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(value)) {
        error = "Please enter a valid email address.";
        }

    if (error) {
        showError(input, span, error);
    return false;
        }

    clearError(span);
    return true;
    }

    if (emailInput) { // Add check to ensure emailInput exists
        emailInput.addEventListener("input", () => {
            emailInput.dataset.modified = "true";
            clearError(emailSpan);
            validateField(emailInput, emailSpan);
        });
    }

    form.addEventListener("submit", function (e) {
        if (emailInput) {
        emailInput.dataset.modified = "true";
    const isEmailValid = validateField(emailInput, emailSpan);

    if (!isEmailValid) {
        e.preventDefault();
            }
        }
    });
});