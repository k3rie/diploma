// Async Search functionality - uses same styles as multiselect.js
// For searching traders and other entities asynchronously with server-side data

window.__asyncSearchInstances = {};

function initAsyncSearch(containerId, config) {
    var fieldName = config.fieldName || '';
    var placeholder = config.placeholder || 'Search...';
    var minChars = config.minChars || 2;
    var apiUrl = config.apiUrl;
    var initialSelected = config.initialSelected || [];
    var onSelectionChange = config.onSelectionChange;
    var multiple = config.multiple !== false;
    var valueField = config.valueField || 'id';
    var textField = config.textField || 'name';

    var container = document.querySelector('[data-id="' + containerId + '"]');
    if (!container) {
        console.error('AsyncSearch container not found: ' + containerId);
        return;
    }

    // Use same class names as multiselect.js for consistent styling
    var selectHeader = container.querySelector('.select-header');
    var dropdown = container.querySelector('.dropdown');
    var searchInput = container.querySelector('.search-box input');
    var optionsList = container.querySelector('.options-list');
    var selectedText = container.querySelector('.selected-text');
    var hiddenInputsContainer = container.querySelector('.hidden-inputs-container');
    var loadingIndicator = container.querySelector('.loading-indicator');

    var selected = initialSelected || [];
    var currentResults = [];
    var searchTimeout = null;

    function renderSelectedText() {
        if (selected.length === 0) {
            selectedText.innerHTML = placeholder;
            return;
        }

        if (multiple && selected.length !== 1) {
            selectedText.innerHTML = 'Selected: <span class="selected-count">' + selected.length + '</span>';
        } else {
            var item = selected[0];
            selectedText.innerHTML = item[textField];
        }
    }

    function updateHiddenInputs() {
        if (hiddenInputsContainer) {
            hiddenInputsContainer.innerHTML = selected.map(function(item) {
                return '<input type="hidden" name="' + fieldName + '" value="' + item[valueField] + '">';
            }).join('');
        }
    }

    function renderOptions(filter) {
        filter = filter || '';
        
        // Filter results from server
        var filtered = currentResults.filter(function(opt) {
            return opt[textField].toLowerCase().indexOf(filter.toLowerCase()) !== -1;
        });

        // Include selected items that might not be in current results
        var allItems = filtered.slice();
        selected.forEach(function(selItem) {
            var alreadyInList = allItems.some(function(item) {
                return String(item[valueField]) === String(selItem[valueField]);
            });
            if (!alreadyInList) {
                allItems.push(selItem);
            }
        });

        if (allItems.length === 0) {
            optionsList.innerHTML = '<div class="no-results">Nothing found.<br/> Please enter at least ' + minChars + ' values.</div>';
            return;
        }

        // Sort: selected first, then alphabetically
        var sorted = allItems.sort(function(a, b) {
            var aSelected = selected.some(function(s) { return String(s[valueField]) === String(a[valueField]); });
            var bSelected = selected.some(function(s) { return String(s[valueField]) === String(b[valueField]); });

            if (aSelected && !bSelected) return -1;
            if (!aSelected && bSelected) return 1;
            return a[textField].localeCompare(b[textField]);
        });

        optionsList.innerHTML = sorted.map(function(opt) {
            var isSelected = selected.some(function(s) { return String(s[valueField]) === String(opt[valueField]); });
            var inputType = multiple ? 'checkbox' : 'radio';
            var inputHtml = multiple 
                ? '<input type="checkbox" id="opt_' + containerId + '_' + opt[valueField] + '" data-value="' + opt[valueField] + '" ' + (isSelected ? 'checked' : '') + '>'
                : '<input type="radio" name="radio_' + containerId + '" id="opt_' + containerId + '_' + opt[valueField] + '" data-value="' + opt[valueField] + '" ' + (isSelected ? 'checked' : '') + '>';
            
            return '<div class="option-item" data-value="' + opt[valueField] + '">' +
                inputHtml +
                '<label for="opt_' + containerId + '_' + opt[valueField] + '">' + opt[textField] + '</label>' +
                '</div>';
        }).join('');

        // Add event listeners - use the actual optionsList element
        var items = optionsList.querySelectorAll('.option-item');
        for (var i = 0; i < items.length; i++) {
            items[i].addEventListener('click', handleItemClick);
        }
        
        if (multiple) {
            var checkboxes = optionsList.querySelectorAll('input[type="checkbox"]');
            checkboxes.forEach(function(cb) { cb.addEventListener('change', handleCheckbox); });
        } else {
            var radios = optionsList.querySelectorAll('input[type="radio"]');
            radios.forEach(function(radio) { radio.addEventListener('change', handleRadio); });
        }
    }

    function handleItemClick(e) {
        e.stopPropagation();

        if (e.target.tagName === 'INPUT') {
            if (multiple) {
                handleCheckbox(e);
            } else {
                handleRadio(e);
            }
            return;
        }

        var item = e.currentTarget;
        var input = item.querySelector('input[type="checkbox"], input[type="radio"]');
        if (input) {
            if (multiple) {
                // Toggle checkbox
                input.checked = !input.checked;
                handleCheckbox({ target: input });
            } else {
                input.checked = true;
                handleRadio({ target: input });
            }
        }
    }

    function handleCheckbox(e) {
        var value = e.target.getAttribute('data-value');
        // First check if item is in current results, if not check selected
        var item = currentResults.find(function(r) { return String(r[valueField]) === String(value); });
        
        if (!item) {
            // Check if already selected - we can still uncheck it
            item = selected.find(function(s) { return String(s[valueField]) === String(value); });
        }
        
        if (!item) return;

        if (e.target.checked) {
            var exists = selected.some(function(s) { return String(s[valueField]) === String(value); });
            if (!exists) {
                selected.push(item);
            }
        } else {
            selected = selected.filter(function(s) { return String(s[valueField]) !== String(value); });
        }

        renderSelectedText();
        updateHiddenInputs();
        renderOptions(searchInput.value);
    }

    function handleRadio(e) {
        var value = e.target.getAttribute('data-value');
        var item = currentResults.find(function(r) { return String(r[valueField]) === String(value); });
        
        if (!item) return;

        selected = [item];
        
        renderSelectedText();
        updateHiddenInputs();
        closeDropdown();
    }

    function performSearch(query) {
        if (query.length < minChars) {
            currentResults = [];
            renderOptions('');
            return;
        }

        if (loadingIndicator) {
            loadingIndicator.style.display = 'inline';
        }

        var url = apiUrl + (apiUrl.indexOf('?') > -1 ? '&' : '?') + 'query=' + encodeURIComponent(query);
        
        fetch(url)
            .then(function(response) {
                if (!response.ok) throw new Error('Search failed');
                return response.json();
            })
            .then(function(data) {
                currentResults = Array.isArray(data) ? data : (data.results || []);
                renderOptions(searchInput.value);
            })
            .catch(function(error) {
                console.error('Search error:', error);
                currentResults = [];
                renderOptions('');
            })
            .finally(function() {
                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }
            });
    }

    function openDropdown() {
        if (dropdown.classList.contains('active')) return;
        
        closeActiveDropdown();
        
        document.body.appendChild(dropdown);
        positionDropdown(selectHeader, dropdown);
        selectHeader.classList.add('active');
        dropdown.classList.add('active');

        window.__asyncSearchInstances[containerId] = {
            dropdown: dropdown,
            container: container,
            close: closeDropdown
        };

        // searchInput.value = '';
        // Show selected items when opening dropdown (without clearing them)
        renderOptions('');
        optionsList.scrollTop = 0;
        searchInput.focus();
    }

    function closeDropdown() {
        if (!dropdown.classList.contains('active')) return;
        
        dropdown.classList.remove('active');
        selectHeader.classList.remove('active');
        
        // Clear search but keep currentResults and selected
        // searchInput.value = '';
        // Don't clear currentResults - keep them for next open
        // But render with empty filter to show all
        
        if (window.__asyncSearchInstances[containerId]) {
            delete window.__asyncSearchInstances[containerId];
        }
    }

    selectHeader.addEventListener('click', function(e) {
        e.stopPropagation();

        if (dropdown.classList.contains('active')) {
            closeDropdown();
            return;
        }

        openDropdown();
    });

    searchInput.addEventListener('input', function(e) {
        var query = this.value;
        
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        
        searchTimeout = setTimeout(function() {
            performSearch(query);
        }, 300);
    });

    document.addEventListener('click', function(e) {
        if (!dropdown.classList.contains('active')) return;

        if (!dropdown.contains(e.target) && !selectHeader.contains(e.target)) {
            closeDropdown();
        }
    });

    // Initial render
    renderSelectedText();
    updateHiddenInputs();
}

