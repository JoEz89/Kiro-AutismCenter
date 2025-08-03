import React, { useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAppointments } from '../../hooks/useAppointments';
import AppointmentList from './AppointmentList';
import AppointmentBookingForm from './AppointmentBookingForm';
import RescheduleModal from './RescheduleModal';
import type { Appointment } from '../../types';

type TabType = 'upcoming' | 'past' | 'book';

const AppointmentDashboard: React.FC = () => {
  const { t } = useLocalization();
  const { appointments } = useAppointments();
  const [activeTab, setActiveTab] = useState<TabType>('upcoming');
  const [rescheduleAppointment, setRescheduleAppointment] = useState<Appointment | null>(null);
  const [showBookingSuccess, setShowBookingSuccess] = useState(false);

  const getUpcomingCount = () => {
    const now = new Date();
    return appointments.filter(apt => 
      new Date(apt.appointmentDate) > now && 
      apt.status !== 'cancelled' && 
      apt.status !== 'completed'
    ).length;
  };

  const getPastCount = () => {
    const now = new Date();
    return appointments.filter(apt => 
      new Date(apt.appointmentDate) <= now || 
      apt.status === 'completed' || 
      apt.status === 'cancelled'
    ).length;
  };

  const handleBookingSuccess = () => {
    setShowBookingSuccess(true);
    setActiveTab('upcoming');
    setTimeout(() => setShowBookingSuccess(false), 5000);
  };

  const handleReschedule = (appointment: Appointment) => {
    setRescheduleAppointment(appointment);
  };

  const handleRescheduleSuccess = () => {
    setRescheduleAppointment(null);
    // Optionally show a success message
  };

  const tabs = [
    {
      id: 'upcoming' as TabType,
      label: t('appointments.upcoming'),
      count: getUpcomingCount(),
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      )
    },
    {
      id: 'past' as TabType,
      label: t('appointments.past'),
      count: getPastCount(),
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      )
    },
    {
      id: 'book' as TabType,
      label: t('appointments.book'),
      count: null,
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
      )
    }
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            {t('appointments.myAppointments')}
          </h1>
          <p className="text-gray-600">
            {t('appointments.dashboardDescription')}
          </p>
        </div>

        {/* Success Message */}
        {showBookingSuccess && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-md">
            <div className="flex items-center">
              <svg className="w-5 h-5 text-green-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-green-800">
                {t('appointments.bookingSuccessMessage')}
              </p>
            </div>
          </div>
        )}

        {/* Tabs */}
        <div className="mb-8">
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex space-x-8">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    flex items-center py-2 px-1 border-b-2 font-medium text-sm whitespace-nowrap
                    ${activeTab === tab.id
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    }
                  `}
                >
                  {tab.icon}
                  <span className="ml-2">{tab.label}</span>
                  {tab.count !== null && (
                    <span className={`
                      ml-2 py-0.5 px-2 rounded-full text-xs font-medium
                      ${activeTab === tab.id
                        ? 'bg-blue-100 text-blue-600'
                        : 'bg-gray-100 text-gray-600'
                      }
                    `}>
                      {tab.count}
                    </span>
                  )}
                </button>
              ))}
            </nav>
          </div>
        </div>

        {/* Tab Content */}
        <div className="bg-white rounded-lg shadow-sm">
          {activeTab === 'upcoming' && (
            <div className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-semibold text-gray-900">
                  {t('appointments.upcoming')}
                </h2>
                <button
                  onClick={() => setActiveTab('book')}
                  className="inline-flex items-center px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700"
                >
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  {t('appointments.bookNew')}
                </button>
              </div>
              <AppointmentList
                filter="upcoming"
                onReschedule={handleReschedule}
              />
            </div>
          )}

          {activeTab === 'past' && (
            <div className="p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-6">
                {t('appointments.past')}
              </h2>
              <AppointmentList filter="past" />
            </div>
          )}

          {activeTab === 'book' && (
            <div className="p-6">
              <div className="mb-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-2">
                  {t('appointments.book')}
                </h2>
                <p className="text-gray-600">
                  {t('appointments.bookingDescription')}
                </p>
              </div>
              <AppointmentBookingForm
                onSuccess={handleBookingSuccess}
                onCancel={() => setActiveTab('upcoming')}
              />
            </div>
          )}
        </div>

        {/* Quick Stats */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">
                  {t('appointments.totalAppointments')}
                </p>
                <p className="text-2xl font-semibold text-gray-900">
                  {appointments.length}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">
                  {t('appointments.completedAppointments')}
                </p>
                <p className="text-2xl font-semibold text-gray-900">
                  {appointments.filter(apt => apt.status === 'completed').length}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
                <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">
                  {t('appointments.upcomingAppointments')}
                </p>
                <p className="text-2xl font-semibold text-gray-900">
                  {getUpcomingCount()}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Reschedule Modal */}
      {rescheduleAppointment && (
        <RescheduleModal
          appointment={rescheduleAppointment}
          onClose={() => setRescheduleAppointment(null)}
          onSuccess={handleRescheduleSuccess}
        />
      )}
    </div>
  );
};

export default AppointmentDashboard;