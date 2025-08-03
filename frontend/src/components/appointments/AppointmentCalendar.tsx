import React, { useState, useEffect } from 'react';
import { useLocalization } from '../../hooks/useLocalization';
import { useAvailableSlots } from '../../hooks/useAppointments';
import type { AppointmentSlot } from '../../services/appointmentService';

interface AppointmentCalendarProps {
  selectedDoctorId?: string;
  onSlotSelect: (slot: AppointmentSlot) => void;
  selectedSlot?: AppointmentSlot;
}

const AppointmentCalendar: React.FC<AppointmentCalendarProps> = ({
  selectedDoctorId,
  onSlotSelect,
  selectedSlot
}) => {
  const { t, language } = useLocalization();
  const { slots, loading, error, fetchAvailableSlots } = useAvailableSlots();
  const [currentDate, setCurrentDate] = useState(new Date());

  useEffect(() => {
    const startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
    const endDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);
    
    fetchAvailableSlots(
      selectedDoctorId,
      startDate.toISOString().split('T')[0],
      endDate.toISOString().split('T')[0]
    );
  }, [selectedDoctorId, currentDate, fetchAvailableSlots]);

  const getDaysInMonth = (date: Date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    const days = [];
    
    // Add empty cells for days before the first day of the month
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(null);
    }
    
    // Add days of the month
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(new Date(year, month, day));
    }
    
    return days;
  };

  const getSlotsByDate = (date: Date) => {
    const dateString = date.toISOString().split('T')[0];
    return slots.filter(slot => slot.date === dateString);
  };

  const navigateMonth = (direction: 'prev' | 'next') => {
    setCurrentDate(prev => {
      const newDate = new Date(prev);
      if (direction === 'prev') {
        newDate.setMonth(prev.getMonth() - 1);
      } else {
        newDate.setMonth(prev.getMonth() + 1);
      }
      return newDate;
    });
  };

  const formatMonthYear = (date: Date) => {
    return date.toLocaleDateString(language === 'ar' ? 'ar-BH' : 'en-US', {
      month: 'long',
      year: 'numeric'
    });
  };

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(':');
    const date = new Date();
    date.setHours(parseInt(hours), parseInt(minutes));
    
    return date.toLocaleTimeString(language === 'ar' ? 'ar-BH' : 'en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  };

  const isToday = (date: Date) => {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  };

  const isPastDate = (date: Date) => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return date < today;
  };

  const weekDays = language === 'ar' 
    ? ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
    : ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2">{t('common.loading')}</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center p-8">
        <p className="text-red-600 mb-4">{error}</p>
        <button
          onClick={() => fetchAvailableSlots(selectedDoctorId)}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          {t('common.retry')}
        </button>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Calendar Header */}
      <div className="flex items-center justify-between mb-6">
        <button
          onClick={() => navigateMonth('prev')}
          className="p-2 hover:bg-gray-100 rounded-md"
          aria-label={t('common.previous')}
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        
        <h2 className="text-xl font-semibold">
          {formatMonthYear(currentDate)}
        </h2>
        
        <button
          onClick={() => navigateMonth('next')}
          className="p-2 hover:bg-gray-100 rounded-md"
          aria-label={t('common.next')}
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
        </button>
      </div>

      {/* Week Days Header */}
      <div className="grid grid-cols-7 gap-1 mb-2">
        {weekDays.map((day) => (
          <div key={day} className="p-2 text-center text-sm font-medium text-gray-500">
            {day}
          </div>
        ))}
      </div>

      {/* Calendar Grid */}
      <div className="grid grid-cols-7 gap-1">
        {getDaysInMonth(currentDate).map((date, index) => {
          if (!date) {
            return <div key={index} className="p-2 h-24"></div>;
          }

          const daySlots = getSlotsByDate(date);
          const availableSlots = daySlots.filter(slot => slot.isAvailable);
          const isDisabled = isPastDate(date) || availableSlots.length === 0;

          return (
            <div
              key={date.toISOString()}
              className={`
                p-2 h-24 border border-gray-200 rounded-md cursor-pointer transition-colors
                ${isToday(date) ? 'bg-blue-50 border-blue-300' : ''}
                ${isDisabled ? 'bg-gray-50 cursor-not-allowed' : 'hover:bg-gray-50'}
              `}
            >
              <div className={`
                text-sm font-medium mb-1
                ${isToday(date) ? 'text-blue-600' : ''}
                ${isDisabled ? 'text-gray-400' : 'text-gray-900'}
              `}>
                {date.getDate()}
              </div>
              
              {!isDisabled && (
                <div className="space-y-1">
                  {availableSlots.slice(0, 2).map((slot) => (
                    <button
                      key={`${slot.date}-${slot.time}`}
                      onClick={() => onSlotSelect(slot)}
                      className={`
                        w-full text-xs px-1 py-0.5 rounded text-center transition-colors
                        ${selectedSlot?.date === slot.date && selectedSlot?.time === slot.time
                          ? 'bg-blue-600 text-white'
                          : 'bg-green-100 text-green-800 hover:bg-green-200'
                        }
                      `}
                    >
                      {formatTime(slot.time)}
                    </button>
                  ))}
                  {availableSlots.length > 2 && (
                    <div className="text-xs text-gray-500 text-center">
                      +{availableSlots.length - 2} {t('common.more')}
                    </div>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Legend */}
      <div className="mt-4 flex flex-wrap gap-4 text-sm">
        <div className="flex items-center">
          <div className="w-3 h-3 bg-green-100 rounded mr-2"></div>
          <span>{t('appointments.available')}</span>
        </div>
        <div className="flex items-center">
          <div className="w-3 h-3 bg-blue-600 rounded mr-2"></div>
          <span>{t('appointments.selected')}</span>
        </div>
        <div className="flex items-center">
          <div className="w-3 h-3 bg-gray-100 rounded mr-2"></div>
          <span>{t('appointments.unavailable')}</span>
        </div>
      </div>
    </div>
  );
};

export default AppointmentCalendar;