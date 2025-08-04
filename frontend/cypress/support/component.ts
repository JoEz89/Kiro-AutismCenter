// Import commands.js using ES2015 syntax:
import './commands';

// Import global styles
import '../../src/index.css';

// Mount command for component testing
import { mount } from 'cypress/react18';

Cypress.Commands.add('mount', mount);

// Declare mount command for TypeScript
declare global {
  namespace Cypress {
    interface Chainable {
      mount: typeof mount;
    }
  }
}