(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
/*
 * Add Time Popup - Main Screen
 */

var addTimePopup = function () {
  var addTimePopup, addTimeBtn, minusTimeBtn;
  var togglePopup = function togglePopup() {
    if (addTimePopup) {
      addTimePopup.classList.toggle("open");
    }
  };
  var init = function init() {
    addTimePopup = document.getElementById("add-time");
    addTimeBtn = document.getElementById("ec-add-time-btn");
    minusTimeBtn = document.getElementById("ec-minus-time-btn");
    if (addTimeBtn) {
      addTimeBtn.addEventListener("click", togglePopup);
    }
    if (minusTimeBtn) {
      minusTimeBtn.addEventListener("click", togglePopup);
    }
  };
  return {
    init: init
  };
}();
var _default = exports["default"] = addTimePopup;

},{}],2:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
/*
 * Add Time Popup - Travel, Break, Site
 */

var timeModule = function () {
  // Function to add or remove the open class
  var toggleOpenClass = function toggleOpenClass(action) {
    if (action === "add-time-cancel") {
      // Remove 'open' class from all divs
      document.querySelectorAll('div[class^="add-time-"]').forEach(function (div) {
        div.classList.remove("open");
      });
    } else {
      // Add 'open' class to the respective div
      var targetDiv = document.querySelector(".".concat(action));

      // Remove 'open' class from all divs to avoid multiple 'open' classes
      document.querySelectorAll('div[class^="add-time-"]').forEach(function (div) {
        div.classList.remove("open");
      });

      // Add 'open' class to the target div
      if (targetDiv) {
        targetDiv.classList.add("open");
      }
    }
  };

  // Initialization function
  var init = function init() {
    // Get all anchor tags with data-action attribute
    var buttons = document.querySelectorAll("a[data-action]");

    // Attach click event listener to each anchor
    buttons.forEach(function (button) {
      button.addEventListener("click", function () {
        var action = button.getAttribute("data-action");
        toggleOpenClass(action);
      });
    });
  };

  // Expose the init function for external usage
  return {
    init: init
  };
}();
var _default = exports["default"] = timeModule;

},{}],3:[function(require,module,exports){
"use strict";

var _togglePassword = _interopRequireDefault(require("./togglePassword.js"));
var _passwordMatch = _interopRequireDefault(require("./passwordMatch.js"));
var _passwordStrength = _interopRequireDefault(require("./passwordStrength.js"));
var _addTimePopup = _interopRequireDefault(require("./addTimePopup.js"));
var _addTimes = _interopRequireDefault(require("./addTimes.js"));
var _calendarWeekSlider = _interopRequireDefault(require("./calendarWeekSlider.js"));
var _calendarDateUpdater = _interopRequireDefault(require("./calendarDateUpdater.js"));
var _popupModule = _interopRequireDefault(require("./popupModule.js"));
var _timesheetSlider = _interopRequireDefault(require("./timesheetSlider.js"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { "default": e }; }
// import DemoConsole from "./demo";

// Initialize main navigation functionality
document.addEventListener("DOMContentLoaded", function () {
  // DemoConsole.init();
  _togglePassword["default"].init("#passwordSwitch", "#password");
  _passwordMatch["default"].init("#reset-password-form");
  _passwordStrength["default"].init("#reset-password-form");
  _passwordStrength["default"].init("#create-password-form");
  _addTimePopup["default"].init();
  _addTimes["default"].init();
  _calendarWeekSlider["default"].init();
  _calendarDateUpdater["default"].init();
  _popupModule["default"].init();
  _timesheetSlider["default"].init();
});

// Initialize the module with the selectors for the checkbox and password input

},{"./addTimePopup.js":1,"./addTimes.js":2,"./calendarDateUpdater.js":4,"./calendarWeekSlider.js":5,"./passwordMatch.js":6,"./passwordStrength.js":7,"./popupModule.js":8,"./timesheetSlider.js":9,"./togglePassword.js":10}],4:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
function _slicedToArray(r, e) { return _arrayWithHoles(r) || _iterableToArrayLimit(r, e) || _unsupportedIterableToArray(r, e) || _nonIterableRest(); }
function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }
function _unsupportedIterableToArray(r, a) { if (r) { if ("string" == typeof r) return _arrayLikeToArray(r, a); var t = {}.toString.call(r).slice(8, -1); return "Object" === t && r.constructor && (t = r.constructor.name), "Map" === t || "Set" === t ? Array.from(r) : "Arguments" === t || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(t) ? _arrayLikeToArray(r, a) : void 0; } }
function _arrayLikeToArray(r, a) { (null == a || a > r.length) && (a = r.length); for (var e = 0, n = Array(a); e < a; e++) n[e] = r[e]; return n; }
function _iterableToArrayLimit(r, l) { var t = null == r ? null : "undefined" != typeof Symbol && r[Symbol.iterator] || r["@@iterator"]; if (null != t) { var e, n, i, u, a = [], f = !0, o = !1; try { if (i = (t = t.call(r)).next, 0 === l) { if (Object(t) !== t) return; f = !1; } else for (; !(f = (e = i.call(t)).done) && (a.push(e.value), a.length !== l); f = !0); } catch (r) { o = !0, n = r; } finally { try { if (!f && null != t["return"] && (u = t["return"](), Object(u) !== u)) return; } finally { if (o) throw n; } } return a; } }
function _arrayWithHoles(r) { if (Array.isArray(r)) return r; }
/***********************************************************
 * Calendar Week Slider
 ************************************************************/

