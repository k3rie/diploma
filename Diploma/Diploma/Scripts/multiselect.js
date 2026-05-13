window.__activeDropdown = null;
window.__activeHeader = null;

function lockBodyScroll() {
    document.body.style.overflow = 'hidden';
}

function unlockBodyScroll() {
    document.body.style.overflow = '';
}
function closeActiveDropdown() {
    if (window.__activeDropdown) {
        window.__activeDropdown.classList.remove('active');
        window.__activeHeader.classList.remove('active');
        unlockBodyScroll();

        window.__activeDropdown = null;
        window.__activeHeader = null;
    }
}
function positionDropdown(header, dropdown) {
    const rect = header.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;

    // ===== MOBILE =====
    if (viewportWidth <= 768) {
        dropdown.style.top = '';
        dropdown.style.left = '';
        dropdown.style.width = '';
        dropdown.style.bottom = '0';
        return;
    }

    // ===== DESKTOP =====

    // временно показать, чтобы измерить высоту
    const prevDisplay = dropdown.style.display;
    const prevVisibility = dropdown.style.visibility;

    dropdown.style.display = 'block';
    dropdown.style.visibility = 'hidden';

    const dropdownHeight = dropdown.offsetHeight;

    dropdown.style.display = prevDisplay;
    dropdown.style.visibility = prevVisibility;

    const spaceBelow = viewportHeight - rect.bottom;
    const spaceAbove = rect.top;

    let top;

    if (spaceBelow < dropdownHeight && spaceAbove > spaceBelow) {
        // открыть ВВЕРХ
        top = rect.top - dropdownHeight - 6;
    } else {
        // открыть ВНИЗ
        top = rect.bottom + 6;
    }

    // защита от выхода за экран
    top = Math.max(8, Math.min(top, viewportHeight - dropdownHeight - 8));

    dropdown.style.top = (top + window.scrollY) + 'px';
    dropdown.style.left = (rect.left + window.scrollX) + 'px';
}



function initMultiSelect(containerId, fieldName, options, initialSelected) {
    const selected = initialSelected || [];

    const selectHeader = document.getElementById('selectHeader_' + containerId);
    const dropdown = document.getElementById('dropdown_' + containerId);
    const searchInput = document.getElementById('searchInput_' + containerId);
    const optionsList = document.getElementById('optionsList_' + containerId);
    const selectedText = document.getElementById('selectedText_' + containerId);
    const hiddenInputsContainer = document.getElementById('hiddenInputs_' + containerId);

    function renderOptions(filter) {
        filter = filter || '';
        const filtered = options.filter(function (opt) {
            return opt.text.toLowerCase().indexOf(filter.toLowerCase()) !== -1;
        });

        if (filtered.length === 0) {
            optionsList.innerHTML = '<div class="no-results">Nothing found</div>';
            return;
        }

        const sorted = filtered.sort(function (a, b) {
            const aSelected = selected.indexOf(a.value) !== -1;
            const bSelected = selected.indexOf(b.value) !== -1;

            if (aSelected && !bSelected) return -1;
            if (!aSelected && bSelected) return 1;
            return a.text.localeCompare(b.text);
        });

        optionsList.innerHTML = sorted.map(function (opt) {
            const isChecked = selected.indexOf(opt.value) !== -1;
            return '<div class="option-item" data-value="' + opt.value + '">' +
                '<input type="checkbox" id="opt_' + containerId + '_' + opt.value + '" ' +
                'data-value="' + opt.value + '" ' +
                (isChecked ? 'checked' : '') + '>' +
                '<label for="opt_' + containerId + '_' + opt.value + '">' + opt.text + '</label>' +
                '</div>';
        }).join('');

        const items = document.querySelectorAll('#optionsList_' + containerId + ' .option-item');
        for (var i = 0; i < items.length; i++) {
            items[i].addEventListener('click', handleItemClick);
        }
    }

    function handleItemClick(e) {
        e.stopPropagation();

        if (e.target.tagName === 'INPUT') {
            handleCheckbox(e);
            return;
        }

        const item = e.currentTarget;
        const checkbox = item.querySelector('input[type="checkbox"]');
        if (checkbox) {
            checkbox.checked = !checkbox.checked;
            handleCheckbox({ target: checkbox });
        }
    }

    function handleCheckbox(e) {
        const value = e.target.getAttribute('data-value');

        if (e.target.checked) {
            if (selected.indexOf(value) === -1) {
                selected.push(value);
            }
        } else {
            const index = selected.indexOf(value);
            if (index > -1) {
                selected.splice(index, 1);
            }
        }

        updateSelectedText();
        updateHiddenInputs();

        renderOptions(searchInput.value);
    }

    function updateSelectedText() {
        if (selected.length === 0) {
            selectedText.innerHTML = 'Select options...';
        } else if (selected.length === 1) {
            const opt = options.filter(function (o) { return o.value === selected[0]; })[0];
            selectedText.innerHTML = opt ? opt.text : selected[0];
        } else {
            selectedText.innerHTML = 'Selected: <span class="selected-count">' + selected.length + '</span>';
        }
    }

    function updateHiddenInputs() {
        hiddenInputsContainer.innerHTML = selected.map(function (value) {
            return '<input type="hidden" name="' + fieldName + '" value="' + value + '">';
        }).join('');
    }

    selectHeader.addEventListener('click', function (e) {
        e.stopPropagation();

        if (dropdown.classList.contains('active')) {
            closeActiveDropdown();
            return;
        }

        closeActiveDropdown();

        document.body.appendChild(dropdown);
        positionDropdown(selectHeader, dropdown);

        selectHeader.classList.add('active');
        dropdown.classList.add('active');

        window.__activeDropdown = dropdown;
        window.__activeHeader = selectHeader;

        searchInput.value = '';
        renderOptions();
        optionsList.scrollTop = 0;
    });

    searchInput.addEventListener('input', function (e) {
        renderOptions(e.target.value);
    });

    document.addEventListener('click', function (e) {
        if (!dropdown.classList.contains('active')) return;

        if (!dropdown.contains(e.target) && !selectHeader.contains(e.target)) {
            closeActiveDropdown();
        }
    });


    renderOptions();
    updateSelectedText();
    updateHiddenInputs();
}

