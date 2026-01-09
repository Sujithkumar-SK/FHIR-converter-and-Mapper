export interface CreateDataRequestDto {
  globalPatientId: string;
  sourceOrganizationId: string;
  notes?: string;
}

export interface ApproveDataRequestDto {
  status: DataRequestStatus;
  notes?: string;
}

export interface DataRequestResponse {
  requestId: string;
  globalPatientId: string;
  requestingUserId: string;
  requestingOrganizationId: string;
  sourceOrganizationId: string;
  status: DataRequestStatus;
  notes?: string;
  approvedAt?: Date;
  approvedByUserId?: string;
  expiresAt: Date;
  createdOn: Date;
  requestingOrganizationName: string;
  sourceOrganizationName: string;
  requestingUserEmail: string;
  approvedByUserEmail?: string;
}

export enum DataRequestStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  DataReady = 4,
  Completed = 5,
  Expired = 6
}

export const DataRequestStatusLabels = {
  [DataRequestStatus.Pending]: 'Pending',
  [DataRequestStatus.Approved]: 'Approved',
  [DataRequestStatus.Rejected]: 'Rejected',
  [DataRequestStatus.DataReady]: 'Data Ready',
  [DataRequestStatus.Completed]: 'Completed',
  [DataRequestStatus.Expired]: 'Expired'
};