var CalendarDateUpdater = function () {
  // Format the date string (YY-MM-DD) into the desired format
  var formatDate = function formatDate(dateString) {
    // dateString: 2025-1-29 (YYYY-MM-DD)
    var _dateString$split = dateString.split("-"),
      _dateString$split2 = _slicedToArray(_dateString$split, 3),
      year = _dateString$split2[0],
      month = _dateString$split2[1],
      day = _dateString$split2[2];
    var date = new Date(year, month - 1, day); // JS months are 0-based

    var daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var monthsShort = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var dayOfWeek = daysOfWeek[date.getDay()];
    var formattedDay = date.getDate();
    var shortMonth = monthsShort[date.getMonth()];
    var formattedYear = date.getFullYear();
    return "".concat(dayOfWeek, ", ").concat(formattedDay, " ").concat(shortMonth, ", ").concat(formattedYear); // Format: Monday, 20 Jan, 2025
  };

  // Format Day
  var formatDay = function formatDay(dateString) {
    // dateString: 2025-1-29 (YYYY-MM-DD)
    var dateValue = dateString;

    // Parse the date string into a Date object
    var _dateValue$split$map = dateValue.split("-").map(Number),
      _dateValue$split$map2 = _slicedToArray(_dateValue$split$map, 3),
      year = _dateValue$split$map2[0],
      month = _dateValue$split$map2[1],
      day = _dateValue$split$map2[2];
    var date = new Date(year, month - 1, day); // JavaScript months are 0-indexed

    // Get the day as a short string (Sun, Mon, etc.)
    var daysOfWeek = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    var dayOfWeek = daysOfWeek[date.getDay()];
    return dayOfWeek; // Mon
  };

  // Initialize the calendar date updater
  var init = function init() {
    var currentDate = document.getElementById("calendar__date");
    var selectedCalendarDate = document.getElementById("selectedCalendarDate");
    $(".calendar__week [data-week-value]").click(function () {
      $(".calendar__week [data-week-value]").removeClass("active");
      $(this).addClass("active");
      var weekValue = $(this).attr("data-week-value");
      if (weekValue) {
        // currentDate.textContent = formatDate(weekValue);
        selectedCalendarDate.textContent = formatDate(weekValue);
      }
    });

    /* Show Current Date */
    if (currentDate && selectedCalendarDate) {
      var today = new Date();
      var year = today.getFullYear();
      var month = String(today.getMonth() + 1).padStart(2, "0"); // Months are 0-based, so add 1
      var day = String(today.getDate()).padStart(2, "0"); // Pad single digit days with leading zero
      var formattedDate = "".concat(year, "-").concat(month, "-").concat(day);
      currentDate.textContent = formatDate(formattedDate);
      selectedCalendarDate.textContent = formatDate(formattedDate);
    }

    /* Plot all dates  */
    var slideItems = document.querySelectorAll(".slide-item");
    if (slideItems.length) {
      slideItems.forEach(function (item) {
        var daySpan = item.querySelector(".calendar__day");

        // Select the <a> tag that contains the data-week-value attribute
        var link = item.querySelector("a[data-week-value]");
        if (link) {
          var weekValue = link.getAttribute("data-week-value"); // Get data-week-value from the <a> tag

          if (daySpan && weekValue) {
            var dayOfWeek = formatDay(weekValue); // Get the day of the week
            daySpan.textContent = dayOfWeek.toUpperCase(); // Update the day in the slide item
          }
        }
      });
    }
  };
  return {
    init: init,
    formatDate: formatDate
  };
}();

