import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider, LanguageProvider, ThemeProvider } from '@/context';
import HomePage from './pages/HomePage';

function App() {
  return (
    <ThemeProvider>
      <LanguageProvider>
        <AuthProvider>
          <Router>
            <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
              <Routes>
                <Route path="/" element={<HomePage />} />
              </Routes>
            </div>
          </Router>
        </AuthProvider>
      </LanguageProvider>
    </ThemeProvider>
  );
}

export default App;
