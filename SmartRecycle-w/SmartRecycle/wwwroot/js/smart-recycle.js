// RePlace - Enhanced JavaScript

document.addEventListener('DOMContentLoaded', function () {
    initializeNavbarScrollEffect();
    initializeFeatureCardInteractions();
    initializeLoadingSpinner();
    initializeUserProfileInteractions();
    initializeNavLinkAnimations();
});

// تأثير التمرير على الناف بار
function initializeNavbarScrollEffect() {
    window.addEventListener('scroll', function () {
        const navbar = document.querySelector('.navbar');
        if (!navbar) return;

        if (window.scrollY > 50) {
            navbar.style.background = 'linear-gradient(135deg, rgba(26,90,40,0.95) 0%, rgba(46,125,50,0.95) 100%)';
            navbar.style.backdropFilter = 'blur(20px)';
        } else {
            navbar.style.background = 'linear-gradient(135deg, #1a5a28 0%, #2e7d32 100%)';
            navbar.style.backdropFilter = 'blur(10px)';
        }
    });
}

// تأثير النقر على البطاقات
function initializeFeatureCardInteractions() {
    const featureCards = document.querySelectorAll('.feature-card');

    featureCards.forEach(card => {
        card.addEventListener('click', function () {
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = 'translateY(-10px)';
            }, 150);
        });

        // إضافة تأثير hover إضافي
        card.addEventListener('mouseenter', function () {
            this.style.boxShadow = '0 16px 32px rgba(0, 0, 0, 0.2)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.boxShadow = '0 2px 4px rgba(0, 0, 0, 0.1)';
        });
    });
}

// إدارة تأثير التحميل
function initializeLoadingSpinner() {
    setTimeout(() => {
        const spinner = document.querySelector('.loading-spinner');
        if (spinner) {
            spinner.style.opacity = '0';
            setTimeout(() => {
                spinner.style.display = 'none';
            }, 500);
        }
    }, 2000);
}

// تأثيرات تفاعلية لملف المستخدم
function initializeUserProfileInteractions() {
    const userProfile = document.querySelector('.user-profile');
    const userAvatar = document.querySelector('.user-avatar');

    if (userProfile && userAvatar) {
        userProfile.addEventListener('mouseenter', function () {
            userAvatar.style.transform = 'rotate(360deg)';
        });

        userProfile.addEventListener('mouseleave', function () {
            userAvatar.style.transform = 'rotate(0deg)';
        });
    }

    // تأثير نقاط المستخدم
    const userPoints = document.querySelector('.user-points');
    if (userPoints) {
        userPoints.addEventListener('click', function () {
            this.style.transform = 'scale(1.1)';
            setTimeout(() => {
                this.style.transform = 'scale(1.05)';
            }, 150);
        });
    }
}

// تأثيرات لروابط الناف بار
function initializeNavLinkAnimations() {
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            // إزالة الكلاس النشط من جميع الروابط
            navLinks.forEach(l => l.classList.remove('active'));
            // إضافة الكلاس النشط للرابط المنقور عليه
            this.classList.add('active');

            // تأثير موجة عند النقر
            createRippleEffect(this, e);
        });
    });
}