// Window Object - Required in .net functionality
window.CalendarDateUpdater = CalendarDateUpdater;
var _default = exports["default"] = CalendarDateUpdater;

},{}],5:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
function _slicedToArray(r, e) { return _arrayWithHoles(r) || _iterableToArrayLimit(r, e) || _unsupportedIterableToArray(r, e) || _nonIterableRest(); }
function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }
function _unsupportedIterableToArray(r, a) { if (r) { if ("string" == typeof r) return _arrayLikeToArray(r, a); var t = {}.toString.call(r).slice(8, -1); return "Object" === t && r.constructor && (t = r.constructor.name), "Map" === t || "Set" === t ? Array.from(r) : "Arguments" === t || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(t) ? _arrayLikeToArray(r, a) : void 0; } }
function _arrayLikeToArray(r, a) { (null == a || a > r.length) && (a = r.length); for (var e = 0, n = Array(a); e < a; e++) n[e] = r[e]; return n; }
function _iterableToArrayLimit(r, l) { var t = null == r ? null : "undefined" != typeof Symbol && r[Symbol.iterator] || r["@@iterator"]; if (null != t) { var e, n, i, u, a = [], f = !0, o = !1; try { if (i = (t = t.call(r)).next, 0 === l) { if (Object(t) !== t) return; f = !1; } else for (; !(f = (e = i.call(t)).done) && (a.push(e.value), a.length !== l); f = !0); } catch (r) { o = !0, n = r; } finally { try { if (!f && null != t["return"] && (u = t["return"](), Object(u) !== u)) return; } finally { if (o) throw n; } } return a; } }
function _arrayWithHoles(r) { if (Array.isArray(r)) return r; }
/***********************************************************
 * Calendar Week Slider
 ************************************************************/

var calendarWeekSlider = {
  init: function init() {
    var weekSlider = document.querySelector("#calendarWeekSlider");

    // Format the date string (YY-MM-DD) into the desired format
    var formatDate = function formatDate(dateString) {
      // dateString: 2025-1-29 (YYYY-MM-DD)
      var _dateString$split = dateString.split("-"),
        _dateString$split2 = _slicedToArray(_dateString$split, 3),
        year = _dateString$split2[0],
        month = _dateString$split2[1],
        day = _dateString$split2[2];
      var date = new Date(year, month - 1, day); // JS months are 0-based

      var daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
      var monthsShort = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
      var dayOfWeek = daysOfWeek[date.getDay()];
      var formattedDay = date.getDate();
      var shortMonth = monthsShort[date.getMonth()];
      var formattedYear = date.getFullYear();
      return "".concat(dayOfWeek, ", ").concat(formattedDay, " ").concat(shortMonth, ", ").concat(formattedYear); // Format: Monday, 20 Jan, 2025
    };
    if (weekSlider) {
      var $carousel = $(weekSlider); // Initialize jQuery carousel reference
      $carousel.owlCarousel({
        margin: 0,
        responsiveClass: true,
        nav: true,
        smartSpeed: 300,
        dots: false,
        items: 7,
        slideBy: 7,
        navText: ['<a href="javascript:;" class="ec-arrow-btn text-decoration-none"><span class="icon-ec-left-arrow text-white"></span></a>', '<a href="javascript:;" class="ec-arrow-btn text-decoration-none"><span class="icon-ec-right-arrow text-white"></span></a>'],
        responsive: {
          0: {
            items: 7,
            slideBy: 7,
            nav: true
          },
          700: {
            items: 7,
            slideBy: 7,
            nav: true
          },
          992: {
            items: 7,
            slideBy: 7,
            nav: true
          }
        },
        mouseDrag: false,
        touchDrag: false
        /* As developer will handle this in .netframework */
        /* 
        onTranslated: function () {
          // Select the first active owl-item
          const firstActiveItem = $carousel.find(".owl-item.active").first();
            // Get the value of the data-week-value attribute
          const weekValue = firstActiveItem.find("[data-week-value]").data("week-value");
            // Log or use the value
          const commencingWeek = document.getElementById("calendar__date");
          if (commencingWeek && weekValue) {
            commencingWeek.textContent = formatDate(weekValue);
          }
        },
        */
      });
    }
  }
};

