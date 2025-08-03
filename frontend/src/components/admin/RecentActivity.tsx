import React, { useEffect, useState } from 'react';
import { useLocalization } from '@/hooks';
import { LoadingSpinner } from '@/components/ui';

interface ActivityItem {
  id: string;
  type: 'order' | 'user' | 'course' | 'appointment';
  action: string;
  description: string;
  timestamp: Date;
  user?: {
    name: string;
    email: string;
  };
  metadata?: Record<string, any>;
}

export const RecentActivity: React.FC = () => {
  const { t } = useLocalization();
  const [activities, setActivities] = useState<ActivityItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRecentActivity();
  }, []);

  const loadRecentActivity = async () => {
    try {
      setLoading(true);
      
      // Mock data for now - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 500));

      const mockActivities: ActivityItem[] = [
        {
          id: '1',
          type: 'order',
          action: 'created',
          description: 'New order placed',
          timestamp: new Date(Date.now() - 5 * 60 * 1000), // 5 minutes ago
          user: { name: 'Ahmed Al-Rashid', email: 'ahmed@example.com' },
          metadata: { orderId: 'ORD-2024-001234', amount: 150 }
        },
        {
          id: '2',
          type: 'user',
          action: 'registered',
          description: 'New user registration',
          timestamp: new Date(Date.now() - 15 * 60 * 1000), // 15 minutes ago
          user: { name: 'Fatima Hassan', email: 'fatima@example.com' }
        },
        {
          id: '3',
          type: 'appointment',
          action: 'booked',
          description: 'Appointment scheduled',
          timestamp: new Date(Date.now() - 30 * 60 * 1000), // 30 minutes ago
          user: { name: 'Mohammed Ali', email: 'mohammed@example.com' },
          metadata: { appointmentDate: '2024-02-15T10:00:00Z' }
        },
        {
          id: '4',
          type: 'course',
          action: 'enrolled',
          description: 'Course enrollment',
          timestamp: new Date(Date.now() - 45 * 60 * 1000), // 45 minutes ago
          user: { name: 'Sara Abdullah', email: 'sara@example.com' },
          metadata: { courseName: 'Understanding Autism Basics' }
        },
        {
          id: '5',
          type: 'order',
          action: 'shipped',
          description: 'Order shipped',
          timestamp: new Date(Date.now() - 60 * 60 * 1000), // 1 hour ago
          user: { name: 'Omar Khalil', email: 'omar@example.com' },
          metadata: { orderId: 'ORD-2024-001230', trackingNumber: 'TRK123456' }
        },
        {
          id: '6',
          type: 'appointment',
          action: 'completed',
          description: 'Appointment completed',
          timestamp: new Date(Date.now() - 90 * 60 * 1000), // 1.5 hours ago
          user: { name: 'Layla Ahmed', email: 'layla@example.com' }
        }
      ];

      setActivities(mockActivities);
    } catch (error) {
      console.error('Failed to load recent activity:', error);
    } finally {
      setLoading(false);
    }
  };

  const getActivityIcon = (type: string, action: string) => {
    const iconClass = "h-5 w-5";
    
    switch (type) {
      case 'order':
        return action === 'created' ? (
          <svg className={`${iconClass} text-green-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v6m3-3H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        ) : (
          <svg className={`${iconClass} text-blue-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 18.75a1.5 1.5 0 01-3 0V8.25a1.5 1.5 0 013 0v10.5zM12 18.75a1.5 1.5 0 01-3 0V8.25a1.5 1.5 0 013 0v10.5zM15.75 18.75a1.5 1.5 0 01-3 0V8.25a1.5 1.5 0 013 0v10.5z" />
          </svg>
        );
      case 'user':
        return (
          <svg className={`${iconClass} text-purple-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M19 7.5v3m0 0v3m0-3h3m-3 0h-3m-2.25-4.125a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zM4 19.235v-.11a6.375 6.375 0 0112.674-1.334c.343.061.672.133.995.216-.39.714-.821 1.416-1.293 2.093A8.014 8.014 0 004 19.235z" />
          </svg>
        );
      case 'course':
        return (
          <svg className={`${iconClass} text-yellow-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M4.26 10.147a60.436 60.436 0 00-.491 6.347A48.627 48.627 0 0112 20.904a48.627 48.627 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.57 50.57 0 00-2.658-.813A59.905 59.905 0 0112 3.493a59.902 59.902 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.697 50.697 0 0112 13.489a50.702 50.702 0 017.74-3.342M6.75 15a.75.75 0 100-1.5.75.75 0 000 1.5zm0 0v-3.675A55.378 55.378 0 0112 8.443m-7.007 11.55A5.981 5.981 0 006.75 15.75v-1.5" />
          </svg>
        );
      case 'appointment':
        return (
          <svg className={`${iconClass} text-indigo-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5a2.25 2.25 0 002.25-2.25m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5a2.25 2.25 0 012.25 2.25v7.5" />
          </svg>
        );
      default:
        return (
          <svg className={`${iconClass} text-gray-500`} fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z" />
          </svg>
        );
    }
  };

  const formatTimestamp = (timestamp: Date) => {
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - timestamp.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) {
      return t('admin.dashboard.justNow', 'Just now');
    } else if (diffInMinutes < 60) {
      return t('admin.dashboard.minutesAgo', '{{minutes}} minutes ago', { minutes: diffInMinutes });
    } else if (diffInMinutes < 1440) {
      const hours = Math.floor(diffInMinutes / 60);
      return t('admin.dashboard.hoursAgo', '{{hours}} hours ago', { hours });
    } else {
      return timestamp.toLocaleDateString();
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-8">
        <LoadingSpinner size="md" />
      </div>
    );
  }

  return (
    <div className="flow-root">
      <ul className="-mb-8">
        {activities.map((activity, index) => (
          <li key={activity.id}>
            <div className="relative pb-8">
              {index !== activities.length - 1 && (
                <span
                  className="absolute top-4 left-4 rtl:left-auto rtl:right-4 -ml-px rtl:-mr-px h-full w-0.5 bg-gray-200 dark:bg-gray-700"
                  aria-hidden="true"
                />
              )}
              <div className="relative flex space-x-3 rtl:space-x-reverse">
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-50 dark:bg-gray-700 ring-8 ring-white dark:ring-gray-800">
                  {getActivityIcon(activity.type, activity.action)}
                </div>
                <div className="flex min-w-0 flex-1 justify-between space-x-4 rtl:space-x-reverse pt-1.5">
                  <div>
                    <p className="text-sm text-gray-900 dark:text-white">
                      <span className="font-medium">{activity.user?.name}</span>{' '}
                      <span className="text-gray-600 dark:text-gray-400">
                        {t(`admin.activity.${activity.type}.${activity.action}`, activity.description)}
                      </span>
                    </p>
                    {activity.metadata && (
                      <div className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                        {activity.type === 'order' && activity.metadata.orderId && (
                          <span>Order: {activity.metadata.orderId}</span>
                        )}
                        {activity.type === 'course' && activity.metadata.courseName && (
                          <span>Course: {activity.metadata.courseName}</span>
                        )}
                        {activity.metadata.amount && (
                          <span className="ml-2 rtl:ml-0 rtl:mr-2">
                            Amount: {activity.metadata.amount} {t('currency.bhd', 'BHD')}
                          </span>
                        )}
                      </div>
                    )}
                  </div>
                  <div className="whitespace-nowrap text-right rtl:text-left text-sm text-gray-500 dark:text-gray-400">
                    <time dateTime={activity.timestamp.toISOString()}>
                      {formatTimestamp(activity.timestamp)}
                    </time>
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
      
      {activities.length === 0 && (
        <div className="text-center py-8">
          <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h4.125m0-15.75v15.75" />
          </svg>
          <h3 className="mt-2 text-sm font-medium text-gray-900 dark:text-white">
            {t('admin.dashboard.noActivity', 'No recent activity')}
          </h3>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            {t('admin.dashboard.noActivityDescription', 'Recent system activity will appear here.')}
          </p>
        </div>
      )}
    </div>
  );
};

export default RecentActivity;