// ===== SINGLE SELECT =====
function initSingleSelect(containerId, fieldName, options, initialSelected, placeholderText) {
    let selectedValue = initialSelected || '';

    const selectHeader = document.getElementById('singleSelectHeader_' + containerId);
    const dropdown = document.getElementById('singleDropdown_' + containerId);
    const searchInput = document.getElementById('singleSearchInput_' + containerId);
    const optionsList = document.getElementById('singleOptionsList_' + containerId);
    const selectedText = document.getElementById('singleSelectedText_' + containerId);
    const hiddenInput = document.getElementById('singleHiddenInput_' + containerId);

    function renderOptions(filter) {
        filter = filter || '';
        const filtered = options.filter(opt => opt.text.toLowerCase().includes(filter.toLowerCase()));

        if (filtered.length === 0) {
            optionsList.innerHTML = '<div class="no-results">Nothing found</div>';
            return;
        }

        optionsList.innerHTML = filtered.map(opt => {
            const isChecked = String(selectedValue) === String(opt.value);
            return `<div class="option-item" data-value="${opt.value}">
                        <input type="radio" name="radio_${containerId}" id="opt_${containerId}_${opt.value}" 
                               data-value="${opt.value}" ${isChecked ? 'checked' : ''}>
                        <label for="opt_${containerId}_${opt.value}">${opt.text}</label>
                    </div>`;
        }).join('');

        optionsList.querySelectorAll('.option-item').forEach(item => item.addEventListener('click', handleItemClick));
        optionsList.querySelectorAll('input[type="radio"]').forEach(radio => radio.addEventListener('change', handleRadio));
    }

    function handleItemClick(e) {
        e.stopPropagation();
        const item = e.currentTarget;
        const radio = item.querySelector('input[type="radio"]');
        if (!radio) return;

        radio.checked = true;
        selectedValue = radio.dataset.value;

        updateSelectedText();
        updateHiddenInput();
        hiddenInput.dispatchEvent(new Event("change", { bubbles: true }));

        closeDropdown();
    }

    function handleRadio(e) {
        e.stopPropagation();
        selectedValue = e.target.dataset.value;

        updateSelectedText();
        updateHiddenInput();
        hiddenInput.dispatchEvent(new Event("change", { bubbles: true }));

        closeDropdown();
    }

    function updateSelectedText() {
        const opt = options.find(o => String(o.value) === String(selectedValue));
        selectedText.innerHTML = (!selectedValue || !opt) ? placeholderText : opt.text;
    }

    function updateHiddenInput() {
        hiddenInput.value = selectedValue;
    }

    function closeDropdown() {
        selectHeader.classList.remove('active');
        dropdown.classList.remove('active');
        searchInput.value = '';
        renderOptions();
        unlockBodyScroll();

        if (window.__activeDropdown === dropdown) {
            window.__activeDropdown = null;
            window.__activeHeader = null;
        }
    }

    selectHeader.addEventListener('click', function (e) {
        e.stopPropagation();

        if (dropdown.classList.contains('active')) {
            closeDropdown();
            return;
        }

        closeActiveDropdown(); // закрыть другие dropdown
        window.__activeDropdown = dropdown;
        window.__activeHeader = selectHeader;

        document.body.appendChild(dropdown);
        positionDropdown(selectHeader, dropdown);
        selectHeader.classList.add('active');
        dropdown.classList.add('active');

        lockBodyScroll();
        searchInput.value = '';
        renderOptions();
        optionsList.scrollTop = 0;
    });

    searchInput.addEventListener('input', e => renderOptions(e.target.value));

    document.addEventListener('click', function (e) {
        if (!dropdown.classList.contains('active')) return;
        if (!dropdown.contains(e.target) && !selectHeader.contains(e.target)) closeDropdown();
    });

    renderOptions();
    updateSelectedText();
    updateHiddenInput();
}
