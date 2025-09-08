import React from 'react';
import { Skeleton } from './Skeleton';

export const PersonCardSkeleton: React.FC = () => {
  return (
    <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
      {/* Avatar and main info */}
      <div className="flex items-start gap-4 mb-4">
        <Skeleton variant="circular" width={48} height={48} />
        <div className="flex-1">
          <Skeleton variant="text" className="h-6 w-3/4 mb-2" />
          <Skeleton variant="text" className="h-4 w-1/2" />
        </div>
      </div>

      {/* Details */}
      <div className="space-y-3 mb-4">
        <div className="flex items-center gap-2">
          <Skeleton variant="circular" width={16} height={16} />
          <Skeleton variant="text" className="h-4 w-32" />
        </div>
        <div className="flex items-center gap-2">
          <Skeleton variant="circular" width={16} height={16} />
          <Skeleton variant="text" className="h-4 w-24" />
        </div>
        <div className="flex items-center gap-2">
          <Skeleton variant="circular" width={16} height={16} />
          <Skeleton variant="text" className="h-4 w-40" />
        </div>
      </div>

      {/* Button */}
      <Skeleton variant="rectangular" className="h-10 w-full rounded-lg" />
    </div>
  );
};

interface PersonCardSkeletonGridProps {
  count?: number;
}

export const PersonCardSkeletonGrid: React.FC<PersonCardSkeletonGridProps> = ({ 
  count = 6 
}) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {Array.from({ length: count }).map((_, index) => (
        <PersonCardSkeleton key={index} />
      ))}
    </div>
  );
};
