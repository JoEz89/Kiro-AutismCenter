// Performance test processor for custom metrics

module.exports = {
  // Custom functions for performance testing
  setRandomEmail: setRandomEmail,
  logResponse: logResponse,
  checkResponseTime: checkResponseTime
};

function setRandomEmail(requestParams, context, ee, next) {
  context.vars.randomEmail = `test${Math.floor(Math.random() * 10000)}@example.com`;
  return next();
}

function logResponse(requestParams, response, context, ee, next) {
  if (response.statusCode >= 400) {
    console.log(`Error response: ${response.statusCode} - ${response.body}`);
  }
  return next();
}

function checkResponseTime(requestParams, response, context, ee, next) {
  const responseTime = response.timings.response;
  
  // Log slow responses
  if (responseTime > 1000) {
    console.log(`Slow response detected: ${requestParams.url} took ${responseTime}ms`);
  }
  
  // Emit custom metrics
  ee.emit('customStat', 'response_time', responseTime);
  
  return next();
}