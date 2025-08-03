import React from 'react';
import { AdminLayout, ReportsAnalytics } from '@/components/admin';
import { useLocalization } from '@/hooks';

export const AdminReportsPage: React.FC = () => {
  const { t } = useLocalization();

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Reports and analytics content */}
        <ReportsAnalytics />
      </div>
    </AdminLayout>
  );
};

export default AdminReportsPage;