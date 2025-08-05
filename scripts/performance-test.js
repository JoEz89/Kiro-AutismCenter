// Performance testing script using Artillery.js
// Run with: npx artillery run scripts/performance-test.js

module.exports = {
  config: {
    target: 'http://localhost:8080',
    phases: [
      // Warm-up phase
      {
        duration: 60,
        arrivalRate: 5,
        name: 'Warm-up'
      },
      // Load test phase
      {
        duration: 300,
        arrivalRate: 20,
        name: 'Load test'
      },
      // Stress test phase
      {
        duration: 120,
        arrivalRate: 50,
        name: 'Stress test'
      }
    ],
    defaults: {
      headers: {
        'Content-Type': 'application/json'
      }
    }
  },
  scenarios: [
    {
      name: 'API Health Check',
      weight: 10,
      flow: [
        {
          get: {
            url: '/health'
          }
        }
      ]
    },
    {
      name: 'Get Products',
      weight: 30,
      flow: [
        {
          get: {
            url: '/api/products',
            qs: {
              page: 1,
              pageSize: 20
            }
          }
        }
      ]
    },
    {
      name: 'Get Courses',
      weight: 20,
      flow: [
        {
          get: {
            url: '/api/courses',
            qs: {
              page: 1,
              pageSize: 10
            }
          }
        }
      ]
    },
    {
      name: 'User Authentication Flow',
      weight: 25,
      flow: [
        {
          post: {
            url: '/api/auth/login',
            json: {
              email: 'test@example.com',
              password: 'TestPassword123!'
            },
            capture: {
              json: '$.token',
              as: 'authToken'
            }
          }
        },
        {
          get: {
            url: '/api/users/profile',
            headers: {
              'Authorization': 'Bearer {{ authToken }}'
            }
          }
        }
      ]
    },
    {
      name: 'Search Products',
      weight: 15,
      flow: [
        {
          get: {
            url: '/api/products/search',
            qs: {
              query: 'autism',
              page: 1,
              pageSize: 10
            }
          }
        }
      ]
    }
  ]
};

// Custom metrics and assertions
module.exports.config.processor = './performance-processor.js';