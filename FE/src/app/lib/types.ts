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
  createdAt: string;
  updatedAt?: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

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
  status: string;
  rejectionReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
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
  roomId: string;
  amenityId: string;
  amenityName: string;
  status: string;
  note?: string | null;
}

export interface PropertyListing extends PropertyResponse {
  images: string[];
  amenities: string[];
}

export interface StoredSession {
  token: string;
  refreshToken?: string | null;
  user: AppUser;
}