// إنشاء تأثير الموجة
function createRippleEffect(element, event) {
    const ripple = document.createElement('span');
    const rect = element.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.cssText = `
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.3);
        transform: scale(0);
        animation: ripple 0.6s linear;
        width: ${size}px;
        height: ${size}px;
        left: ${x}px;
        top: ${y}px;
        pointer-events: none;
    `;

    // إضافة الأنيميشن إذا لم يكن موجود
    if (!document.querySelector('#ripple-animation')) {
        const style = document.createElement('style');
        style.id = 'ripple-animation';
        style.textContent = `
            @keyframes ripple {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }

    element.style.position = 'relative';
    element.style.overflow = 'hidden';
    element.appendChild(ripple);

    setTimeout(() => {
        ripple.remove();
    }, 600);
}

// وظائف مساعدة للتفاعل مع API
const SmartRecycleAPI = {
    // جلب نقاط المستخدم
    getUserPoints: async function (userId) {
        try {
            const response = await fetch(`/api/users/${userId}/points`);
            if (response.ok) {
                const data = await response.json();
                return data.points;
            }
        } catch (error) {
            console.error('خطأ في جلب النقاط:', error);
        }
        return 0;
    },

    // تحديث نقاط المستخدم في الواجهة
    updateUserPointsDisplay: function (points) {
        const pointsElement = document.querySelector('.user-points span');
        if (pointsElement) {
            pointsElement.textContent = points.toLocaleString('ar-SA');
        }
    },

    // إضافة نقاط جديدة
    addPoints: async function (userId, points, reason) {
        try {
            const response = await fetch('/api/users/add-points', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    userId: userId,
                    points: points,
                    reason: reason
                })
            });

            if (response.ok) {
                const data = await response.json();
                this.updateUserPointsDisplay(data.newPoints);
                this.showPointsAnimation(points);
                return true;
            }
        } catch (error) {
            console.error('خطأ في إضافة النقاط:', error);
        }
        return false;
    },

    // عرض أنيميشن النقاط
    showPointsAnimation: function (points) {
        const pointsElement = document.querySelector('.user-points');
        if (pointsElement) {
            const animation = document.createElement('div');
            animation.textContent = `+${points}`;
            animation.style.cssText = `
                position: absolute;
                color: #ffc107;
                font-weight: bold;
                font-size: 1.2rem;
                pointer-events: none;
                animation: pointsFloat 2s ease-out forwards;
                z-index: 1000;
            `;

            // إضافة الأنيميشن إذا لم يكن موجود
            if (!document.querySelector('#points-animation')) {
                const style = document.createElement('style');
                style.id = 'points-animation';
                style.textContent = `
                    @keyframes pointsFloat {
                        0% {
                            opacity: 1;
                            transform: translateY(0);
                        }
                        100% {
                            opacity: 0;
                            transform: translateY(-50px);
                        }
                    }
                `;
                document.head.appendChild(style);
            }

            pointsElement.appendChild(animation);
            setTimeout(() => animation.remove(), 2000);
        }
    }
};

// إضافة وظائف التنبيهات المخصصة
const SmartRecycleNotifications = {
    // عرض تنبيه نجاح
    showSuccess: function (message, duration = 3000) {
        this.showNotification(message, 'success', duration);
    },

    // عرض تنبيه خطأ
    showError: function (message, duration = 3000) {
        this.showNotification(message, 'error', duration);
    },

    // عرض تنبيه معلومات
    showInfo: function (message, duration = 3000) {
        this.showNotification(message, 'info', duration);
    },

    // عرض تنبيه تحذير
    showWarning: function (message, duration = 3000) {
        this.showNotification(message, 'warning', duration);
    },

    // الدالة الأساسية لعرض التنبيهات
    showNotification: function (message, type, duration) {
        const notification = document.createElement('div');
        notification.className = `smart-notification smart-notification-${type}`;
        notification.textContent = message;

        // إضافة الأنماط إذا لم تكن موجودة
        this.ensureNotificationStyles();

        document.body.appendChild(notification);

        // إظهار التنبيه
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);

        // إخفاء التنبيه
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                notification.remove();
            }, 300);
        }, duration);
    },

    // التأكد من وجود أنماط التنبيهات
    ensureNotificationStyles: function () {
        if (!document.querySelector('#notification-styles')) {
            const style = document.createElement('style');
            style.id = 'notification-styles';
            style.textContent = `
                .smart-notification {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    padding: 12px 20px;
                    border-radius: 8px;
                    color: white;
                    font-weight: 500;
                    font-size: 0.9rem;
                    z-index: 1050;
                    transform: translateX(100%);
                    opacity: 0;
                    transition: all 0.3s ease;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                }
                
                .smart-notification.show {
                    transform: translateX(0);
                    opacity: 1;
                }
                
                .smart-notification-success {
                    background: linear-gradient(135deg, #2e7d32, #4caf50);
                }
                
                .smart-notification-error {
                    background: linear-gradient(135deg, #d32f2f, #f44336);
                }
                
                .smart-notification-info {
                    background: linear-gradient(135deg, #0288d1, #03a9f4);
                }
                
                .smart-notification-warning {
                    background: linear-gradient(135deg, #f57c00, #ff9800);
                }
            `;
            document.head.appendChild(style);
        }
    }
};

// تصدير الكائنات للاستخدام العام
window.SmartRecycleAPI = SmartRecycleAPI;
window.SmartRecycleNotifications = SmartRecycleNotifications;