// Global function for SingleSelectAsync
function initSingleSelectAsync(containerId, fieldName, options, selectedValue, placeholder) {
    var initialSelected = selectedValue && selectedValue.length > 0
        ? [{ id: selectedValue, name: selectedValue }]
        : [];
    
    // Override container query for single select (uses different ID prefix)
    var originalQuerySelector = document.querySelector;
    var container = document.querySelector('[data-id="' + containerId + '"]');
    
    if (container) {
        // Override querySelector for this instance to use single select IDs
        var originalFind = container.querySelector.bind(container);
        container.querySelector = function(selector) {
            if (selector === '.select-header') {
                return originalFind('#singleSelectHeader_' + containerId) || originalFind('.select-header');
            }
            if (selector === '.dropdown') {
                return originalFind('#singleDropdown_' + containerId) || originalFind('.dropdown');
            }
            if (selector === '.search-box input') {
                return originalFind('#singleSearchInput_' + containerId) || originalFind('.search-box input');
            }
            if (selector === '.options-list') {
                return originalFind('#singleOptionsList_' + containerId) || originalFind('.options-list');
            }
            if (selector === '.selected-text') {
                return originalFind('#singleSelectedText_' + containerId) || originalFind('.selected-text');
            }
            return originalFind(selector);
        };
    }
    
    return initAsyncSearch(containerId, {
        fieldName: fieldName,
        placeholder: placeholder,
        minChars: 2,
        apiUrl: options.apiUrl,
        initialSelected: initialSelected,
        multiple: false,
        valueField: 'id',
        textField: 'name'
    });
}

