const HomePage = () => {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          Autism Center Website
        </h1>
        <p className="text-lg text-gray-600 mb-8">
          Welcome to the Autism Center - Your comprehensive resource for autism support and services
        </p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-xl font-semibold mb-3">E-commerce</h3>
            <p className="text-gray-600">Browse and purchase autism-related products and resources</p>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-xl font-semibold mb-3">Online Courses</h3>
            <p className="text-gray-600">Access educational content and training materials</p>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-xl font-semibold mb-3">Appointments</h3>
            <p className="text-gray-600">Schedule consultations with autism specialists</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;