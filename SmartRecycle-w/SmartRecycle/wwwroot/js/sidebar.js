// Sidebar JavaScript Functionality
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const dropdownToggles = document.querySelectorAll('.dropdown-toggle');
    const navLinks = document.querySelectorAll('.nav-link');

    // Toggle Sidebar
    function toggleSidebar() {
        sidebar.classList.toggle('active');
        sidebarOverlay.classList.toggle('active');
        mobileMenuToggle.classList.toggle('active');
        document.body.classList.toggle('sidebar-open');
    }

    // Close Sidebar
    function closeSidebar() {
        sidebar.classList.remove('active');
        sidebarOverlay.classList.remove('active');
        mobileMenuToggle.classList.remove('active');
        document.body.classList.remove('sidebar-open');
    }

    // Open Sidebar
    function openSidebar() {
        sidebar.classList.add('active');
        sidebarOverlay.classList.add('active');
        mobileMenuToggle.classList.add('active');
        document.body.classList.add('sidebar-open');
    }

    // Mobile Menu Toggle Event
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function (e) {
            e.preventDefault();
            toggleSidebar();
        });
    }

    // Sidebar Toggle Event (X button)
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            closeSidebar();
        });
    }

    // Overlay Click Event
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            closeSidebar();
        });
    }

    // Dropdown Toggle Events
    dropdownToggles.forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            const parentItem = this.closest('.nav-item');
            const isActive = parentItem.classList.contains('active');

            // Close all other dropdowns
            document.querySelectorAll('.nav-item.active').forEach(function (item) {
                if (item !== parentItem) {
                    item.classList.remove('active');
                }
            });

            // Toggle current dropdown
            parentItem.classList.toggle('active', !isActive);
        });
    });

    // Set Active Link
    function setActiveLink() {
        const currentPath = window.location.pathname;
        navLinks.forEach(function (link) {
            link.classList.remove('active');
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    }

    // Initialize Active Link
    setActiveLink();

    // Handle Window Resize
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 1200) {
            // Desktop: Always show sidebar
            sidebar.classList.add('active');
            sidebarOverlay.classList.remove('active');
            mobileMenuToggle.classList.remove('active');
            document.body.classList.remove('sidebar-open');
        } else {
            // Mobile/Tablet: Hide sidebar by default
            if (!mobileMenuToggle.classList.contains('active')) {
                sidebar.classList.remove('active');
                sidebarOverlay.classList.remove('active');
                document.body.classList.remove('sidebar-open');
            }
        }
    });

    // Initialize sidebar state based on screen size
    if (window.innerWidth >= 1200) {
        sidebar.classList.add('active');
    }

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
                // Close sidebar on mobile after clicking link
                if (window.innerWidth < 1200) {
                    closeSidebar();
                }
            }
        });
    });

    // Close sidebar when clicking on nav links (mobile only)
    navLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 1200 && !this.classList.contains('dropdown-toggle')) {
                setTimeout(closeSidebar, 300); // Small delay for better UX
            }
        });
    });

    // Keyboard Navigation
    document.addEventListener('keydown', function (e) {
        // Close sidebar with Escape key
        if (e.key === 'Escape' && sidebar.classList.contains('active') && window.innerWidth < 1200) {
            closeSidebar();
        }
    });

    // Touch/Swipe support for mobile
    let touchStartX = 0;
    let touchEndX = 0;

    document.addEventListener('touchstart', function (e) {
        touchStartX = e.changedTouches[0].screenX;
    }, { passive: true });

    document.addEventListener('touchend', function (e) {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }, { passive: true });

    function handleSwipe() {
        const swipeThreshold = 50;
        const swipeDistance = touchStartX - touchEndX;

        if (window.innerWidth < 1200) {
            // Swipe right to left (close sidebar)
            if (swipeDistance > swipeThreshold && sidebar.classList.contains('active')) {
                closeSidebar();
            }
            // Swipe left to right from right edge (open sidebar)
            else if (swipeDistance < -swipeThreshold && touchStartX > window.innerWidth - 50 && !sidebar.classList.contains('active')) {
                openSidebar();
            }
        }
    }

    // Auto-close dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.has-dropdown')) {
            document.querySelectorAll('.nav-item.active').forEach(function (item) {
                item.classList.remove('active');
            });
        }
    });

    // Highlight current page in navigation
    function highlightCurrentPage() {
        const currentController = window.location.pathname.split('/')[1];
        navLinks.forEach(function (link) {
            const linkController = link.getAttribute('href')?.split('/')[1];
            if (linkController === currentController) {
                link.classList.add('active');
            }
        });
    }

    highlightCurrentPage();

    // Add loading animation for navigation links
    navLinks.forEach(function (link) {
        if (!link.classList.contains('dropdown-toggle')) {
            link.addEventListener('click', function () {
                // Add loading state
                const icon = this.querySelector('i');
                const originalClass = icon.className;
                icon.className = 'fas fa-spinner fa-spin';

                // Reset icon after delay (in case navigation is instant)
                setTimeout(() => {
                    icon.className = originalClass;
                }, 2000);
            });
        }
    });
});