// Global function for MultiSelectAsync
function initMultiSelectAsync(containerId, fieldName, options, selectedValues, placeholder) {
    var initialSelected = Array.isArray(selectedValues)
        ? selectedValues.map(function(v) { return { id: v, name: v }; })
        : [];
    
    // Override container query for multi select (uses different ID prefix)
    var container = document.querySelector('[data-id="' + containerId + '"]');
    
    if (container) {
        // Override querySelector for this instance to use multi select IDs
        var originalFind = container.querySelector.bind(container);
        container.querySelector = function(selector) {
            if (selector === '.select-header') {
                return originalFind('#selectHeader_' + containerId) || originalFind('.select-header');
            }
            if (selector === '.dropdown') {
                return originalFind('#dropdown_' + containerId) || originalFind('.dropdown');
            }
            if (selector === '.search-box input') {
                return originalFind('#searchInput_' + containerId) || originalFind('.search-box input');
            }
            if (selector === '.options-list') {
                return originalFind('#optionsList_' + containerId) || originalFind('.options-list');
            }
            if (selector === '.selected-text') {
                return originalFind('#selectedText_' + containerId) || originalFind('.selected-text');
            }
            return originalFind(selector);
        };
    }
    
    return initAsyncSearch(containerId, {
        fieldName: fieldName,
        placeholder: placeholder,
        minChars: 2,
        apiUrl: options.apiUrl,
        initialSelected: initialSelected,
        multiple: true,
        valueField: 'id',
        textField: 'name'
    });
}