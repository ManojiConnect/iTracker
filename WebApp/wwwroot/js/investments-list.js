document.addEventListener('DOMContentLoaded', function () {
    initializePortfolioFilter();
    initializeCategoryFilter();
    initializeSearch();
    initializePagination();
    initializeSorting();
});

function initializePortfolioFilter() {
    const portfolioFilter = document.getElementById('portfolioFilter');
    if (!portfolioFilter) {
        console.error('Portfolio filter element not found');
        return;
    }
    
    console.log('Initializing portfolio filter');
    
    portfolioFilter.addEventListener('change', function () {
        console.log('Portfolio filter changed:', this.value);
        const filterForm = document.getElementById('filterForm');
        if (!filterForm) {
            console.error('Filter form not found');
            return;
        }

        // Reset to page 1 when changing filters
        const pageInput = filterForm.querySelector('input[name="pageNumber"]');
        if (pageInput) {
            pageInput.value = '1';
        }

        filterForm.submit();
    });
}

function initializeCategoryFilter() {
    const categoryFilter = document.getElementById('category');
    if (!categoryFilter) {
        console.error('Category filter element not found');
        return;
    }
    
    // Find the filter form
    const filterForm = document.getElementById('filterForm');
    if (!filterForm) {
        console.error('Filter form not found');
        return;
    }
    
    categoryFilter.addEventListener('change', function () {
        console.log('Category filter changed:', this.value);
        
        // Reset to page 1 when changing filters
        const pageInput = filterForm.querySelector('input[name="pageNumber"]');
        if (pageInput) {
            pageInput.value = '1';
        }
        
        filterForm.submit();
    });
}

function initializeSearch() {
    const searchInput = document.getElementById('searchText');
    if (!searchInput) {
        console.error('Search input element not found');
        return;
    }
    
    let searchTimeout;
    searchInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            console.log('Search text changed:', this.value);
            const filterForm = document.getElementById('filterForm');
            if (!filterForm) {
                console.error('Filter form not found');
                return;
            }

            // Reset to page 1 when searching
            const pageInput = filterForm.querySelector('input[name="pageNumber"]');
            if (pageInput) {
                pageInput.value = '1';
            }

            filterForm.submit();
        }, 500); // Debounce search for 500ms
    });
}

function initializePagination() {
    const paginationLinks = document.querySelectorAll('.pagination .page-link');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const pageNumber = this.getAttribute('data-page');
            if (!pageNumber) {
                console.error('Page number not found on link');
                return;
            }

            const filterForm = document.getElementById('filterForm');
            if (!filterForm) {
                console.error('Filter form not found');
                return;
            }

            const pageInput = filterForm.querySelector('input[name="pageNumber"]');
            if (pageInput) {
                pageInput.value = pageNumber;
                filterForm.submit();
            }
        });
    });
}

function initializeSorting() {
    let currentSortColumn = -1;
    let currentSortOrder = 'asc';

    // Find all sortable headers
    const sortableHeaders = document.querySelectorAll('th .cursor-pointer');
    if (!sortableHeaders.length) {
        console.log('No sortable headers found');
        return;
    }
    
    console.log(`Found ${sortableHeaders.length} sortable headers`);
    
    sortableHeaders.forEach((header, index) => {
        // Remove any existing click handlers
        header.removeEventListener('click', handleSortClick);
        
        // Add new click handler
        header.addEventListener('click', function() {
            handleSortClick(this, index);
        });
    });
    
    function handleSortClick(header, index) {
        const sortIcon = header.querySelector('.sort-icon');
        
        if (currentSortColumn === index) {
            currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = index;
            currentSortOrder = 'asc';
        }

        // Update sort icons
        document.querySelectorAll('.sort-icon').forEach(icon => {
            icon.textContent = '';
        });
        sortIcon.textContent = currentSortOrder === 'asc' ? 'arrow_upward' : 'arrow_downward';

        // Update table body sort order
        const tableBody = document.getElementById('tableBody');
        if (tableBody) {
            tableBody.dataset.sortOrder = currentSortOrder;
            
            // Sort the table
            sortTable(currentSortColumn, currentSortOrder);
        }
    }
}

function sortTable(columnIndex, order) {
    const tableBody = document.getElementById('tableBody');
    if (!tableBody) {
        console.error('Table body not found');
        return;
    }
    
    const rows = Array.from(tableBody.querySelectorAll('tr'));
    if (!rows.length) {
        console.log('No rows found to sort');
        return;
    }
    
    console.log(`Sorting ${rows.length} rows by column ${columnIndex} in ${order} order`);

    rows.sort((a, b) => {
        let aValue = a.cells[columnIndex].textContent.trim();
        let bValue = b.cells[columnIndex].textContent.trim();

        // Handle numeric values
        if (columnIndex >= 3) { // Initial Investment, Current Value, Return columns
            aValue = parseFloat(aValue.replace(/[^0-9.-]+/g, ''));
            bValue = parseFloat(bValue.replace(/[^0-9.-]+/g, ''));
        }

        if (order === 'asc') {
            return aValue > bValue ? 1 : -1;
        } else {
            return aValue < bValue ? 1 : -1;
        }
    });

    // Reorder rows
    rows.forEach(row => tableBody.appendChild(row));
}

function deleteInvestment(id) {
    if (confirm('Are you sure you want to delete this investment?')) {
        fetch(`/api/investments/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then(response => {
            if (response.ok) {
                window.location.reload();
            } else {
                console.error('Failed to delete investment');
            }
        })
        .catch(error => {
            console.error('Error deleting investment:', error);
        });
    }
} 
