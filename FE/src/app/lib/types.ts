export type Role = "tenant" | "landlord" | "admin";

export interface AppUser {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: Role;
  avatar: string;
  address?: string;
}

export interface AuthResponse {
  token?: string;
  email: string;
  fullName: string;
  role: string;
  avatarUrl?: string | null;
  refreshToken?: string | null;
  refreshTokenExpiration?: string | null;
}

export interface UserResponse {
  id: string;
  fullName: string;
  email: string;
  role: string;
  address?: string | null;
  avatarUrl?: string | null;
  phoneNumber?: string | null;
  isBlocked?: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export type AvailabilityStatus = "Available" | "Rented" | "Maintenance";
export type ModerationStatus = "Pending" | "Approved" | "Rejected";

export interface PropertyResponse {
  id: string;
  landlordId: string;
  areaId?: string | null;
  propertyName: string;
  address?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  size: number;
  description?: string | null;
  price: number;
  status: AvailabilityStatus;
  moderationStatus: ModerationStatus;
  rejectedAt?: string | null;
  rejectionReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  electricPrice?: number | null;
  waterPrice?: number | null;
  averageRating?: number;
  totalRatings?: number;
  imageUrls?: string[];
  amenityNames?: string[];
}

export interface PropertyImageResponse {
  id: string;
  propertyId: string;
  imageUrl: string;
  isPrimary: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface RoomAmenityResponse {
  id: string;
  propertyId: string;
  amenityId: string;
  amenityName: string;
  status: string;
  note?: string | null;
}

export interface AmenityResponse {
  id: string;
  name: string;
  description?: string | null;
}

export interface PropertyListing extends PropertyResponse {
  images: string[];
  amenities: string[];
  landlord?: UserResponse;
}

export interface AreaResponse {
  id: string;
  name: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  roomCount: number;
  description?: string | null;
  landlordId: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface StoredSession {
  token: string;
  refreshToken?: string | null;
  user: AppUser;
}