// Window Object - Required in .net functionality
window.calendarWeekSlider = calendarWeekSlider;

// Initialize the slider
// calendarWeekSlider.init();
var _default = exports["default"] = calendarWeekSlider;

},{}],6:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
var PasswordMatchValidator = function () {
  // Private variables
  var passwordField;
  var confirmPasswordField;
  var messageField;
  var form;

  // Private function to validate passwords
  var validatePasswords = function validatePasswords() {
    var password = passwordField.value.trim();
    var confirmPassword = confirmPasswordField.value.trim();
    if (password !== confirmPassword) {
      messageField.classList.remove("d-none"); // Show the error message
      return false;
    } else {
      messageField.classList.add("d-none"); // Hide the error message
      return true;
    }
  };

  // Private function to attach event listeners
  var attachListeners = function attachListeners() {
    // Listen for input events on the password and confirm-password fields
    passwordField.addEventListener("input", validatePasswords);
    confirmPasswordField.addEventListener("input", validatePasswords);

    // Validate on form submission
    form.addEventListener("submit", function (event) {
      if (!validatePasswords()) {
        event.preventDefault();
      }
    });
  };

  // Public init function
  var init = function init(formSelector) {
    form = document.querySelector(formSelector);
    if (!form) return;
    passwordField = form.querySelector("#password");
    confirmPasswordField = form.querySelector("#confirm-password");
    messageField = form.querySelector("#password-match");
    if (passwordField && confirmPasswordField && messageField) {
      messageField.classList.add("d-none"); // Hide the error message initially
      attachListeners();
    }
  };
  return {
    init: init
  };
}();
var _default = exports["default"] = PasswordMatchValidator;

},{}],7:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.passwordStrength = exports["default"] = void 0;
// passwordStrength.js
var passwordStrength = exports.passwordStrength = function () {
  var init = function init(formSelector) {
    var form = document.querySelector(formSelector);
    if (!form) return;
    var passwordInput = form.querySelector("#password");
    var progressBar = form.querySelector("#es-progress-bar");
    var strengthText = form.querySelector("#password-strength-text");
    var updateStrength = function updateStrength() {
      var password = passwordInput.value;
      var strength = 0;

      // Conditions
      var isLongEnough = password.length >= 10;
      var hasUppercase = /[A-Z]/.test(password);
      var hasLowercase = /[a-z]/.test(password);
      var hasNumber = /\d/.test(password);
      if (isLongEnough) strength += 25;
      if (hasUppercase) strength += 25;
      if (hasLowercase) strength += 25;
      if (hasNumber) strength += 25;

      // Update progress bar
      progressBar.style.width = "".concat(strength, "%");

      // Update strength text
      var strengthLabel = "";
      if (strength === 25) {
        strengthLabel = "Poor";
        progressBar.className = "progress-bar bg-ec-red";
      } else if (strength === 50) {
        strengthLabel = "Weak";
        progressBar.className = "progress-bar bg-warning";
      } else if (strength === 75) {
        strengthLabel = "Medium";
        progressBar.className = "progress-bar bg-info";
      } else if (strength === 100) {
        strengthLabel = "Strong";
        progressBar.className = "progress-bar bg-success";
      }
      strengthText.textContent = strengthLabel;
    };
    passwordInput.addEventListener("input", updateStrength);
  };
  return {
    init: init
  };
}();
var _default = exports["default"] = passwordStrength;

},{}],8:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
/*********************************
 *    Popup
 **********************************/
