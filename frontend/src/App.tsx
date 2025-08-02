import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { HelmetProvider } from 'react-helmet-async';
import { AuthProvider, LanguageProvider, ThemeProvider } from '@/context';
import { SkipLink } from '@/components';
import { useLocalization } from '@/hooks';
import HomePage from './pages/HomePage';

function AppContent() {
  const { t } = useLocalization();

  return (
    <Router>
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
        {/* Skip Links for Accessibility */}
        <SkipLink href="#main-content">
          {t('accessibility.skipToMain', 'Skip to main content')}
        </SkipLink>
        <SkipLink href="#navigation">
          {t('accessibility.skipToNavigation', 'Skip to navigation')}
        </SkipLink>
        
        <Routes>
          <Route path="/" element={<HomePage />} />
        </Routes>
      </div>
    </Router>
  );
}

function App() {
  return (
    <HelmetProvider>
      <ThemeProvider>
        <LanguageProvider>
          <AuthProvider>
            <AppContent />
          </AuthProvider>
        </LanguageProvider>
      </ThemeProvider>
    </HelmetProvider>
  );
}

export default App;
