import React, { useEffect, useState } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAppointments } from '../../hooks/useAppointments';
import type { Appointment } from '../../types';

interface AppointmentNotification {
  id: string;
  appointment: Appointment;
  type: 'reminder' | 'join' | 'missed';
  message: string;
}

const AppointmentNotifications: React.FC = () => {
  const { t, language } = useLocalization();
  const { appointments } = useAppointments();
  const [notifications, setNotifications] = useState<AppointmentNotification[]>([]);
  const [dismissed, setDismissed] = useState<Set<string>>(new Set());

  useEffect(() => {
    const checkAppointments = () => {
      const now = new Date();
      const newNotifications: AppointmentNotification[] = [];

      appointments.forEach(appointment => {
        if (appointment.status === 'cancelled' || appointment.status === 'completed') {
          return;
        }

        const appointmentDate = new Date(appointment.appointmentDate);
        const minutesUntil = (appointmentDate.getTime() - now.getTime()) / (1000 * 60);
        const hoursUntil = minutesUntil / 60;

        // 24-hour reminder
        if (hoursUntil <= 24 && hoursUntil > 23.5) {
          const notificationId = `${appointment.id}-24h`;
          if (!dismissed.has(notificationId)) {
            newNotifications.push({
              id: notificationId,
              appointment,
              type: 'reminder',
              message: t('appointments.reminder24h', {
                time: formatDateTime(appointment.appointmentDate)
              })
            });
          }
        }

        // 1-hour reminder
        if (hoursUntil <= 1 && hoursUntil > 0.5) {
          const notificationId = `${appointment.id}-1h`;
          if (!dismissed.has(notificationId)) {
            newNotifications.push({
              id: notificationId,
              appointment,
              type: 'reminder',
              message: t('appointments.reminder1h', {
                time: formatDateTime(appointment.appointmentDate)
              })
            });
          }
        }

        // Join meeting notification (15 minutes before to 15 minutes after)
        if (minutesUntil <= 15 && minutesUntil >= -15 && appointment.zoomLink) {
          const notificationId = `${appointment.id}-join`;
          if (!dismissed.has(notificationId)) {
            newNotifications.push({
              id: notificationId,
              appointment,
              type: 'join',
              message: t('appointments.readyToJoin')
            });
          }
        }

        // Missed appointment (more than 15 minutes late)
        if (minutesUntil < -15 && appointment.status === 'confirmed') {
          const notificationId = `${appointment.id}-missed`;
          if (!dismissed.has(notificationId)) {
            newNotifications.push({
              id: notificationId,
              appointment,
              type: 'missed',
              message: t('appointments.missedAppointment')
            });
          }
        }
      });

      setNotifications(newNotifications);
    };

    // Check immediately
    checkAppointments();

    // Check every minute
    const interval = setInterval(checkAppointments, 60000);

    return () => clearInterval(interval);
  }, [appointments, dismissed, t]);

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    const formattedDate = date.toLocaleDateString(language === 'ar' ? 'ar-BH' : 'en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric'
    });

    const formattedTime = date.toLocaleTimeString(language === 'ar' ? 'ar-BH' : 'en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });

    return `${formattedDate} ${t('appointments.at')} ${formattedTime}`;
  };

  const dismissNotification = (notificationId: string) => {
    setDismissed(prev => new Set([...prev, notificationId]));
  };

  const getNotificationStyle = (type: AppointmentNotification['type']) => {
    switch (type) {
      case 'reminder':
        return 'bg-blue-50 border-blue-200 text-blue-800';
      case 'join':
        return 'bg-green-50 border-green-200 text-green-800';
      case 'missed':
        return 'bg-red-50 border-red-200 text-red-800';
      default:
        return 'bg-gray-50 border-gray-200 text-gray-800';
    }
  };

  const getNotificationIcon = (type: AppointmentNotification['type']) => {
    switch (type) {
      case 'reminder':
        return (
          <svg className="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-5 5v-5zM12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        );
      case 'join':
        return (
          <svg className="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
        );
      case 'missed':
        return (
          <svg className="w-5 h-5 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
        );
      default:
        return null;
    }
  };

  if (notifications.length === 0) {
    return null;
  }

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2 max-w-sm">
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={`
            p-4 rounded-lg border shadow-lg animate-slide-in-right
            ${getNotificationStyle(notification.type)}
          `}
        >
          <div className="flex items-start">
            <div className="flex-shrink-0">
              {getNotificationIcon(notification.type)}
            </div>
            <div className="ml-3 flex-1">
              <p className="text-sm font-medium">
                {notification.message}
              </p>
              <div className="mt-2 flex space-x-2">
                {notification.type === 'join' && notification.appointment.zoomLink && (
                  <a
                    href={notification.appointment.zoomLink}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center px-3 py-1 bg-green-600 text-white text-xs font-medium rounded hover:bg-green-700"
                  >
                    <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                    {t('appointments.joinNow')}
                  </a>
                )}
                <button
                  onClick={() => dismissNotification(notification.id)}
                  className="text-xs text-gray-500 hover:text-gray-700"
                >
                  {t('common.dismiss')}
                </button>
              </div>
            </div>
            <button
              onClick={() => dismissNotification(notification.id)}
              className="flex-shrink-0 ml-2 text-gray-400 hover:text-gray-600"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      ))}
    </div>
  );
};

export default AppointmentNotifications;