var PopupModule = function () {
  var bodyEl;
  var popup;
  var backdrop;

  // Function to create and append backdrop
  function createBackdrop() {
    if (!backdrop) {
      backdrop = document.createElement("div");
      backdrop.className = "popup-backdrop";
    }
    if (!bodyEl.contains(backdrop)) {
      bodyEl.appendChild(backdrop);
    }
  }

  // Function to open popup
  function openPopup() {
    if (popup) {
      popup.classList.add("open");
      bodyEl.classList.add("popup-open");
      createBackdrop();
    }
  }

  // Function to close popup
  function closePopup() {
    if (popup) {
      popup.classList.remove("open");
      bodyEl.classList.remove("popup-open");
      if (backdrop && bodyEl.contains(backdrop)) {
        bodyEl.removeChild(backdrop);
      }
    }
  }

  // Public init function
  function init() {
    bodyEl = document.body;
    popup = document.getElementById("popup");

    // Handle click outside of popup
    document.addEventListener("click", function (event) {
      if (popup && popup.classList.contains("open")) {
        if (!event.target.closest('[data-popup="inner"]')) {
          closePopup(); // Close popup if clicked outside
          var adminModels = document.querySelectorAll(".admin-modal");
          if (adminModels.length > 0) {
            adminModels.forEach(function (modelPopup) {
              modelPopup.classList.add("d-none");
            });
          }
        }
      }
    });

    // Handle Click outside of popup for admin popups

    // Open popup on specific triggers
    var submitWeekelyTimesheetBtn = document.querySelector('[data-action="open-submit-modal"]');
    if (submitWeekelyTimesheetBtn) {
      submitWeekelyTimesheetBtn.addEventListener("click", function (event) {
        event.stopPropagation(); // Prevent immediate document click listener execution
        openPopup();
        /* Update Commencing Date in submit modal popup  */
        var commencingDate = document.getElementById("calendar__date");
        if (commencingDate) {
          var dateText = commencingDate.textContent;
          var modalCommencingDate = document.getElementById("submit-modal__date");
          modalCommencingDate.textContent = dateText; // Set the text for the new span
        }
      });
    }
    // Close popup on cancel button click
    var cancelWeekelyTimesheetBtn = document.querySelector('[data-action="close-submit-modal"]');
    if (cancelWeekelyTimesheetBtn) {
      cancelWeekelyTimesheetBtn.addEventListener("click", function (event) {
        event.stopPropagation(); // Prevent propagation to document click listener
        closePopup();
      });
    }

    // Open admin Modal on specific trigger
    var openAdminModalBtns = document.querySelectorAll('[data-action="open-admin-modal"]');

    /* Common admin modals with ids  */
    var resendPasswordEmployeeModal = document.getElementById("resendPasswordEmployee");
    var activateDeactivateEmployeeModal = document.getElementById("activateDeactivateEmployee");
    var keepDiscardChangesModal = document.getElementById("keepDiscardChanges");
    var editTimesheetEmployee = document.getElementById("editTimesheetEmployee");
    if (openAdminModalBtns) {
      openAdminModalBtns.forEach(function (item) {
        // console.log(item);
        item.addEventListener("click", function (event) {
          event.stopPropagation(); // Prevent immediate document click listener execution
          // Check if the clicked button has a child with the class 'icon-ec-delete'
          var hasChildWithAttrDeactivateEmployee = event.currentTarget.querySelector('[data-action="deactivateEmployee"]') !== null;
          var hasChildWithAttrResendPass = event.currentTarget.querySelector('[data-action="resendPassword"]') !== null;
          var hasChildWithAttrEditEmployee = event.currentTarget.querySelector('[data-action="editEmployee"]') !== null;
          var hasChildWithAttrEditTimesheet = event.currentTarget.querySelector('[data-action="editTimeSheet"]') !== null;
          // console.log(event.target);
          if (hasChildWithAttrDeactivateEmployee) {
            if (activateDeactivateEmployeeModal) {
              activateDeactivateEmployeeModal.classList.remove("d-none");
            }
          } else if (hasChildWithAttrResendPass) {
            if (resendPasswordEmployeeModal) {
              resendPasswordEmployeeModal.classList.remove("d-none");
            }
          } else if (hasChildWithAttrEditEmployee) {
            keepDiscardChangesModal.classList.remove("d-none");
          } else if (hasChildWithAttrEditTimesheet) {
            editTimesheetEmployee.classList.remove("d-none");
          } else {
            console.log("The button has neither 'icon-ec-deleted' nor 'icon-ec-unlock' as a child.");
          }
          openPopup();
        });
      });
      // console.log(openAdminModalBtns);
    }

    // Close admin Modal on specific trigger
    var closeAdminModalBtns = document.querySelectorAll('[data-action="close-admin-modal"]');
    if (closeAdminModalBtns) {
      closeAdminModalBtns.forEach(function (item) {
        item.addEventListener("click", function (event) {
          event.stopPropagation(); // Prevent immediate document click listener execution
          closePopup();
          /* When user cancel modal - hide all admin-modals */
          setTimeout(function () {
            resendPasswordEmployeeModal.classList.add("d-none");
            activateDeactivateEmployeeModal.classList.add("d-none");
            keepDiscardChangesModal.classList.add("d-none");
            editTimesheetEmployee.classList.add("d-none");
          }, 500);
        });
      });
    }
  }

  // Return public API
  return {
    init: init,
    closePopup: closePopup
  };
}();

