function toggleDropdown(uniqueId) {
    const dropdown = document.querySelector(`#dropdown-${uniqueId}`);
    dropdown.style.display = dropdown.style.display === 'none' || dropdown.style.display === '' ? 'block' : 'none';
}

function updateSelectedCount(uniqueId, displayName) {
    const checkboxes = document.querySelectorAll(`#dropdown-${uniqueId} .custom-option input[type="checkbox"]`);
    const selectedCount = Array.from(checkboxes).filter(checkbox => checkbox.checked).length;

    const button = document.querySelector(`#dropdown-${uniqueId}`).previousElementSibling;
    if (selectedCount > 0) {
        button.textContent = `Selected: ${selectedCount} ${displayName}`;
    } else {
        button.textContent = `Select ${displayName}`;
    }
}

document.addEventListener('click', function (event) {
    const containers = document.querySelectorAll('.custom-select-container');
    containers.forEach(container => {
        if (!container.contains(event.target)) {
            const dropdown = container.querySelector('.custom-select-dropdown');
            dropdown.style.display = 'none';
        }
    });
});

function getSelectedValues(propertyName) {
    const selectedValues = [];
    document.querySelectorAll(`#dropdown-${propertyName} input[type="checkbox"]:checked`).forEach(checkbox => {
        selectedValues.push(parseInt(checkbox.value));
    });
    return selectedValues;
}
function resetValues(propertyName) {
    document.querySelectorAll(`#dropdown-${propertyName} input[type="checkbox"]:checked`).forEach(checkbox => {
        checkbox.checked = false;
    });
}