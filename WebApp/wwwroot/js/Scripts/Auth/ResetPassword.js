document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("reset-password-form");
    const passwordInput = document.getElementById("password");
    const confirmPasswordInput = document.getElementById("confirm-password");
    const passwordSpan = document.querySelector("[data-valmsg-for='ResetPasswordRequest.NewPassword']");
    const confirmPasswordSpan = document.querySelector("[data-valmsg-for='ResetPasswordRequest.ConfirmPassword']");

    // Store initial server-side validation messages
    const initialValidationState = new Map();
    document.querySelectorAll('.text-invalid').forEach(span => {
        const input = span.closest('.d-flex')?.querySelector('input');
        if (input && span.textContent.trim()) {
            initialValidationState.set(input.id, span.textContent.trim());
        }
    });

    function showError(span, message) {
        if (span) {
            span.textContent = message;
            span.dataset.jsError = message ? "true" : "";
        }
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

        if (input.id === "password") {
            if (!value) {
                error = "Password is required.";
            }
        } else if (input.id === "confirm-password") {
            if (!value) {
                error = "Confirm password is required.";
            } else if (value !== passwordInput.value) {
                error = "Passwords do not match.";
            }
        }

        if (error) {
            showError(span, error);
            return false;
        }

        clearError(span);
        return true;
    }

    passwordInput.addEventListener("input", () => {
        passwordInput.dataset.modified = "true";
        clearError(passwordSpan);
        validateField(passwordInput, passwordSpan);
    });

    confirmPasswordInput.addEventListener("input", () => {
        confirmPasswordInput.dataset.modified = "true";
        clearError(confirmPasswordSpan);
        validateField(confirmPasswordInput, confirmPasswordSpan);
    });

    form.addEventListener("submit", function (e) {
        passwordInput.dataset.modified = "true";
        confirmPasswordInput.dataset.modified = "true";

        const isPasswordValid = validateField(passwordInput, passwordSpan);
        const isConfirmPasswordValid = validateField(confirmPasswordInput, confirmPasswordSpan);

        if (!isPasswordValid || !isConfirmPasswordValid) {
            e.preventDefault();
        }
    });
});