// Window Object - Required in .net functionality
window.PopupModule = PopupModule;

// Export the module
var _default = exports["default"] = PopupModule;

},{}],9:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
/***********************************************************
 * Timesheet Slider
 ************************************************************/
var timesheetSlider = {
  init: function init() {
    var timesheetSlider = document.querySelectorAll(".timesheet-slider");

    // Loop through each slider and initialize Owl Carousel
    timesheetSlider.forEach(function (slider) {
      var $carousel = $(slider);
      $carousel.owlCarousel({
        margin: 4,
        responsiveClass: true,
        nav: true,
        smartSpeed: 300,
        dots: false,
        items: 5,
        slideBy: 1,
        navText: ['<div class="es-button bodyBold text-white text-decoration-none d-inline-flex"><span class="icon-ec-left-arrow"></span></div>', '<div class="es-button bodyBold text-white text-decoration-none d-inline-flex"><span class="icon-ec-right-arrow"></span></div>'],
        responsive: {
          0: {
            items: 3,
            slideBy: 1,
            nav: true
          },
          700: {
            items: 5,
            slideBy: 1,
            nav: true
          },
          992: {
            items: 5,
            slideBy: 1,
            nav: true
          }
        },
        mouseDrag: false,
        touchDrag: false
      });
    });
  }
};

// Window Object - Required in .net functionality
window.timesheetSlider = timesheetSlider;

// Initialize the slider
timesheetSlider.init();
var _default = exports["default"] = timesheetSlider;

},{}],10:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;
/**
 * Toggle Password
 */

var TogglePassword = function () {
  var togglePasswordVisibility = function togglePasswordVisibility(passwordInput) {
    passwordInput.type = passwordInput.type === "password" ? "text" : "password";
  };
  var init = function init(toggleSelector, passwordSelector) {
    var toggleSwitch = document.querySelector(toggleSelector);
    var passwordInput = document.querySelector(passwordSelector);
    if (toggleSwitch && passwordInput) {
      toggleSwitch.addEventListener("change", function () {
        return togglePasswordVisibility(passwordInput);
      });
    }
  };
  return {
    init: init
  };
}();
var _default = exports["default"] = TogglePassword;

},{}]},{},[3]);
