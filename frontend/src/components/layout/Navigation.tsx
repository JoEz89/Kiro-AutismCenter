import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useLocalization } from '@/hooks';
import { LanguageSwitcher } from '@/components/ui';
import { cn } from '@/lib/utils';

interface NavigationProps {
  className?: string;
  id?: string;
}

export const Navigation: React.FC<NavigationProps> = ({ className, id }) => {
  const { t, isRTL } = useLocalization();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const navigationItems = [
    { key: 'home', href: '/', label: t('navigation.home') },
    { key: 'products', href: '/products', label: t('navigation.products') },
    { key: 'courses', href: '/courses', label: t('navigation.courses') },
    { key: 'appointments', href: '/appointments', label: t('navigation.appointments') },
    { key: 'about', href: '/about', label: t('navigation.about') },
    { key: 'contact', href: '/contact', label: t('navigation.contact') },
  ];

  const authItems = [
    { key: 'login', href: '/login', label: t('navigation.login') },
    { key: 'register', href: '/register', label: t('navigation.register') },
  ];

  const isActiveLink = (href: string) => {
    return location.pathname === href;
  };

  return (
    <nav 
      id={id}
      className={cn(
        'bg-white dark:bg-gray-900 shadow-sm border-b border-gray-200 dark:border-gray-700',
        className
      )}
      role="navigation"
      aria-label={t('accessibility.mainNavigation', 'Main navigation')}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <div className="flex-shrink-0">
            <Link 
              to="/" 
              className="flex items-center space-x-2 rtl:space-x-reverse"
              aria-label={t('navigation.home')}
            >
              <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-lg">A</span>
              </div>
              <span className="text-xl font-bold text-gray-900 dark:text-white">
                {t('home.welcome').replace('Welcome to ', '')}
              </span>
            </Link>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden md:block">
            <div className="ml-10 flex items-baseline space-x-4 rtl:space-x-reverse">
              {navigationItems.map((item) => (
                <Link
                  key={item.key}
                  to={item.href}
                  className={cn(
                    'px-3 py-2 rounded-md text-sm font-medium transition-colors',
                    'focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2',
                    isActiveLink(item.href)
                      ? 'bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-white'
                  )}
                  aria-current={isActiveLink(item.href) ? 'page' : undefined}
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </div>

          {/* Right side items */}
          <div className="hidden md:flex items-center space-x-4 rtl:space-x-reverse">
            <LanguageSwitcher variant="toggle" />
            
            {/* Auth buttons */}
            <div className="flex items-center space-x-2 rtl:space-x-reverse">
              {authItems.map((item) => (
                <Link
                  key={item.key}
                  to={item.href}
                  className={cn(
                    'px-4 py-2 rounded-md text-sm font-medium transition-colors',
                    'focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2',
                    item.key === 'register'
                      ? 'bg-blue-600 text-white hover:bg-blue-700'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
                  )}
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </div>

          {/* Mobile menu button */}
          <div className="md:hidden flex items-center space-x-2 rtl:space-x-reverse">
            <LanguageSwitcher variant="toggle" />
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className={cn(
                'inline-flex items-center justify-center p-2 rounded-md',
                'text-gray-700 dark:text-gray-300',
                'hover:bg-gray-100 dark:hover:bg-gray-800',
                'focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2'
              )}
              aria-expanded={isMobileMenuOpen}
              aria-label={isMobileMenuOpen ? t('accessibility.closeMenu') : t('accessibility.openMenu')}
              aria-controls="mobile-menu"
            >
              <span className="sr-only">Open main menu</span>
              {!isMobileMenuOpen ? (
                <svg className="block h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              ) : (
                <svg className="block h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              )}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden" id="mobile-menu">
          <div className="px-2 pt-2 pb-3 space-y-1 sm:px-3 bg-gray-50 dark:bg-gray-800">
            {navigationItems.map((item) => (
              <Link
                key={item.key}
                to={item.href}
                className={cn(
                  'block px-3 py-2 rounded-md text-base font-medium transition-colors',
                  'focus:outline-none focus:ring-2 focus:ring-blue-500',
                  isActiveLink(item.href)
                    ? 'bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300'
                    : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                )}
                onClick={() => setIsMobileMenuOpen(false)}
                aria-current={isActiveLink(item.href) ? 'page' : undefined}
              >
                {item.label}
              </Link>
            ))}
            
            {/* Mobile auth buttons */}
            <div className="pt-4 pb-3 border-t border-gray-200 dark:border-gray-700">
              {authItems.map((item) => (
                <Link
                  key={item.key}
                  to={item.href}
                  className={cn(
                    'block px-3 py-2 rounded-md text-base font-medium transition-colors',
                    'focus:outline-none focus:ring-2 focus:ring-blue-500',
                    item.key === 'register'
                      ? 'bg-blue-600 text-white hover:bg-blue-700 mx-3'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                  )}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </div>
        </div>
      )}
    </nav>
  );
};

export default Navigation;